using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] float timeMod;
    [SerializeField] private float rotationSpeed = 5f;
    Path boatsPath;
    Vector3[] points;
    int i;
    float t;

    public void BoatStart(Path path)
    {
        boatsPath = path;
        points = path.CalculateEvenlySpacedPoints(.5f,1);
        points[0] = Vector3.Lerp(points[1], points[points.Length-1],0.5f);
        i= 1;
        t = 0;
        transform.position = points[0];
        transform.LookAt(points[1]);
        
    }

    private void Update()
    {
        if (boatsPath != null && points.Length != 0)
        {
            if(!FOVCheck())
            {
                Quaternion targetRotation = Quaternion.LookRotation(points[i] - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.LookAt(points[i + 1]);
                transform.position = Vector3.Lerp(points[i], points[i + 1], t);

                t += Time.deltaTime * timeMod;

                if (t >= 1)
                {
                    i++;
                    t = 0;
                }

                if (i >= points.Length - 1)
                {
                    i = 0;
                }
            }
        }

    }

    bool FOVCheck()
    {
        foreach (GameObject boat in ClickBehaviour.boatList)
        {
            Transform trans = boat.transform;
            // Oblicz dystans do obiektu
            float distance = Vector3.Distance(transform.position, trans.position);

            // Jeœli dystans jest wiêkszy ni¿ 1, pomiñ
            if (distance >= 3f)
                continue;

            // SprawdŸ, czy obiekt znajduje siê przed bie¿¹cym obiektem
            Vector3 directionToTrans = (trans.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTrans);

            // Jeœli iloczyn skalarny > 0, obiekt jest przed nim
            if (dotProduct > 0.8f)
            {
                return true;
            }
        }

        return false;
    }
}
