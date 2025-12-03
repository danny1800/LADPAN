using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void PlayClick()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
