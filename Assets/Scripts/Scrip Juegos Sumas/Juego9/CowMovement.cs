using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowMovement : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip deathSound;

    // Esta es la ÚNICA función que debe existir
    public void PlayLoseAnimation()
    {
        // Animación de sacar lengua
        if (animator != null)
            animator.SetTrigger("Lose");

        // Sonido final
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);
    }
}
