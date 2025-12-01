using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManagerrr : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    // En tu inspector veo que Row 1 Texts tiene Size 2.
    // texts[0] será el primer número, texts[1] será el segundo.
    public Text[] row1Texts;
    public DropSlot row1Slot;

    [Header("UI Puntuación")]
    public Text scoreText;    // Veo en tu foto que ya asignaste "puntos (Text)". ¡Bien!

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

    // --- CAMBIO IMPORTANTE ---
    // He borrado la variable "public LevelSaver databaseScript"
    // Al guardar este script, esa casilla vacía en tu inspector DESAPARECERÁ.

    // Variables de estado
    private int currentLevel = 1;
    private int correctCount = 0;

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 30;

    // ID ÚNICO: Como es la pizarra verde, le ponemos este ID para el Ranking
    private string gameID = "JuegoRestas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // Limpiar los slots
        row1Slot.isFilled = false;
        row2Slot.isFilled = false;
        row3Slot.isFilled = false;

        row1Slot.GetComponentInChildren<Text>().text = "";
        row2Slot.GetComponentInChildren<Text>().text = "";
        row3Slot.GetComponentInChildren<Text>().text = "";

        List<int> correctAnswers = new List<int>();

        // Generar ecuaciones
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // Rellenar respuestas
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            int fakeMax = 15 + (currentLevel * 5);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

        for (int i = 0; i < answerOptions.Length; i++)
        {
            answerOptions[i].numberValue = finalOptions[i];
            answerTexts[i].text = finalOptions[i].ToString();
            answerOptions[i].gameObject.SetActive(true);
            answerOptions[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (answersParent != null) answerOptions[i].transform.SetParent(answersParent);
            answerOptions[i].transform.localScale = Vector3.one;
        }
    }

    void SetupEquation(Text[] texts, DropSlot slot, List<int> answers)
    {
        // LOGICA DE RESTAS (Coincide con tu pizarra verde)
        int minNum = 5 + (currentLevel * 2);
        int maxNum = 10 + (currentLevel * 5);

        int numA = Random.Range(minNum, maxNum);
        int numB = Random.Range(1, numA); // B menor que A para que no de negativo
        int result = numA - numB;

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

            if (correctCount >= 3)
            {
                Debug.Log("¡Nivel Completado!");

                // Sumar puntos
                currentScore += pointsPerLevel;
                UpdateScoreUI();

                // Guardar progreso de NIVEL (Opcional, si quieres guardar en qué nivel va)
                if (DatabaseManager.Instance != null && GameSession.Current != null && GameSession.Current.CurrentUser != null)
                {
                    DatabaseManager.Instance.SaveProgress(GameSession.Current.CurrentUser.Id, currentLevel);
                }

                currentLevel++;
                Invoke("GenerateLevel", 1f);
            }
        }
        else
        {
            // AL PERDER: Guardamos el PUNTAJE final
            SaveMyScore();
            StartCoroutine(GameOverSequence());
        }
    }

    // Guardar en la DB usando el ID "JuegoRestas"
    void SaveMyScore()
    {
        if (DatabaseManager.Instance != null && GameSession.Current != null && GameSession.Current.CurrentUser != null)
        {
            int myUserId = GameSession.Current.CurrentUser.Id;
            DatabaseManager.Instance.SaveScore(myUserId, gameID, currentScore);
            Debug.Log($"Puntaje de Restas guardado: {currentScore}");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + currentScore.ToString();
        }
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText) gameOverText.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar
        currentScore = 0;
        UpdateScoreUI();

        currentLevel = 1;
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