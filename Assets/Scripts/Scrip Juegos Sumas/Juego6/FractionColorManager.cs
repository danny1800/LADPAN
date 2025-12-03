using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FractionColorManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Text fractionText; // Muestra la fracción (ej: 2/4)
    public Text levelText;
    public Text winLoseText;
    public GameObject gameOverText;

    [Header("UI Puntuación")]
    public Text scoreText;

    [Header("Juego")]
    public Transform circleContainer; // Objeto vacío donde nacen las rebanadas
    public GameObject slicePrefab;    // Tu prefab de la rebanada
    public Button checkButton;        // Botón comprobar

    // Variables internas
    private int currentLevel = 1;
    private int numerator;   // Cuántos hay que pintar
    private int denominator; // Total de rebanadas
    private List<SliceController> currentSlices = new List<SliceController>();

    // --- SISTEMA DE PUNTUACIÓN (Igualado al primer código) ---
    private int currentScore = 0;
    private int pointsPerLevel = 100; // CAMBIO: Ahora da 100 puntos por nivel

    // ID ÚNICO PARA ESTE JUEGO
    private string gameID = "JuegoFraccionesColor";

    void Start()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(false);

        if (checkButton) checkButton.onClick.AddListener(CheckAnswer);

        // --- 1. CARGAR NIVEL DESDE LA BASE DE DATOS ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            // Pedimos el nivel guardado para este juego específico
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);

            // Si es la primera vez, empezamos en nivel 1
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

    void StartLevel()
    {
        // 1. Limpiar el círculo anterior
        foreach (Transform child in circleContainer) Destroy(child.gameObject);
        currentSlices.Clear();

        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (levelText) levelText.text = "Nivel: " + currentLevel;

        // 2. DIFICULTAD (Aumenta partes según el nivel)
        int minParts = 2;
        int maxParts = 3 + (currentLevel / 2); // Sube dificultad más lento
        if (maxParts > 8) maxParts = 8; // Máximo visual recomendado para que no se vea feo

        denominator = Random.Range(minParts, maxParts + 1);
        numerator = Random.Range(1, denominator); // Siempre menor al denominador

        // 3. Mostrar Texto
        fractionText.text = numerator + "\n—\n" + denominator;

        // 4. Crear el círculo
        GenerateCircle(denominator);
    }

    void GenerateCircle(int parts)
    {
        float fillAmount = 1.0f / parts;
        float degreesPerSlice = 360f / parts;

        for (int i = 0; i < parts; i++)
        {
            GameObject newSlice = Instantiate(slicePrefab, circleContainer);

            // Asumimos que tu prefab tiene el script SliceController
            SliceController controller = newSlice.GetComponent<SliceController>();

            if (controller != null)
            {
                float rotationZ = -(degreesPerSlice * i);
                controller.Setup(fillAmount, rotationZ);

                // Truco del botón: Agregar funcionalidad de click dinámicamente
                Button btn = newSlice.GetComponent<Button>();
                if (btn == null) btn = newSlice.AddComponent<Button>(); // Si no tiene botón, se lo ponemos

                btn.transition = Selectable.Transition.None;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(controller.OnClick);

                currentSlices.Add(controller);
            }
        }
    }

    void CheckAnswer()
    {
        int paintedCount = 0;

        foreach (SliceController slice in currentSlices)
        {
            if (slice.isSelected) paintedCount++;
        }

        Debug.Log("Pintaste: " + paintedCount + ". Necesarias: " + numerator);

        if (paintedCount == numerator)
        {
            StartCoroutine(ResultSequence(true));
        }
        else
        {
            StartCoroutine(ResultSequence(false));
        }
    }

    IEnumerator ResultSequence(bool success)
    {
        if (winLoseText)
        {
            winLoseText.gameObject.SetActive(true);
            winLoseText.text = success ? "¡CORRECTO!" : "¡INCORRECTO!";
            winLoseText.color = success ? Color.green : Color.red;
        }

        if (checkButton) checkButton.interactable = false;
        yield return new WaitForSeconds(1.5f);
        if (checkButton) checkButton.interactable = true;

        if (success)
        {
            // 1. Sumar puntos (Ahora suma 100)
            currentScore += pointsPerLevel;
            UpdateScoreUI();

            // 2. Subir nivel
            currentLevel++;

            // 3. --- GUARDAR PROGRESO EN BD ---
            SaveProgress(pointsPerLevel);

            StartLevel();
        }
        else
        {
            // AL PERDER: Game Over
            StartCoroutine(GameOverSequence());
        }
    }

    // --- NUEVO: Guardar Unificado ---
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;
            // Guardamos con el ID "JuegoFraccionesColor"
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Color guardado: Nivel {currentLevel}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (gameOverText) gameOverText.SetActive(false);

        // --- LÓGICA DE DERROTA IDENTICA AL PRIMER JUEGO ---

        // 1. Reiniciar Puntos de la sesión
        currentScore = 0;
        UpdateScoreUI();

        // 2. Reiniciar nivel (Descomentado para igualar dificultad al primer juego)
        currentLevel = 1;

        StartLevel();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + currentScore.ToString();
    }
}