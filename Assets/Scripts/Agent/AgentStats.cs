using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentStats : MonoBehaviour
{
	public string agentName;
	public Value resourceCollected;
	public Value distanceTravelled; //time alive?

    // Start is called before the first frame update
    void Start()
    {
		//Do load instead.
		agentName = possibleNames[Random.Range(0, possibleNames.Length)];
		resourceCollected = 0;
		distanceTravelled = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private string[] possibleNames =
	{ "Alexa", "Phil", "Colonel", "Daisy", "Forgone Conclusion", "Pain is infinite", "Dodger",
	"Graham", "Lamaril", "Kaladin", "Vin", "Tissinus", "Badger", "Gallon", "Hasbro", "Derek",
	"Barbie", "Jacks", "Villain", "Barry", "Gaston", "Boss", "The Don", "Killmonger", "Virtue",
	"X Æ A-12", "R2D4", "Shrek", "Capitulus", "Legolas", "Ronald", "Potter", "Baby Jesus",
	"Thorne", "Rose", "Jefe", "Doraemon", "SCARA", "Dave", "Sophia", "Chang'e 5",  "Apollo",
	"Atlas", "Thor", "Ablestar", "Agena", "Delta", "Scout", "Rehbar", "Saturn", "Titan",
	"Minuteman", "Centaur", "Burner", "Pegasus", "Taurus", "Falcon", "Electron", "Proton",
	"Neutron", "Aerobee", "Deacon", "Loki", "Hopi", "Apache", "Skylark", "Mesquito", "Javelin",
	"Antares", "Luffy", "Zoro", "Redstone", "Times New Roman", "Juno", "Junior", "Radeon",
	"Solomon", "Frodo", "Samwise", "Merry", "Pippin", "Mitchell", "Choo", "The Novelty",
	"The Binding", "Breaker", "Carbon", "Beikell", "Durex", "Jeeves", "Icey", "Beedle",
	"Woodhouse", "Obama", "Hope", "Don Quixote", "Gatsby", "Lord", "The Mockingbird", "Chandler",
	"Billy"};
	
}
