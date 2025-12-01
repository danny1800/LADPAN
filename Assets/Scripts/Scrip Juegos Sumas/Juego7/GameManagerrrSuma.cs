using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManagerrrSuma : MonoBehaviour
{
    [Header("Referencias UI Ecuaciones")]
    public Text[] row1Texts;
    public DropSlot row1Slot;

    [Header("UI Puntuación")]
    public Text scoreText;    // Arrastra aquí tu texto de Puntos

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

    // --- CAMBIO: Referencia vieja borrada ---
    // public LevelSaver databaseScript; // BORRADO

    // Variables de estado
    private int currentLevel = 1;
    private int correctCount = 0;

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 30; // Puntos por completar las 3 sumas

    // ID ÚNICO PARA ESTE JUEGO
    private string gameID = "JuegoSumas";

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);

        // Iniciar puntos en 0
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

        // Limpiar textos
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
            // En sumas, los resultados son más grandes, así que la trampa debe ser mayor
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
            answerOptions[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (answersParent) answerOptions[i].transform.SetParent(answersParent);

            answerOptions[i].transform.localScale = Vector3.one;
        }
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
            if (correctCount >= 3)
            {
                Debug.Log("¡Nivel Completado!");

                // Sumar puntos
                currentScore += pointsPerLevel;
                UpdateScoreUI();

                // (Opcional) Guardar progreso de NIVEL
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
            // AL PERDER: Guardar PUNTAJE final
            SaveMyScore();
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Guardar en la DB ---
    void SaveMyScore()
    {
        if (DatabaseManager.Instance != null && GameSession.Current != null && GameSession.Current.CurrentUser != null)
        {
            int myUserId = GameSession.Current.CurrentUser.Id;
            // Guardamos con el ID "JuegoSumas"
            DatabaseManager.Instance.SaveScore(myUserId, gameID, currentScore);
            Debug.Log($"Puntaje de Sumas guardado: {currentScore}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText) gameOverText.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar puntos y nivel
        currentScore = 0;
        UpdateScoreUI();

        currentLevel = 1;
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