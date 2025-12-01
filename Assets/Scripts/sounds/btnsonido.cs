using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class btnsonido : MonoBehaviour
{
    public AudioSource sonido;
    public AudioClip clip;
    void Start()
    {
        sonido.clip = clip;
    }

    public void reproducir() 
    {
        sonido.Play();
    }
}
