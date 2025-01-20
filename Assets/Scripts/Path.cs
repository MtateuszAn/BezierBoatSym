using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField, HideInInspector]
    List<Point> points;

    [SerializeField] Object anchor;
    [SerializeField] Object controll;
    [SerializeField] Object boatPrefab;

    GameObject boat = null;

    private void Start()
    {
        Vector2 center = new Vector2 (transform.position.x, transform.position.z);

        points = new List<Point>(){
        Instantiate(anchor).GetComponent<Point>(),
        Instantiate(controll).GetComponent<Point>(),
        Instantiate(controll).GetComponent<Point>(),
        Instantiate(anchor).GetComponent<Point>()
        };

        points[0].NewPoint(center + Vector2.left*2,this,0);
        points[1].NewPoint(center + (Vector2.left + Vector2.up) , this, 1);
        points[2].NewPoint(center + (Vector2.right + Vector2.down), this, 2);
        points[3].NewPoint(center + Vector2.right*2, this, 3);

        ClickBehaviour.playEvent.AddListener(OnPlayEvent);
    }

    void OnPlayEvent()
    {
        if(boat == null)
        {
            boat = (GameObject)Instantiate(boatPrefab);
            Vector3 start = new Vector3(points[0].position.x, 0, points[0].position.y);
            boat.GetComponent<Boat>().BoatStart(this);
            ClickBehaviour.boatList.Add(boat);
        }
        else
        {
            Object.Destroy(boat);
            boat = null;
        }
    }

    public Vector2 this[int i] { get { return points[i].position; } }
    public int NumPoints { get { return points.Count; } }

    public int NumSegments{ get { return (points.Count -4)/3+2; }}

    public void AddSegment(Vector2 anchorPos)
    {
        int i = NumPoints;
        points.Add(Instantiate(controll).GetComponent<Point>());
        points[points.Count-1].NewPoint(points[points.Count - 2].position * 2 - points[points.Count - 3].position, this, i);
        points.Add(Instantiate(controll).GetComponent<Point>());
        points[points.Count - 1].NewPoint((points[points.Count - 2].position + anchorPos) / 2, this, i + 1);
        points.Add(Instantiate(anchor).GetComponent<Point>());
        points[points.Count - 1].NewPoint(anchorPos, this, i + 2);
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        if (i == NumSegments-1)
        {
            float dstF = (points[1].position - points[0].position).magnitude;

            // Kierunek od `anchorIndex` do `i`, ale w przeciwn¹ stronê
            Vector2 dirF = (points[0].position - points[1].position).normalized;

            float dstL = (points[points.Count - 2].position - points[points.Count - 1].position).magnitude;

            // Kierunek od `anchorIndex` do `i`, ale w przeciwn¹ stronê
            Vector2 dirL = (points[points.Count - 1].position - points[points.Count - 2].position).normalized;

            return new Vector2[]
            {
                points[points.Count-1].position,
                points[points.Count-1].position + dirL * dstL,
                points[0].position + dirF * dstF,
                points[0].position
            };
        }

        return new Vector2[] {
            points[i * 3].position,
            points[i*3+1].position,
            points[i * 3 + 2].position,
            points[i * 3 + 3].position };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i].position;
        points[i].MovePoint(pos);

        if (i % 3 == 0)
        {
            //Anchor
            if(i+1 < points.Count)
                points[i+1].MovePoint(points[i+1].position+deltaMove);
            if(i-1>=0)
                points[i-1].MovePoint(points[i-1].position+deltaMove);
        }
        else
        {
            // Controll
            bool nextIsAnchor = (i + 1) % 3 == 0;
            int correspondingControllIndex = (nextIsAnchor) ? i + 2 : i - 2;
            int anchorIndex = (nextIsAnchor) ? i + 1 : i - 1;

            if(correspondingControllIndex >=0 && correspondingControllIndex < points.Count)
            {
                //float dst = (points[anchorIndex].position - points[correspondingControllIndex].position).magnitude;
                //Vector2 dir = (points[anchorIndex].position - pos).normalized;
                //points[correspondingControllIndex].MovePoint(points[anchorIndex].position + dir * dst);

                // Dystans miêdzy punktem `anchorIndex` a punktem `i`
                float dst = (points[i].position - points[anchorIndex].position).magnitude;

                // Kierunek od `anchorIndex` do `i`, ale w przeciwn¹ stronê
                Vector2 dir = (points[anchorIndex].position - points[i].position).normalized;

                // Przesuniêcie `correspondingControllIndex` po przeciwnej stronie
                points[correspondingControllIndex].MovePoint(points[anchorIndex].position + dir * dst);
            }
        }
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = .5f)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0].position);
        Vector2 previousPoint = points[0].position;
        float dstSinceLastPoint = 0;
        for (int segI = 0; segI < NumSegments; segI++)
        {
            Vector2[] p = GetPointsInSegment(segI);
            float controlNetLenght = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLenght / 2;
            int divisions = Mathf.CeilToInt( estimatedCurveLength * resolution * 10 );
            float t = 0;
            while(t<=1)
            {
                t += 1f/divisions;
                Vector2 pointOnCurve = Bezier.GetPointOnBezierCurve(p[0], p[1], p[2], p[3], t);
                dstSinceLastPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastPoint >= spacing)
                {
                    float overshotDst = dstSinceLastPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint-pointOnCurve).normalized * overshotDst;
                    evenlySpacedPoints.Add(new Vector3(newEvenlySpacedPoint.x,0, newEvenlySpacedPoint.y));
                    dstSinceLastPoint = overshotDst;

                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;

            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public Matrix4x4[] CalculateEvenlySpacedPointsMatrix4x4(float spacing, Vector3 scale, float resolution = .5f)
    {
        List<Matrix4x4> evenlySpacedPoints = new List<Matrix4x4>();
        //List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(Matrix4x4.TRS(new Vector3(points[0].position.x, 0, points[0].position.y), Quaternion.identity,scale));
        //evenlySpacedPoints.Add(points[0].position);
        Vector2 previousPoint = points[0].position;
        float dstSinceLastPoint = 0;
        for (int segI = 0; segI < NumSegments; segI++)
        {
            Vector2[] p = GetPointsInSegment(segI);
            float controlNetLenght = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLenght / 2;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.GetPointOnBezierCurve(p[0], p[1], p[2], p[3], t);
                dstSinceLastPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastPoint >= spacing)
                {
                    float overshotDst = dstSinceLastPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshotDst;
                    evenlySpacedPoints.Add(Matrix4x4.TRS(new Vector3(newEvenlySpacedPoint.x, 0, newEvenlySpacedPoint.y), Quaternion.identity, scale));
                    //evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastPoint = overshotDst;

                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;

            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public void EnablePoints()
    {
        foreach (Point p in points)
        { 
            p.GameObject().SetActive(true);
        }
    }
    public void DisablePoints() 
    { 
        foreach(var p in points)
        {
            p.GameObject().SetActive(false);
        }
    }
}
