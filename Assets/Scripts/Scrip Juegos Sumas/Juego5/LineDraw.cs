using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{
    public LineRenderer line;
    public float minDistance = 0.005f;

    private int pointCount = 0;

    void Start()
    {
        line.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f)
            );

            // Agregamos el primer punto:
            if (pointCount == 0)
            {
                AddPoint(pos);
                return;
            }

            // Distancia entre el último y el actual
            float dist = Vector3.Distance(pos, line.GetPosition(pointCount - 1));

            if (dist >= minDistance)
            {
                AddPoint(pos);
            }
        }
    }

    void AddPoint(Vector3 point)
    {
        line.positionCount++;
        line.SetPosition(pointCount, point);
        pointCount++;
    }
}