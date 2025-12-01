using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerS : MonoBehaviour
{
    // Singleton: permite acceder a este script usando GameManager.instance
    public static GameManagerS instance;

    [Header("UI References")]
    public TextMeshProUGUI questionTextUI;
    public TextMeshProUGUI boostStatusUI;

    [HideInInspector] public int currentCorrectAnswer; // La respuesta correcta actual

    void Awake()
    {
        // Configuración básica del Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Esta función la llamará la caja cuando el jugador la toque
    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            Debug.Log("¡Respuesta Correcta! TURBO ACTIVADO");
            // Buscar al jugador y activar su turbo
            FindObjectOfType<PlayerController>().ActivateBoost();
        }
        else
        {
            Debug.Log("Respuesta Incorrecta.");
            // Aquí podrías añadir una penalización, como reducir velocidad temporalmente.
        }

        // Opcional: Generar la siguiente pregunta inmediatamente
        FindObjectOfType<Spawner>().SpawnQuestionRow();
    }

    public void SetNewQuestionText(string text, int answer)
    {
        questionTextUI.text = text;
        currentCorrectAnswer = answer;
    }

    public void UpdateBoostUI(bool active)
    {
        if (boostStatusUI) boostStatusUI.text = active ? "TURBO: ON!" : "TURBO: OFF";
    }
}
