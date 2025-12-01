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

    // --- NUEVO: Texto para ver los puntos ---
    public Text scoreText;

    // --- CAMBIO: Eliminamos la referencia vieja ---
    // public LevelSaver databaseScript; // BORRADO

    // Variables de estado
    private int currentLevel = 1;
    private int correctCount = 0;

    // --- NUEVO: Variables para el puntaje ---
    private int currentScore = 0;
    private int pointsPerLevel = 50; // Puntos por completar las 3 sumas
    private string gameID = "JuegoSumas"; // ID ÚNICO PARA LA TABLA

    void Start()
    {
        if (gameOverText) gameOverText.SetActive(false);
        UpdateScoreUI();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        correctCount = 0;

        // Limpiar slots
        row1Slot.isFilled = false; row2Slot.isFilled = false; row3Slot.isFilled = false;

        // Limpiar textos viejos
        if (row1Slot.GetComponentInChildren<Text>()) row1Slot.GetComponentInChildren<Text>().text = "";
        if (row2Slot.GetComponentInChildren<Text>()) row2Slot.GetComponentInChildren<Text>().text = "";
        if (row3Slot.GetComponentInChildren<Text>()) row3Slot.GetComponentInChildren<Text>().text = "";

        List<int> correctAnswers = new List<int>();

        // Crear Ecuaciones de SUMA
        SetupEquation(row1Texts, row1Slot, correctAnswers);
        SetupEquation(row2Texts, row2Slot, correctAnswers);
        SetupEquation(row3Texts, row3Slot, correctAnswers);

        // Rellenar respuestas
        List<int> finalOptions = new List<int>(correctAnswers);

        while (finalOptions.Count < answerOptions.Length)
        {
            // Sumas pueden dar números más altos, ajustamos la trampa
            int fakeMax = 20 + (currentLevel * 5);
            int fakeNumber = Random.Range(1, fakeMax);
            if (!finalOptions.Contains(fakeNumber)) finalOptions.Add(fakeNumber);
        }

        Shuffle(finalOptions);

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

    void SetupEquation(Text[] texts, DropSlot slot, List<int> answers)
    {
        // DIFICULTAD SUMAS:
        int maxNum = 9 + currentLevel;

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

    public void CheckAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            correctCount++;

            // Si completa las 3 sumas
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
            // AL PERDER: Guardamos el PUNTAJE final
            SaveMyScore();
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Función para guardar en la base de datos ---
    void SaveMyScore()
    {
        if (DatabaseManager.Instance != null && GameSession.Current != null && GameSession.Current.CurrentUser != null)
        {
            int myUserId = GameSession.Current.CurrentUser.Id;
            // Guardamos bajo el ID "JuegoSumas"
            DatabaseManager.Instance.SaveScore(myUserId, gameID, currentScore);
            Debug.Log($"Puntaje de Sumas guardado: {currentScore}");
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