using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Animator anim;

    private bool playDanceNext = true;
    private bool playSadNext = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // ----------- ANIMACIONES DE ACIERTO -----------
    public void PlayCorrectAnimations()
    {
        StopAllCoroutines();
        StartCoroutine(CorrectRoutine());
    }

    private IEnumerator CorrectRoutine()
    {
        if (playDanceNext)
        {
            anim.SetBool("isDancing", true);
            anim.SetBool("isEmo", false);
        }
        else
        {
            anim.SetBool("isDancing", false);
            anim.SetBool("isEmo", true);
        }

        playDanceNext = !playDanceNext;  // Alternar

        yield return new WaitForSeconds(1.5f);

        anim.SetBool("isDancing", false);
        anim.SetBool("isEmo", false);
    }

    // ----------- ANIMACIONES DE FALLO -----------
    public void PlayWrongAnimations()
    {
        StopAllCoroutines();
        StartCoroutine(WrongRoutine());
    }

    private IEnumerator WrongRoutine()
    {
        if (playSadNext)
        {
            anim.SetBool("isNo", true);
            anim.SetBool("isSad", false);
        }
        else
        {
            anim.SetBool("isNo", false);
            anim.SetBool("isSad", true);
        }

        playSadNext = !playSadNext;   // Alternar fallo

        yield return new WaitForSeconds(1.5f);

        anim.SetBool("isNo", false);
        anim.SetBool("isSad", false);
    }
}
