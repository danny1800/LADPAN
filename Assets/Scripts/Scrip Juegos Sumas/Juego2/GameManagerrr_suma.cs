using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManagerrr_suma : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    public Text[] row1Texts;
    public DropSlot_SUMA row1Slot;

    [Header("UI Puntuación")]
    public Text scoreText;

    public Text[] row2Texts;
    public DropSlot_SUMA row2Slot;

    public Text[] row3Texts;
    public DropSlot_SUMA row3Slot;

    [Header("Referencias UI Respuestas")]
    public DraggableItem_SUMA[] answerOptions;
    public Text[] answerTexts;
    public Transform answersParent;

    [Header("UI Juego")]
    public GameObject gameOverText;

    // Variables internas
    private int currentLevel = 1;
    private int correctCount = 0;

    // Variables de Puntuación
    private int currentScore = 0; // Puntos de esta sesión
    private int pointsPerLevel = 30; // Puntos al ganar nivel

    // ID ÚNICO PARA ESTE JUEGO (Importante para la Base de Datos)
    private string gameID = "JuegoSumas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        // --- 1. CARGAR NIVEL DESDE LA BASE DE DATOS ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            // Pedimos el nivel guardado para "JuegoSumas"
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);

            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            Debug.Log("Modo Prueba: Jugando sin usuario logueado.");
            currentLevel = 1;
        }

        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // Validación de seguridad
        if (row1Slot == null || row2Slot == null || row3Slot == null)
        {
            Debug.LogError("¡ALERTA! Faltan asignar los Slots_SUMA en el Inspector.");
            return;
        }

        // Reiniciar slots (Visual y lógicamente)
        ResetSlot(row1Slot);
        ResetSlot(row2Slot);
        ResetSlot(row3Slot);

        List<int> correctAnswers = new List<int>();

        // Crear Ecuaciones
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // Crear Respuestas (3 correctas + falsas)
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            int fakeMax = 20 + (currentLevel * 10);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

        // Asignar valores a las fichas
        for (int i = 0; i < answerOptions.Length; i++)
        {
            if (answerOptions[i] != null)
            {
                answerOptions[i].numberValue = finalOptions[i];
                if (answerTexts[i] != null) answerTexts[i].text = finalOptions[i].ToString();

                answerOptions[i].gameObject.SetActive(true);

                if (answerOptions[i].GetComponent<CanvasGroup>())
                    answerOptions[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

                if (answersParent) answerOptions[i].transform.SetParent(answersParent);
                answerOptions[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Helper para limpiar slots
    void ResetSlot(DropSlot_SUMA slot)
    {
        slot.isFilled = false;
        if (slot.GetComponentInChildren<Text>()) slot.GetComponentInChildren<Text>().text = "";
    }

    void SetupEquation(Text[] texts, DropSlot_SUMA slot, List<int> answers)
    {
        // LOGICA DE SUMA (Dificultad progresiva)
        int rangeMax = 5 + (currentLevel * 3);
        int numA = Random.Range(1, rangeMax);
        int numB = Random.Range(1, rangeMax);

        int result = numA + numB;

        if (texts.Length > 1)
        {
            texts[0].text = numA.ToString();
            texts[1].text = numB.ToString();
        }

        slot.expectedResult = result;
        answers.Add(result);
    }

    // Esta función la llaman tus slots _SUMA cuando sueltan la ficha
    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;

            // Ganas si aciertas las 3 ecuaciones
            if (correctCount >= 3)
            {
                Debug.Log("Nivel completado");

                // 1. Puntos Visuales
                currentScore += pointsPerLevel;
                UpdateScoreUI();

                // 2. Aumentar Nivel
                currentLevel++;

                // 3. --- GUARDAR PROGRESO EN BD ---
                // Pasamos los puntos ganados (para sumar al acumulado) y el nuevo nivel
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

    // --- NUEVO: Guardar unificado ---
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;

            // Usamos la función del DatabaseManager que maneja Nivel + Puntaje Acumulado
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Sumas guardado. Nivel: {currentLevel}");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + currentScore.ToString();
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText)
        {
            gameOverText.SetActive(true);
            if (gameOverText.GetComponent<Text>()) gameOverText.GetComponent<Text>().text = "¡INTENTA DE NUEVO!";
        }

        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar puntos de sesión actual
        currentScore = 0;
        UpdateScoreUI();

        // Opcional: ¿Quieres bajar al nivel 1 al perder o mantenerte?
        // currentLevel = 1; 

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