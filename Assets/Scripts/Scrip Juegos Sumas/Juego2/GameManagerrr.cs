using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManagerrr : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    public Text[] row1Texts;
    public DropSlot row1Slot;

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
    private int pointsPerLevel = 30;

    // ID ÚNICO
    private string gameID = "JuegoRestas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        // --- Cargar Nivel ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);
            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            currentLevel = 1;
        }

        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // 1. Limpiar slots para que el imán detecte que están vacíos
        ResetSlot(row1Slot);
        ResetSlot(row2Slot);
        ResetSlot(row3Slot);

        List<int> correctAnswers = new List<int>();

        // 2. Generar ecuaciones
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // 3. Rellenar respuestas
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            int fakeMax = 15 + (currentLevel * 5);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

        // 4. REINICIAR FICHAS (Para que vuelvan abajo y se puedan volver a usar)
        for (int i = 0; i < answerOptions.Length; i++)
        {
            if (answerOptions[i] != null)
            {
                answerOptions[i].numberValue = finalOptions[i];
                if (answerTexts[i] != null) answerTexts[i].text = finalOptions[i].ToString();

                answerOptions[i].gameObject.SetActive(true);

                // Desbloquear raycast
                CanvasGroup cg = answerOptions[i].GetComponent<CanvasGroup>();
                if (cg) cg.blocksRaycasts = true;

                // Devolver al contenedor padre (romper el vínculo con la caja anterior)
                if (answersParent != null) answerOptions[i].transform.SetParent(answersParent);
                answerOptions[i].transform.localScale = Vector3.one;
            }
        }
    }

    void ResetSlot(DropSlot slot)
    {
        if (slot == null) return;
        slot.isFilled = false; // Habilita la caja para recibir nuevas fichas

        Text textComp = slot.GetComponentInChildren<Text>();
        if (textComp) textComp.text = "";
    }

    void SetupEquation(Text[] texts, DropSlot slot, List<int> answers)
    {
        int minNum = 5 + (currentLevel * 2);
        int maxNum = 10 + (currentLevel * 5);

        int numA = Random.Range(minNum, maxNum);
        int numB = Random.Range(1, numA); // B menor que A para evitar negativos
        int result = numA - numB;

        if (texts.Length > 1)
        {
            texts[0].text = numA.ToString();
            texts[1].text = numB.ToString();
        }

        slot.expectedResult = result;
        answers.Add(result);
    }

    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;

            if (correctCount >= 3)
            {
                Debug.Log("¡Nivel Completado!");
                currentScore += pointsPerLevel;
                UpdateScoreUI();
                currentLevel++;

                SaveProgress(pointsPerLevel);
                Invoke("GenerateLevel", 1f);
            }
        }
        else
        {
            StartCoroutine(GameOverSequence());
        }
    }

    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);
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

        currentScore = 0;
        UpdateScoreUI();
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