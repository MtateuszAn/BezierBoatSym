using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Point : MonoBehaviour
{
    int index;
    public Vector2 position;
    public Path path;

    
    public void NewPoint(Vector2 newPosition,Path newPath,int i)
    {
        index = i;
        position = newPosition;
        path = newPath;
        this.transform.position = new Vector3(position.x,0,position.y);
    }

    public void MovePoint(Vector2 pos)
    {
        position = pos;
        this.transform.position = new Vector3(pos.x, 0f, pos.y);
    }

    public void MouseMovePoint(Vector3 newPosition)
    {
        path.MovePoint(index, new Vector2(newPosition.x, newPosition.z));
    }
}
