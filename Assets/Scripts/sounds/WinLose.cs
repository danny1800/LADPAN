using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLose : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip incorrectClip;

    public void PlayCorrect()
    {
        audioSource.PlayOneShot(correctClip);
    }

    public void PlayIncorrect()
    {
        audioSource.PlayOneShot(incorrectClip);
    }
}
