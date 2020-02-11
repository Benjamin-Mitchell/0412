using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    UIManager _UIManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif UNITY_ANDROID || UNITY_IOS
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
               Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Base"))
                {
                    _UIManager.EnableUI(hit.collider.gameObject.GetComponent<Base>());   
                }
            }
        }
    }
}
