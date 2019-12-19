using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Aim of this class:
// Describe the trilateral movement of an agent across a 3D grid, with obstacle avoidance.

// Initial implementation only works for uniform size colliders (cube, sphere etc)
public class Pathfinder : MonoBehaviour
{
    /// <summary>
    ///  Grid Bits
    /// </summary>
    class Node
    {
        public int x, y, z;
        public Vector3 pos;
        public bool occupied;
        public float f, g, h;
        public Node parent;
    }


    public static int gridSize = 500;
    public static float nodeSize = 1.0f;
    public static Vector3 centrePos = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 gridBottomCorner;

    List<GameObject> obstacles;
    List<GameObject> agents;
    
    Node[,,] grid = new Node[gridSize, gridSize, gridSize];

    /// <summary>
    /// A_Star bits
    /// </summary>


    public List<Vector3> requestPath(Vector3 from, Vector3 to)
    {
        Node fromNode = ReturnNodeFromVector3(from);
        Node toNode = ReturnNodeFromVector3(to);
        List<Node> nodePath = FindPath(fromNode, toNode);

        List<Vector3> vector3Path = new List<Vector3>();
        for(int i = 0; i < nodePath.Count; i++)
        {
            vector3Path.Add(nodePath[i].pos);
        }

        return vector3Path;
    }


    List<Node> FindPath(Node from, Node to)
    {
        List<Node> open = new List<Node>();
        List<Node> visited = new List<Node>(); ;
        Node current;

        current = from;
        open.Add(current);
        //open.Clear();
        //open.AddRange(grid);

        int failSafe = 0;
        while(open.Count > 0 && failSafe < 10000)
        {
            if (current.Equals(to))
            {
                List<Node> returnNodes = new List<Node>();
                // check this
                while (current.parent != null && failSafe < 10000)
                {
                    returnNodes.Add(current);
                    current = current.parent;
                    failSafe++;
                }

                return returnNodes;
            }

            current = open[0];
            open.Remove(current);
            visited.Add(current);

            List<Node> neighbours = GetValidNeighbours(current);

            for(int i = 0; i < neighbours.Count; i++)
            {
                // skip if aleady ruled out.
                if (visited.Contains(neighbours[i]))
                    continue;

                float g = neighbours[i].g + nodeSize;
                float h = manhattanHeuristic(neighbours[i], to);
                float f = g + h;

                bool inOpen = open.Contains(neighbours[i]);

                if(!inOpen || (inOpen && (neighbours[i].f > f)))
                {
                    if (!inOpen)
                        open.Add(neighbours[i]);

                    neighbours[i].f = f;
                    neighbours[i].g = g;
                    neighbours[i].h = h;
                    neighbours[i].parent = current;
                }
                

            }

            failSafe++;
        }


        Debug.Log("WARNING: Destination could not be found for path find!!");
        return null;
    }

    float manhattanHeuristic(Node from, Node to)
    {
        return Vector3.Distance(from.pos, to.pos);
    }

    List<Node> GetValidNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        int i = 0;
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    // skip current node
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    int newNodeX = node.x + x;
                    int newNodeY = node.y + y;
                    int newNodeZ = node.z + z;

                    // don't add off-screen nodes
                    if (newNodeX < 0 || newNodeY < 0 || newNodeZ < 0
                            || newNodeX > gridSize || newNodeY > gridSize || newNodeZ > gridSize)
                        continue;

                    // don't add occupied nodes
                    if (grid[newNodeX, newNodeY, newNodeZ].occupied)
                        continue;

                    neighbours.Add(grid[node.x + x, node.y + y, node.z + z]);
                }
            }
        }

        return neighbours;
    }




    void Awake()
    {
        transform.position = centrePos;

        //inital population of lists.
        obstacles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Obstacle"));
        agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));


        gridBottomCorner = centrePos - (new Vector3(gridSize / 2.0f, gridSize / 2.0f, gridSize / 2.0f) * nodeSize);

        //initiate grid
        InitiateGrid();

        //populate grid with inital objects.
        UpdateGridWithObstacles();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitiateGrid()
    {
        for(int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    // positions
                    if (grid[x, y, z].pos.x > 10.0f)
                        Debug.Log("cool");

                    grid[x, y, z].pos.x = gridBottomCorner.x + (x * nodeSize);
                    grid[x, y, z].pos.y = gridBottomCorner.y + (y * nodeSize);
                    grid[x, y, z].pos.z = gridBottomCorner.z + (z * nodeSize);

                    // indices
                    grid[x, y, z].x = x;
                    grid[x, y, z].y = y;
                    grid[x, y, z].z = z;
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

            Vector3 colBotCorner = pos - (colSize / 2.0f);

            Vector3 colTopCorner = pos + (colSize / 2.0f);

            //TEST: pos = 6.6, blah, blah
            //TEST: nodeSize = 1.0f

            //find bottom corner node
            Vector3 nodeBotCorner = colBotCorner - new Vector3(colBotCorner.x % nodeSize, colBotCorner.y % nodeSize, colBotCorner.z % nodeSize);
            Vector3 nodeTopCorner = colTopCorner - new Vector3(colTopCorner.x % nodeSize, colTopCorner.y % nodeSize, colTopCorner.z % nodeSize);

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
            for(int x = botNodeX; x < TopNodeX; x++)
            {
                for (int y = botNodeY; y < TopNodeY; y++)
                {
                    for (int z = botNodeZ; z < TopNodeZ; z++)
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
    Node ReturnNodeFromVector3(Vector3 vec)
    {
        Vector3 nodePos = vec - new Vector3(vec.x % nodeSize, vec.y % nodeSize, vec.z % nodeSize);
        int nodeX = (int)(nodePos.x / nodeSize);
        int nodeY = (int)(nodePos.y / nodeSize);
        int nodeZ = (int)(nodePos.z / nodeSize);

        return grid[nodeX, nodeY, nodeZ];
    }
}
