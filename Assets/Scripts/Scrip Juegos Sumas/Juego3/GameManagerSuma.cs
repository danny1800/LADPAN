using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerSuma : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    public Text[] row1Texts; // [0]=NumA, [1]=NumB
    public DropSlot row1Slot;

    public Text[] row2Texts;
    public DropSlot row2Slot;

    public Text[] row3Texts;
    public DropSlot row3Slot;

    [Header("Referencias UI Respuestas")]
    public DraggableItem[] answerOptions;
    public Text[] answerTexts;
    public Transform answersParent;

    [Header("UI Juego")]
    public GameObject gameOverText;

    // UI Puntos
    public Text scoreText;

    // Variables de estado
    private int currentLevel = 1;
    private int correctCount = 0;

    // Variables para el puntaje
    private int currentScore = 0;
    private int pointsPerLevel = 50; // Puntos por completar el nivel

    // ID ÚNICO PARA LA BASE DE DATOS
    private string gameID = "JuegoSumas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        // --- 1. CARGAR NIVEL DESDE LA BASE DE DATOS ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            // Pedimos el nivel guardado para este juego específico
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);

            // Si es la primera vez (nivel 0), forzamos nivel 1
            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            Debug.Log("Modo Prueba (Sin Usuario Logueado). Nivel 1.");
            currentLevel = 1;
        }

        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // Limpiar slots (Visual y Lógico)
        ResetSlot(row1Slot);
        ResetSlot(row2Slot);
        ResetSlot(row3Slot);

        List<int> correctAnswers = new List<int>();

        // Crear Ecuaciones de SUMA
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // Rellenar respuestas (Correctas + Distractores)
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            // Sumas pueden dar números más altos, ajustamos el rango del distractor
            int fakeMax = 20 + (currentLevel * 5);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

        // Asignar a la UI
        for (int i = 0; i < answerOptions.Length; i++)
        {
            answerOptions[i].numberValue = finalOptions[i];
            answerTexts[i].text = finalOptions[i].ToString();

            answerOptions[i].gameObject.SetActive(true);

            if (answerOptions[i].GetComponent<CanvasGroup>())
                answerOptions[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (answersParent != null) answerOptions[i].transform.SetParent(answersParent);
            answerOptions[i].transform.localScale = Vector3.one;
        }
    }

    // Función auxiliar para limpiar
    void ResetSlot(DropSlot slot)
    {
        slot.isFilled = false;
        if (slot.GetComponentInChildren<Text>()) slot.GetComponentInChildren<Text>().text = "";
    }

    void SetupEquation(Text[] texts, DropSlot slot, List<int> answers)
    {
        // DIFICULTAD SUMAS:
        int maxNum = 5 + (currentLevel * 2);

        int numA = Random.Range(1, maxNum);
        int numB = Random.Range(1, maxNum);

        int result = numA + numB; // LÓGICA DE SUMA

        if (texts.Length > 1)
        {
            texts[0].text = numA.ToString();
            texts[1].text = numB.ToString();
        }

        slot.expectedResult = result;
        answers.Add(result);
    }

    // Esta función debe ser llamada por tu DropSlot cuando sueltan la ficha
    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;

            // Si completa las 3 sumas
            if (correctCount >= 3)
            {
                Debug.Log("¡Nivel Completado!");

                // 1. Sumar puntos visuales
                currentScore += pointsPerLevel;
                UpdateScoreUI();

                // 2. Subir nivel
                currentLevel++;

                // 3. --- GUARDAR PROGRESO (Nivel y Puntos) ---
                SaveProgress(pointsPerLevel);

                Invoke("GenerateLevel", 1f);
            }
        }
        else
        {
            // AL PERDER
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Función unificada para guardar ---
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;

            // Guardamos ID Alumno, ID Juego ("JuegoSumas"), Puntos a sumar y Nivel actual
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Sumas guardado: Nivel {currentLevel}");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + currentScore.ToString();
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText) gameOverText.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar puntos de sesión y volver a generar
        currentScore = 0;
        UpdateScoreUI();

        // currentLevel = 1; // Descomenta si quieres que vuelvan al inicio al perder

        GenerateLevel();
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}