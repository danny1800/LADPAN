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
    public Text scoreText;

    [Header("Zona de Dibujo")]
    public RectTransform drawingArea;
    public GameObject linePrefab;
    public Transform linesContainer;

    [Header("Botones")]
    public Button checkButton;
    public Button cleanButton;

    // Variables de Juego
    private int currentLevel = 1;
    private int targetParts;
    private int linesNeeded;
    private List<GameObject> drawnLines = new List<GameObject>();

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 100;

    // ID ÚNICO PARA LA BASE DE DATOS
    private string gameID = "JuegoFracciones";

    // Variables para el Dibujo
    private GameObject currentLine;
    private bool isDrawing = false;
    private Vector2 startPoint;

    void Start()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(false);

        if (checkButton) checkButton.onClick.AddListener(CheckAnswer);
        if (cleanButton) cleanButton.onClick.AddListener(ClearLines);

        // --- 1. CARGAR NIVEL DESDE LA BD ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);
            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            Debug.Log("Modo Prueba: Sin usuario. Nivel 1.");
            currentLevel = 1;
        }

        UpdateScoreUI();
        StartLevel();
    }

    void Update()
    {
        // Solo permitimos dibujar si el botón de comprobar está activo (significa que estamos jugando)
        if (checkButton != null && checkButton.interactable)
        {
            HandleDrawingInput();
        }
    }

    void HandleDrawingInput()
    {
        // Detectar clic inicial
        if (Input.GetMouseButtonDown(0))
        {
            // Solo dibujar si el mouse está dentro del área blanca
            if (RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition))
            {
                StartDrawing();
            }
        }

        // Detectar arrastre
        if (Input.GetMouseButton(0) && isDrawing)
        {
            UpdateCurrentLine();
        }

        // Detectar soltar clic
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

        // Resetear tamaño inicial
        RectTransform rt = currentLine.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);

        // Desactivar Raycast mientras dibujamos para que no interfiera con el mouse
        if (currentLine.GetComponent<Image>()) currentLine.GetComponent<Image>().raycastTarget = false;
    }

    void UpdateCurrentLine()
    {
        if (currentLine == null) return;

        Vector2 currentPos = Input.mousePosition;
        Vector2 direction = currentPos - startPoint;
        float distance = direction.magnitude;

        RectTransform rt = currentLine.GetComponent<RectTransform>();

        // Ajustar largo
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);

        // Ajustar rotación
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FinishDrawing()
    {
        isDrawing = false;
        if (currentLine != null)
        {
            RectTransform rt = currentLine.GetComponent<RectTransform>();

            // Si la línea es muy cortita (un punto accidental), la borramos
            if (rt.sizeDelta.x < 10)
            {
                Destroy(currentLine);
            }
            else
            {
                // Activamos Raycast de nuevo (opcional, depende de tu lógica de colisión)
                if (currentLine.GetComponent<Image>()) currentLine.GetComponent<Image>().raycastTarget = true;
                drawnLines.Add(currentLine);
            }
        }
        currentLine = null;
    }

    void StartLevel()
    {
        ClearLines();
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (levelText) levelText.text = "Nivel: " + currentLevel;

        // Lógica de dificultad simple: Elegir un denominador al azar
        // Podrías hacerlo más difícil según el currentLevel (ej: nivel 10 usa fracciones impares)
        int[] validFractions = { 2, 4, 6, 8 };
        targetParts = validFractions[Random.Range(0, validFractions.Length)];

        // Calcular líneas necesarias (cortes = partes / 2 para círculos tipo pizza)
        // Nota: Esta lógica es simplificada para tu juego visual actual
        if (targetParts == 2) linesNeeded = 1;
        else if (targetParts == 4) linesNeeded = 2;
        else if (targetParts == 6) linesNeeded = 3;
        else if (targetParts == 8) linesNeeded = 4;

        if (instructionText) instructionText.text = "Corta en: 1/" + targetParts;
        if (hintText) hintText.text = "(Dibuja " + linesNeeded + " líneas)";
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

        // Validación simple por cantidad de líneas
        // (Para un juego real, aquí iría lógica de colisión geométrica)
        if (linesDrawn == linesNeeded) isCorrect = true;

        // Excepción o tolerancia
        if (targetParts == 4 && linesDrawn == 3) isCorrect = false; // Corregido a false para ser estricto, o true si quieres ser amable

        if (isCorrect)
        {
            Debug.Log("¡Correcto!");
            StartCoroutine(NextLevelSequence(true));
        }
        else
        {
            Debug.Log("Incorrecto.");
            StartCoroutine(NextLevelSequence(false));
        }
    }

    IEnumerator NextLevelSequence(bool success)
    {
        if (winLoseText)
        {
            winLoseText.gameObject.SetActive(true);
            winLoseText.text = success ? "¡MUY BIEN!" : "¡SIGUE INTENTANDO!";
            winLoseText.color = success ? Color.green : Color.red;
        }

        // Bloquear botones mientras pasa la animación
        if (checkButton) checkButton.interactable = false;
        if (cleanButton) cleanButton.interactable = false;

        yield return new WaitForSeconds(1.5f);

        if (checkButton) checkButton.interactable = true;
        if (cleanButton) cleanButton.interactable = true;

        if (success)
        {
            // 1. Sumar Puntos
            currentScore += pointsPerLevel;
            UpdateScoreUI();

            // 2. Subir Nivel
            currentLevel++;

            // 3. --- GUARDAR PROGRESO EN BD ---
            SaveProgress(pointsPerLevel);

            StartLevel();
        }
        else
        {
            // Al perder, reiniciamos (Game Over)
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Guardar en DB ---
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;
            // Guardamos con ID "JuegoFracciones"
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Fracciones guardado: Nivel {currentLevel}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText)
        {
            gameOverText.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar Puntuación y Nivel al perder
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
}