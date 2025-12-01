using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCPUMover : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Mover hacia adelante constantemente
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
