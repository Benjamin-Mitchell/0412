using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsText : MonoBehaviour {

    public GameManager gameManager;

    private UnityEngine.UI.Text text;

	// Use this for initialization
	void Start () {
        text = GetComponent<UnityEngine.UI.Text>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
        //text.text = "Points: " + gameManager.Points.ToString();
	}
}
