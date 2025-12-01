using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowMovement : MonoBehaviour
{
    [Header("Límites del movimiento")]
    public Transform puntoA;   // Izquierda
    public Transform puntoB;   // Derecha

    [Header("Movimiento")]
    public float speed = 2f;
    private bool movingToB = true;
    private bool canMove = true;

    [Header("Animaciones")]
    public Animator animator;
    public string animWalk = "Walk";          // Tu animación normal
    public string animGameOver = "WalkBox";   // CAMINA HACIA EL CUADRO (tu animación final)

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.Play(animWalk);
    }

    void Update()
    {
        if (!canMove) return;
        MoveBetweenPoints();
    }

    void MoveBetweenPoints()
    {
        Transform target = movingToB ? puntoB : puntoA;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            movingToB = !movingToB;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // LLAMADA DESDE GAMEMANAGER
    public void StopMovementAndPlayFinalAnimation()
    {
        canMove = false;
        animator.Play(animGameOver);
    }
}
