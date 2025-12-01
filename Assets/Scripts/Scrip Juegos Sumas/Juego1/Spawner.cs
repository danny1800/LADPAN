using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
public class Spawner : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject answerRowPrefab; // El prefab que contiene las 3 cajas
    public float spawnInterval = 5f; // Cada cuánto tiempo aparece una nueva fila
    private float timer;

    [Header("Referencia al Jugador")]
    public Transform playerTransform; // Para saber dónde generar las cajas (siempre delante del jugador)
    public float spawnDistanceAhead = 15f; // Distancia delante del jugador

    void Start()
    {
        // Generar la primera pregunta al empezar
        SpawnQuestionRow();
    }

    void Update()
    {
        // Temporizador para generar preguntas automáticamente si no se responden rápido
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnQuestionRow();
            timer = 0;
        }
    }

    public void SpawnQuestionRow()
    {
        timer = 0; // Reset timer

        // 1. Generar la operación matemática
        int numA = Random.Range(2, 10); // Números entre 2 y 9 para que no sea muy fácil (x1) o muy difícil
        int numB = Random.Range(2, 10);
        int correctAnswer = numA * numB;
        string questionString = numA + " x " + numB;

        // Enviar la pregunta al GameManager para mostrarla en la UI
        GameManagerS.instance.SetNewQuestionText(questionString, correctAnswer);

        // 2. Calcular dónde spawnear
        // Calculamos una posición Y delante del jugador actual
        Vector3 spawnPos = new Vector3(0, playerTransform.position.y + spawnDistanceAhead, 0);

        // Instanciar el prefab de la fila de respuestas
        GameObject newRow = Instantiate(answerRowPrefab, spawnPos, Quaternion.identity);

        // 3. Configurar las cajas dentro de la fila
        // Obtenemos los 3 scripts "AnswerBox" hijos del prefab
        AnswerBox[] boxes = newRow.GetComponentsInChildren<AnswerBox>();

        // Elegir al azar cuál de las 3 cajas (0, 1 o 2) tendrá la respuesta correcta
        int correctBoxIndex = Random.Range(0, 3);

        // Lista para guardar respuestas usadas y evitar duplicados
        List<int> usedAnswers = new List<int>();
        usedAnswers.Add(correctAnswer);

        for (int i = 0; i < boxes.Length; i++)
        {
            if (i == correctBoxIndex)
            {
                // Esta es la caja correcta
                boxes[i].SetupBox(correctAnswer, true);
            }
            else
            {
                // Esta es una caja incorrecta, generar número falso único
                int wrongAnswer;
                do
                {
                    // Generar una respuesta falsa cercana a la real para despistar
                    wrongAnswer = correctAnswer + Random.Range(-10, 10);
                }
                while (usedAnswers.Contains(wrongAnswer) || wrongAnswer <= 0); // Repetir si ya existe o es negativo

                usedAnswers.Add(wrongAnswer);
                boxes[i].SetupBox(wrongAnswer, false);
            }
        }
    }
}
