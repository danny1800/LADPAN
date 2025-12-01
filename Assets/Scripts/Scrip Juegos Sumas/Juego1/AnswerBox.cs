using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerBox : MonoBehaviour
{
    public int myValue; // El número que muestra esta caja
    public TextMeshPro textDisplay; // Referencia al texto dentro de la caja
    private bool isCorrectAnswer; // ¿Es esta la caja buena?

    // Esta función la llamará el Spawner para configurar la caja
    public void SetupBox(int val, bool isCorrect)
    {
        myValue = val;
        isCorrectAnswer = isCorrect;
        // Actualizamos el texto visual de la caja
        if (textDisplay != null) textDisplay.text = myValue.ToString();
    }

    // Cuando algo con un Trigger (el jugador) entra en esta caja
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Avisamos al GameManager del resultado
            GameManagerS.instance.CheckAnswer(isCorrectAnswer);

            // Destruimos la fila completa de cajas (el padre de esta caja)
            // para que no choques con las otras dos.
            Destroy(transform.parent.gameObject);
        }
    }
}
