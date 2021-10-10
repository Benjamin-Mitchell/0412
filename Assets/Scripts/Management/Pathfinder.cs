using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

//Currently this class is duplicated on every agent. This may become a memory issue on many Agents, as we will have
//a copy of the Node grid on each of them. If that is the case, may need to re-investigate using a single Pathfinder
//object that all of the agents borrow from. This could result in queueing for accces to variables (e.g. the open set) from threads.

//Aim of this class:
// Describe the trilateral movement of an agent across a 3D grid, with obstacle avoidance.

// Initial implementation only works for uniform size colliders (cube, sphere etc)
public class Pathfinder : MonoBehaviour
{
	struct PathfindResult
	{
		public AgentPathfinder agent;
		public List<Vector3> path;

		public PathfindResult(AgentPathfinder agent, List<Vector3> path)
		{
			this.agent = agent;
			this.path = path;
		}
	}
	Queue<PathfindResult> results = new Queue<PathfindResult>();
    /// <summary>
    ///  Grid Bits
    /// </summary>
    public class Node
    {
        public struct IntVector3
        {
            public int x, y, z;

            // customs operator, because c# .Equals() is slow.
            public static bool operator == (IntVector3 a, IntVector3 b)
            {
                bool status = false;
                status = (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
                return status;
            }

            public static bool operator !=(IntVector3 a, IntVector3 b)
            {
                bool status = false;
                if ((a.x != b.x) || (a.y != b.y) || (a.z != b.z))
                    status = true;
                return status;
            }
        }

        public IntVector3 intVec3;

        public Vector3 pos;
        public bool occupied;

        // booleans replace open and closed lists, as checking lists was too underperformant.
        public bool open;
        public bool visited;

        public float f, g, h;
        public Node parent;

        public void clear()
        {
            f = g = h = 0;
            parent = null;
            open = false;
            visited = false;
        }
    }


    //public static int gridSize = 10;
    public static float nodeSize = 1.0f;
    public static Vector3 centrePos;
    private  Vector3 gridBottomCorner;

    List<GameObject> obstacles;

    int mapX, mapY, mapZ;
    Node[,,] grid;

    //pre-initialize lists, to remove GC overhead.
	//TODO: sync me across threads!!
    List<Node> openNodes;
    List<Node> returnNodes;
    List<Node> neighbours;
    List<Vector3> vector3Path;

    /// <summary>
    /// A_Star bits
    /// </summary>


    public void requestPath(Vector3 from, Vector3 to, AgentPathfinder agent)
    {
        Node fromNode = ReturnNodeFromVector3(from);

        int X = mapX;
        int Y = mapY;
        int Z = mapZ;


        if (to.x > X || to.x < .0f
            || to.y > Y || to.y < .0f
            || to.z > Z || to.z < .0f)
        {
            Debug.Log("Demanded an out of range vector destination!");
			PathfindResult result;
			result.agent = agent;
			return;
        }

        Node toNode = ReturnNodeFromVector3ToNode(from, to);

		Thread t = new Thread(() => FindPath(fromNode, toNode, to, agent));
		t.Start();

		//ThreadStart threadStart = delegate
		//{
		//	FindPath(fromNode, toNode, to, agent);
		//};
    }

	public void PathFound(List<Node> nodePath, AgentPathfinder agent, Vector3 to)
	{
		if (nodePath == null)
		{
			PathfindResult result = new PathfindResult(agent, null);
			lock(results){ results.Enqueue(result); }
			return;
		}

		vector3Path.Clear();
		
		//invert path and convert to Vector3
		// i > 0 as we don't want to add last one, see below comment 
		for (int i = nodePath.Count - 1; i > 0; i--)
		{
			vector3Path.Add(nodePath[i].pos);
		}
		
		//last one should be exact position, not node position.
		vector3Path.Add(to);

		PathfindResult successfulResult = new PathfindResult(agent, vector3Path);
		lock (results) { results.Enqueue(successfulResult); }
	}

