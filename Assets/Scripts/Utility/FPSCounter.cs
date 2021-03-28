using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	Text text;

	int[] rollingValue = new int[300];
	int globalFrames = 0;
	int currentFrame = 0;

    // Start is called before the first frame update
    void Start()
    {
		text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
		float fpsFloat = (1f / Time.unscaledDeltaTime);
		int fps = (int)fpsFloat;
		rollingValue[currentFrame] = fps;
		currentFrame = (currentFrame == 299) ? 0 : ++currentFrame;

		globalFrames = (globalFrames < 300) ? ++globalFrames : 300;
		int rollingAverage = 0;
		for(int i = 0; i < globalFrames; i++)
		{
			rollingAverage += rollingValue[i];
		}
		rollingAverage /= (globalFrames + 1);

		text.text = "FPS: " + fps + "          Avg. FPS: " + rollingAverage;
    }
}
