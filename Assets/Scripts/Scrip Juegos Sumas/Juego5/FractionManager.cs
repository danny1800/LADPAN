using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FractionManager : MonoBehaviour
{
    [Header("UI Textos")]
    public Text instructionText; // "Corta en: 1/2"
    public Text levelText;
    public Text winLoseText;
    public GameObject gameOverText;

    [Header("Zona de Dibujo")]
    public RectTransform drawingArea; // Arrastra la IMAGEN del plato aquí
    public GameObject linePrefab;     // Tu prefab de línea (¡Pivot X a 0!)
    public Transform linesContainer;  // Arrastra la IMAGEN del plato aquí también

    [Header("Botones")]
    public Button checkButton; // Botón "TRAZAR" / "COMPROBAR"
    public Button cleanButton; // Botón "BORRAR"

    [Header("Base de Datos")]
    public LevelSaver databaseScript;

    // Variables de Juego
    private int currentLevel = 1;
    private int targetParts;
    private List<GameObject> drawnLines = new List<GameObject>();

    // Variables para el Dibujo
    private GameObject currentLine;
    private bool isDrawing = false;
    private Vector2 startPoint;

    void Start()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(false);

        // Conectar botones
        if (checkButton) checkButton.onClick.AddListener(CheckAnswer);
        if (cleanButton) cleanButton.onClick.AddListener(ClearLines);

        StartLevel();
    }

    void Update()
    {
        // Solo permitir dibujar si el botón de comprobar está activo
        if (checkButton != null && checkButton.interactable)
        {
            HandleDrawingInput();
        }
    }

    // --- LÓGICA DE DIBUJO (Touch / Mouse) ---
    void HandleDrawingInput()
    {
        // 1. Clic inicial (Empezar corte)
        if (Input.GetMouseButtonDown(0))
        {
            // Solo dibujar si tocamos DENTRO del plato
            if (RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition))
            {
                StartDrawing();
            }
        }

        // 2. Arrastrar (Estirar corte)
        if (Input.GetMouseButton(0) && isDrawing)
        {
            UpdateCurrentLine();
        }

        // 3. Soltar (Terminar corte)
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishDrawing();
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        startPoint = Input.mousePosition;

        // Crear línea hija del plato
        currentLine = Instantiate(linePrefab, linesContainer);
        currentLine.transform.position = startPoint;

        // Poner tamaño inicial en 0
        RectTransform rt = currentLine.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);

        // Desactivar Raycast de la línea para que no estorbe al mouse
        if (currentLine.GetComponent<Image>()) currentLine.GetComponent<Image>().raycastTarget = false;
    }

    void UpdateCurrentLine()
    {
        if (currentLine == null) return;

        Vector2 currentPos = Input.mousePosition;
        Vector2 direction = currentPos - startPoint;
        float distance = direction.magnitude;

        RectTransform rt = currentLine.GetComponent<RectTransform>();

        // Estirar largo
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);

        // Rotar hacia el mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FinishDrawing()
    {
        isDrawing = false;

        RectTransform rt = currentLine.GetComponent<RectTransform>();
        // Si la línea es muy corta (un error), la borramos
        if (rt.sizeDelta.x < 20)
        {
            Destroy(currentLine);
        }
        else
        {
            drawnLines.Add(currentLine);
        }
        currentLine = null;
    }

    // --- LÓGICA DEL JUEGO ---

    void StartLevel()
    {
        ClearLines();
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (levelText) levelText.text = "Level: " + currentLevel;

        // SOLO pedimos fracciones que se pueden cortar tipo Pizza (cruces)
        // 1/2, 1/4, 1/6, 1/8
        int[] validFractions = { 2, 4, 6, 8 };

        targetParts = validFractions[Random.Range(0, validFractions.Length)];

        if (instructionText) instructionText.text = "Corta en: 1/" + targetParts;
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

        // VALIDACIÓN (Regla de Pizza)
        // 1/2 necesita 1 línea
        // 1/4 necesita 2 líneas (cruz)
        // 1/6 necesita 3 líneas (asterisco)
        // 1/8 necesita 4 líneas (doble cruz)

        if (targetParts == 2 && linesDrawn == 1) isCorrect = true;
        else if (targetParts == 4 && linesDrawn == 2) isCorrect = true;
        else if (targetParts == 6 && linesDrawn == 3) isCorrect = true;
        else if (targetParts == 8 && linesDrawn == 4) isCorrect = true;

        // Validación extra por si alguien hace cortes paralelos para 1/4 (3 líneas)
        else if (targetParts == 4 && linesDrawn == 3) isCorrect = true;

        if (isCorrect)
        {
            Debug.Log("¡Correcto!");
            StartCoroutine(NextLevelSequence(true));
        }
        else
        {
            Debug.Log("Incorrecto. Se pidieron 1/" + targetParts + " y dibujaste " + linesDrawn + " líneas.");
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
            if (databaseScript != null) databaseScript.SaveProgress(currentLevel);
            currentLevel++;
            StartLevel();
        }
        else
        {
            if (databaseScript != null) databaseScript.SaveProgress(currentLevel);
            StartCoroutine(GameOverSequence());
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
        currentLevel = 1;
        StartLevel();
    }
}