    //This function is broken up to allow for easier profiling with Unity tools.
    void FindPath(Node from, Node to, Vector3 finalTo, AgentPathfinder agent)
    {
        Node current = FindPathInit(from);

        if (CheckReached(current, to))
        {
            PathFound(returnNodes, agent, finalTo);
            return;
        }

        while (openNodes.Count > 0)
        {
            current.f = current.g = current.h = 0;
			
            current = openNodes[0];
            openNodes.RemoveAt(0);

            if (CheckReached(current, to))
            {
                PathFound(returnNodes, agent, finalTo);
                return;
            }

            if (current == null)
				Debug.Log("How is current null?!");


            current.open = false;
            current.visited = true;

            

            // check neighbours
            for (int x = -1; x < 2; x++)
            {
				if (current.intVec3.x + x < 0 || current.intVec3.x + x >= mapX)	// do these checks early in the loop to ensure we don't waste time checking an entire row of null neighbours
					continue;
                for (int y = -1; y < 2; y++)
                {
					if (current.intVec3.y + y < 0 || current.intVec3.y + y >= mapY)
						continue;
					for (int z = -1; z < 2; z++)
                    {
						if (current.intVec3.z + z < 0 || current.intVec3.z + z >= mapZ)
							continue;


						Node neighbour = null;
                        // check its a valid neighbour


                        neighbour = CheckNeighbourValidity(x, y, z, current);

                        if (neighbour == null)
                            continue;

                        float g = neighbour.g + nodeSize;
                        float h = manhattanHeuristic(neighbour, to);
                        float f = g + h;
						
                        bool inOpen = neighbour.open;
						
                        AddToOpenSet(inOpen, neighbour, f, g, h, current);

                    }
                }
            }
        }


        Debug.Log("WARNING: Destination could not be found for path find!!");
		PathFound(null, agent, finalTo);
	}

    Node FindPathInit(Node from)
    {
        openNodes.Clear();
        Node current;

        current = from;
        openNodes.Add(current);

        foreach (Node node in grid)
        {
            node.clear();
        }

        return current;
    }
    
    bool CheckReached(Node current, Node to)
    {
        if (current.intVec3 == to.intVec3)
        {
            returnNodes.Clear();
            // check this
            while (current.parent != null)
            {
                returnNodes.Add(current);
                current = current.parent;
            }

            return true;
        }
        return false;
    }
    
    Node CheckNeighbourValidity(int x, int y, int z, Node current)
    {
        // skip current node
        if (x == 0 && y == 0 && z == 0)
            return null;

        //neighbour position
        int newNodeX = current.intVec3.x + x;
        int newNodeY = current.intVec3.y + y;
        int newNodeZ = current.intVec3.z + z;

        // don't add occupied nodes
        if (grid[newNodeX, newNodeY, newNodeZ].occupied)
            return null;

        if (grid[newNodeX, newNodeY, newNodeZ].visited)
            return null;

        return grid[newNodeX, newNodeY, newNodeZ];
    }

    void AddToOpenSet(bool inOpen, Node toAdd, float f, float g, float h, Node current)
    {
        if (!inOpen || (inOpen && (toAdd.f > f)))
        {
            toAdd.f = f;
            toAdd.g = g;
            toAdd.h = h;
            toAdd.parent = current;

            if (!inOpen)
            {
                toAdd.open = true;

				lock (openNodes) {
                    for (int i = 0; i < openNodes.Count; i++)
                    {
                        if(toAdd.f < openNodes[i].f)
						{
                            openNodes.Insert(i, toAdd);
                            return;
						}
                    }
                    openNodes.Add(toAdd); };
            }

            
        }
    }


    float manhattanHeuristic(Node from, Node to)
    {
        return Vector3.Distance(from.pos, to.pos);
    }

