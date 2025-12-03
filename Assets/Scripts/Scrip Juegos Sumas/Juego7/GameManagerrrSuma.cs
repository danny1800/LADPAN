using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManagerrrSuma : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    // [0]=NumA, [1]=NumB
    public Text[] row1Texts;
    public DropSlot row1Slot; // Mantenemos DropSlot estándar según tu código

    [Header("UI Puntuación")]
    public Text scoreText;

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

    // Variables de estado
    private int currentLevel = 1;
    private int correctCount = 0;

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 30; // Puntos por completar las 3 sumas

    // ID ÚNICO PARA LA BASE DE DATOS
    private string gameID = "JuegoSumas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        // --- 1. CARGAR NIVEL DESDE LA BD ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            // Cargamos el nivel específico de "JuegoSumas"
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);

            // Si es 0 (nunca ha jugado), empezamos en 1
            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            Debug.Log("Modo Prueba: Nivel 1 (Sin usuario)");
            currentLevel = 1;
        }

        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // Limpiar los slots (Visual y Lógico)
        ResetSlot(row1Slot);
        ResetSlot(row2Slot);
        ResetSlot(row3Slot);

        List<int> correctAnswers = new List<int>();

        // Generar ecuaciones
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // Rellenar respuestas
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            // Trampas para sumas (números un poco más altos)
            int fakeMax = 20 + (currentLevel * 5);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

        // Mostrar fichas
        for (int i = 0; i < answerOptions.Length; i++)
        {
            answerOptions[i].numberValue = finalOptions[i];
            answerTexts[i].text = finalOptions[i].ToString();

            answerOptions[i].gameObject.SetActive(true);
            if (answerOptions[i].GetComponent<CanvasGroup>())
                answerOptions[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (answersParent) answerOptions[i].transform.SetParent(answersParent);

            answerOptions[i].transform.localScale = Vector3.one;
        }
    }

    // Helper para limpiar
    void ResetSlot(DropSlot slot)
    {
        slot.isFilled = false;
        if (slot.GetComponentInChildren<Text>())
            slot.GetComponentInChildren<Text>().text = "";
    }

    void SetupEquation(Text[] texts, DropSlot slot, List<int> answers)
    {
        // FORMULA DE DIFICULTAD PARA SUMAS:
        int maxNum = 8 + (currentLevel * 2);

        int numA = Random.Range(1, maxNum);
        int numB = Random.Range(1, maxNum);

        // --- LÓGICA DE SUMA ---
        int result = numA + numB;
        // -----------------------

        texts[0].text = numA.ToString();
        texts[1].text = numB.ToString();

        slot.expectedResult = result;
        answers.Add(result);
    }

    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;

            // Si completa las 3 sumas
            if (correctCount >= 3)
            {
                Debug.Log("¡Nivel Completado!");

                // 1. Puntos visuales
                currentScore += pointsPerLevel;
                UpdateScoreUI();

                // 2. Subir Nivel
                currentLevel++;

                // 3. --- GUARDAR PROGRESO EN BD ---
                // Guardamos puntos acumulados y el nuevo nivel
                SaveProgress(pointsPerLevel);

                Invoke("GenerateLevel", 1f);
            }
        }
        else
        {
            // AL PERDER
            // No guardamos nada para no ensuciar la base de datos con intentos fallidos
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Función Unificada de Guardado ---
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;

            // Guardamos: ID Alumno, ID Juego ("JuegoSumas"), Puntos a sumar, Nivel actual
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Sumas guardado: Nivel {currentLevel}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText) gameOverText.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar puntos de sesión y nivel
        currentScore = 0;
        UpdateScoreUI();

        // Opcional: Reiniciar nivel a 1 si quieres que sea difícil
        // currentLevel = 1;

        GenerateLevel();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + currentScore.ToString();
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