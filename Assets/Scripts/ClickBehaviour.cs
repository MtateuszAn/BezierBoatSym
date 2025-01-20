using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using static UnityEditor.PlayerSettings;
using UnityEngine.UI;
using UnityEngine.Events;

public class ClickBehaviour : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public static bool playMode = false;
    [SerializeField] GameObject pathOBJ;
    [SerializeField] LayerMask layerMask;
    [SerializeField] LayerMask all;
    [SerializeField] LayerMask hidePoints;

    public static List<GameObject> boatList = new List<GameObject>();

    public static UnityEvent playEvent = new UnityEvent();

    //[SerializeField] Path selectedPath;

    List<Path> paths = new List<Path>();
    int pathIndex=-1;

    Point selectedPoint = null;

    Ray ray;
    RaycastHit hit;

    // Update is called once per frame
    void Update()
    {
        if (!playMode)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //mouse button down
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 1000f))
            {
                Vector3 pos = hit.point;
                pos.y = 0;
                if (hit.collider.tag == "Plane")
                {
                    if(pathIndex==-1)
                    {
                        Path path = Instantiate(pathOBJ,pos,Quaternion.identity).GetComponent<Path>();
                        paths.Add(path);
                        ChangeIndex(paths.Count - 1);
                    }
                    else
                    {
                        //selectedPath.AddSegment(new Vector2(pos.x, pos.z));
                        if(pathIndex>=0&& pathIndex<paths.Count)
                            paths[pathIndex].AddSegment(new Vector2(pos.x, pos.z));
                    }
                  
                }
                else if (hit.collider.tag == "Point")
                {
                    selectedPoint = hit.transform.gameObject.GetComponent<Point>();
                }
            }

            //mouse button hold
            if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit, 1000f, layerMask))
            {
                Vector3 pos = hit.point;
                pos.y = 0;

                if (selectedPoint != null)
                {
                    Debug.Log("Drag Point");
                    selectedPoint.MouseMovePoint(pos);
                }
            }

            //mouse button relise 
            if (Input.GetMouseButtonUp(0))
            {
                selectedPoint = null;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!playMode)
            {
                boatList.Clear();
                playMode = true;
                text.text = "PLAY";
                Debug.Log("PLAY");
                this.GetComponent<Camera>().cullingMask = hidePoints;
                playEvent.Invoke();
            }
            else
            {
                boatList.Clear();
                playMode = false;
                text.text = "EDIT";
                Debug.Log("PLAY");
                this.GetComponent<Camera>().cullingMask = all;
                playEvent.Invoke();
            }
                
        }

        if (!playMode && Input.GetKeyDown(KeyCode.Tab) && paths.Count >0)
        {
            ChangeIndex(pathIndex+1);
        }
    }

    void ChangeIndex(int i)
    {
        if (pathIndex >= 0)
            paths[pathIndex].DisablePoints();

        pathIndex = i;

        if (pathIndex >= paths.Count)
        {
            pathIndex = -1;
            text.text = "ADD";
        }
        else
        {
            paths[pathIndex].EnablePoints();
            text.text = "EDIT";
        }
    }
}