    List<Node> GetValidNeighbours(Node node)
    {
        neighbours.Clear();
        
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    // skip current node
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    int newNodeX = node.intVec3.x + x;
                    int newNodeY = node.intVec3.y + y;
                    int newNodeZ = node.intVec3.z + z;

                    // don't add off-screen nodes
                    if (newNodeX < 0 || newNodeY < 0 || newNodeZ < 0
                            || newNodeX >= mapX || newNodeY >= mapY || newNodeZ >= mapZ)
                        continue;

                    // don't add occupied nodes
                    if (grid[newNodeX, newNodeY, newNodeZ].occupied)
                        continue;

                    neighbours.Add(grid[node.intVec3.x + x, node.intVec3.y + y, node.intVec3.z + z]);
                }
            }
        }

        return neighbours;
    }


    void Awake()
    {
        mapX = GameManager.Instance.mapX;
        mapY = GameManager.Instance.mapY;
        mapZ = GameManager.Instance.mapZ;
        grid = new Node[mapX, mapY, mapZ];

        // Assumption made that map will never be taller (Y) than it is wide/long (X/Z).
        int maxGridTraversal = mapX + mapZ;
        openNodes = new List<Node>(maxGridTraversal);
        returnNodes = new List<Node>(maxGridTraversal);
        vector3Path = new List<Vector3>(maxGridTraversal);

        int maxNeighbours = 26;
        neighbours = new List<Node>(maxNeighbours);

        //transform.position = centrePos;

        //inital population of lists.
        obstacles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Obstacle"));

        //gridBottomCorner = centrePos - (new Vector3(gridSize / 2.0f, gridSize / 2.0f, gridSize / 2.0f) * nodeSize);
        gridBottomCorner = Vector3.zero;

		centrePos = new Vector3(gridBottomCorner.x + (mapX / 2.0f), gridBottomCorner.y + (mapY / 2.0f), gridBottomCorner.z + (mapZ / 2.0f));

		//initiate grid
		InitiateGrid();

        //populate grid with inital objects.
        UpdateGridWithObstacles();
    }


    // Start is called before the first frame update
    void Start()
    {
        

        //JUST FOR TESTING
        //drawGrid();
    }

    // Update is called once per frame
    void Update()
    {
		//update objects when a thread has finished processing their path.
		lock (results)
		{
			while(results.Count > 0)
			{
				PathfindResult res = results.Dequeue();
				res.agent.NotifyPathAvailable(res.path);
			}
		}

#if UNITY_EDITOR
		drawOuterGrid();
#endif
	}

	void InitiateGrid()
    {
        for(int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                for (int z = 0; z < mapZ; z++)
                {

                    grid[x, y, z] = new Node();
                    // positions


                    float halfNodeSize = nodeSize / 2;

                    grid[x, y, z].pos.x = gridBottomCorner.x + (x * nodeSize) + halfNodeSize;
                    grid[x, y, z].pos.y = gridBottomCorner.y + (y * nodeSize) + halfNodeSize;
                    grid[x, y, z].pos.z = gridBottomCorner.z + (z * nodeSize) + halfNodeSize;

                    // indices
                    grid[x, y, z].intVec3.x = x;
                    grid[x, y, z].intVec3.y = y;
                    grid[x, y, z].intVec3.z = z;
                }
            }
        }

    }

    void UpdateGridWithObstacles()
    {
        foreach (GameObject obj in obstacles)
        {
            // transform 
            Vector3 pos = obj.transform.position;

            // get bottom corner of collider
            Vector3 colSize = obj.GetComponent<colliderManager>().size;

            //CORRECT
            Vector3 colBotCorner = pos - (colSize / 2.0f);
            Vector3 colTopCorner = pos + (colSize / 2.0f);

            //TEST: pos = 6.6, blah, blah
            //TEST: nodeSize = 1.0f

            //find bottom corner node
            Vector3 nodeBotCorner = colBotCorner - new Vector3(colBotCorner.x % nodeSize, colBotCorner.y % nodeSize, colBotCorner.z % nodeSize);
            Vector3 nodeTopCorner = colTopCorner - new Vector3(colTopCorner.x % nodeSize, colTopCorner.y % nodeSize, colTopCorner.z % nodeSize);

            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.position = nodeBotCorner;
            //cube.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            //
            //GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube2.transform.position = nodeTopCorner;
            //cube2.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            //TEST: nodeBotCorner = 6.6 - 0.6 = 6.0
            int botNodeX = (int)(nodeBotCorner.x / nodeSize);
            int botNodeY = (int)(nodeBotCorner.y / nodeSize);
            int botNodeZ = (int)(nodeBotCorner.z / nodeSize);

            int TopNodeX = (int)(nodeTopCorner.x / nodeSize);
            int TopNodeY = (int)(nodeTopCorner.y / nodeSize);
            int TopNodeZ = (int)(nodeTopCorner.z / nodeSize);

            //TEST: nodeX = (int)6.0 / 1.0f = 6
            //Node bottomNode = grid[nodeX, nodeY, nodeZ];

            // Now need to cover all boxes covered by that (currently only correct function on boxColliders)
            for(int x = botNodeX; x <= TopNodeX; x++)
            {
                for (int y = botNodeY; y <= TopNodeY; y++)
                {
                    for (int z = botNodeZ; z <= TopNodeZ; z++)
                    {
                        grid[x, y, z].occupied = true;
                    }
                }
            }

            // can use the collider's size to determine which grid cells the obstacle overlaps
            // then update the grid accordingly.


        }
    }

    // helper function for accessing nodes using Vector3 positions.
    // MAX VALUE CANNOT EQUAL OR EXCEED MAP SIZE
    Node ReturnNodeFromVector3(Vector3 vec)
    {
        Vector3 nodePos = vec - new Vector3(vec.x % nodeSize, vec.y % nodeSize, vec.z % nodeSize);

        // -1 for offset into 0 indexed array.
        int nodeX = (int)(nodePos.x / nodeSize);
        int nodeY = (int)(nodePos.y / nodeSize);
        int nodeZ = (int)(nodePos.z / nodeSize);

		if (nodeX >= mapX)
			nodeX = mapX-1;

		if (nodeY >= mapY)
			nodeY = mapY-1;

		if (nodeZ >= mapZ)
			nodeZ = mapZ-1;

		if (nodeX < 0)
			nodeX = 0;

		if (nodeY < 0)
			nodeY = 0;

		if (nodeZ < 0)
			nodeZ = 0;

        Node ret = grid[nodeX, nodeY, nodeZ];

        if (ret == null)
            Debug.Log("ReturnNodeFromVector3 shit the bed. Node: (" + nodeX + ", " + nodeY + ", " + nodeZ + ")");

        return ret;
    }

    //This function exists to give a better approximation of what node should be used as the last node.
    //The final "to" position will be a 3d Vector position surrounded by a group of nodes.
    //We want to pick the node closest to the starting position (the one most in the same direction as where the agent will be coming from)
    Node ReturnNodeFromVector3ToNode(Vector3 from, Vector3 to)
    {
        Vector3 nodePos = to - new Vector3(to.x % nodeSize, to.y % nodeSize, to.z % nodeSize);

        Vector3 direction = from - to;

        // -1 for offset into 0 indexed array.
        int nodeX = (int)(nodePos.x / nodeSize);
        int nodeY = (int)(nodePos.y / nodeSize);
        int nodeZ = (int)(nodePos.z / nodeSize);

        if (direction.x < 0)
            nodeX--;

        if (direction.y < 0)
            nodeY--;

        if (direction.z < 0)
            nodeZ--;

        if (nodeX >= mapX)
            nodeX = mapX - 1;

        if (nodeY >= mapY)
            nodeY = mapY - 1;

        if (nodeZ >= mapZ)
            nodeZ = mapZ - 1;

        if (nodeX < 0)
            nodeX = 0;

        if (nodeY < 0)
            nodeY = 0;

        if (nodeZ < 0)
            nodeZ = 0;

        Node ret = grid[nodeX, nodeY, nodeZ];

        if (ret == null)
            Debug.Log("ReturnNodeFromVector3 shit the bed. Node: (" + nodeX + ", " + nodeY + ", " + nodeZ + ")");

        return ret;
    }

    /////////////////////////////////////////////
    ///JUST FOR TESTING
    /////////////////////////////////////////////
    void drawGrid()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        var verticies = new List<Vector3>();

        var indicies = new List<int>();
        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                for (int z = 0; z < mapZ; z++)
                {                    
                    if (grid[x, y, z].occupied)
                    {
                        float halfNodeSize = nodeSize / 2;
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = grid[x, y, z].pos;
                        cube.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    }
                }
            }
        }
    }

	void drawOuterGrid()
	{

		float x0 = centrePos.x - ((float)mapX / 2.0f);
		float y0 = centrePos.y - ((float)mapY / 2.0f);
		float z0 = centrePos.z - ((float)mapZ / 2.0f);

		float x1 = centrePos.x + ((float)mapX / 2.0f);
		float y1 = centrePos.y + ((float)mapY / 2.0f);
		float z1 = centrePos.z + ((float)mapZ / 2.0f);



		for (int x = 0; x < mapX; x++)
		{
			float xpos = x0 + x;
			
			//bottom
			Vector3 start = new Vector3(xpos, y0, z0);
			Vector3 end = new Vector3(xpos, y0, z1);

			//top
			start = new Vector3(xpos, y1, z0);
			end = new Vector3(xpos, y1, z1);
			Debug.DrawLine(start, end);
		}

		for (int y = 0; y < mapY; y++)
		{
			float ypos = y0 + y;
			//front
			Vector3 start = new Vector3(x0, ypos, z0);
			Vector3 end = new Vector3(x1, ypos, z0);
			Debug.DrawLine(start, end);

			//back
			start = new Vector3(x0, ypos, z1);
			end = new Vector3(x1, ypos, z1);
			Debug.DrawLine(start, end);

			//left
			start = new Vector3(x0, ypos, z0);
			end = new Vector3(x0, ypos, z1);
			Debug.DrawLine(start, end);

			//right
			start = new Vector3(x1, ypos, z0);
			end = new Vector3(x1, ypos, z1);
			Debug.DrawLine(start, end);
		}
	}
}
