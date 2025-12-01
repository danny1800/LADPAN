using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FractionDrawManager : MonoBehaviour
{
    [Header("UI Textos")]
    public Text instructionText;
    public Text hintText;
    public Text levelText;
    public Text winLoseText;
    public GameObject gameOverText;

    [Header("UI Puntuación")]
    public Text scoreText;    // Arrastra aquí tu texto de "Puntos: 0"

    [Header("Zona de Dibujo")]
    public RectTransform drawingArea;
    public GameObject linePrefab;
    public Transform linesContainer;

    [Header("Botones")]
    public Button checkButton;
    public Button cleanButton;

    // --- CAMBIO: Referencia vieja borrada ---
    // public LevelSaver databaseScript; // BORRADO

    // Variables de Juego
    private int currentLevel = 1;
    private int targetParts;
    private int linesNeeded;
    private List<GameObject> drawnLines = new List<GameObject>();

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 100;

    // ID ÚNICO PARA ESTE JUEGO
    private string gameID = "JuegoFracciones";

    // Variables para el Dibujo
    private GameObject currentLine;
    private bool isDrawing = false;
    private Vector2 startPoint;

    private WinLose winLoseSound;

    void Start()
    {
        winLoseSound = FindObjectOfType<WinLose>();

        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(false);

        if (checkButton) checkButton.onClick.AddListener(CheckAnswer);
        if (cleanButton) cleanButton.onClick.AddListener(ClearLines);

        // Iniciar Score en 0
        UpdateScoreUI();

        StartLevel();
    }

    void Update()
    {
        if (checkButton != null && checkButton.interactable)
        {
            HandleDrawingInput();
        }
    }

    void HandleDrawingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition))
            {
                StartDrawing();
            }
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            UpdateCurrentLine();
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishDrawing();
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        startPoint = Input.mousePosition;
        currentLine = Instantiate(linePrefab, linesContainer);
        currentLine.transform.position = startPoint;

        RectTransform rt = currentLine.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);

        if (currentLine.GetComponent<Image>()) currentLine.GetComponent<Image>().raycastTarget = false;
    }

    void UpdateCurrentLine()
    {
        if (currentLine == null) return;
        Vector2 currentPos = Input.mousePosition;
        Vector2 direction = currentPos - startPoint;
        float distance = direction.magnitude;

        RectTransform rt = currentLine.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FinishDrawing()
    {
        isDrawing = false;
        RectTransform rt = currentLine.GetComponent<RectTransform>();

        if (rt.sizeDelta.x < 10) Destroy(currentLine);
        else
        {
            if (currentLine.GetComponent<Image>()) currentLine.GetComponent<Image>().raycastTarget = true;
            drawnLines.Add(currentLine);
        }
        currentLine = null;
    }

    void StartLevel()
    {
        ClearLines();
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (levelText) levelText.text = "Level: " + currentLevel;

        int[] validFractions = { 2, 4, 6, 8 };
        targetParts = validFractions[Random.Range(0, validFractions.Length)];

        if (targetParts == 2) linesNeeded = 1;
        else if (targetParts == 4) linesNeeded = 2;
        else if (targetParts == 6) linesNeeded = 3;
        else if (targetParts == 8) linesNeeded = 4;

        if (instructionText) instructionText.text = "Corta en: 1/" + targetParts;
        if (hintText) hintText.text = "(Usa " + linesNeeded + " líneas)";
    }

    void ClearLines()
    {
        foreach (GameObject line in drawnLines) Destroy(line);
        if (currentLine != null) Destroy(currentLine);
        drawnLines.Clear();
    }

    void CheckAnswer()
    {
        int linesDrawn = drawnLines.Count;
        bool isCorrect = false;

        if (linesDrawn == linesNeeded) isCorrect = true;
        if (targetParts == 4 && linesDrawn == 3) isCorrect = true;

        if (isCorrect)
        {
            Debug.Log("¡Correcto!");
            if (winLoseSound != null)
                winLoseSound.PlayCorrect();
            StartCoroutine(NextLevelSequence(true));
        }
        else
        {
            Debug.Log("Incorrecto.");
            if (winLoseSound != null)
                winLoseSound.PlayIncorrect();
            StartCoroutine(NextLevelSequence(false));
        }
    }

    IEnumerator NextLevelSequence(bool success)
    {
        if (winLoseText)
        {
            winLoseText.gameObject.SetActive(true);
            winLoseText.text = success ? "¡CORRECTO!" : "¡INCORRECTO!";
            winLoseText.color = success ? Color.green : Color.red;
        }

        if (checkButton) checkButton.interactable = false;
        if (cleanButton) cleanButton.interactable = false;

        yield return new WaitForSeconds(1.5f);

        if (checkButton) checkButton.interactable = true;
        if (cleanButton) cleanButton.interactable = true;

        if (success)
        {
            // Sumar puntos
            currentScore += pointsPerLevel;
            UpdateScoreUI();

            // Guardar progreso de NIVEL
            if (DatabaseManager.Instance != null && GameSession.Current != null && GameSession.Current.CurrentUser != null)
            {
                DatabaseManager.Instance.SaveProgress(GameSession.Current.CurrentUser.Id, currentLevel);
            }

            currentLevel++;
            StartLevel();
        }
        else
        {
            // AL PERDER: Guardamos el PUNTAJE final
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
            // Guardamos con el ID "JuegoFracciones"
            DatabaseManager.Instance.SaveScore(myUserId, gameID, currentScore);
            Debug.Log($"Puntaje de Fracciones guardado: {currentScore}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText)
        {
            if (gameOverText.GetComponent<Text>()) gameOverText.GetComponent<Text>().text = "GAME OVER";
            gameOverText.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar Puntuación al perder
        currentScore = 0;
        UpdateScoreUI();

        currentLevel = 1;
        StartLevel();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + currentScore.ToString();
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}