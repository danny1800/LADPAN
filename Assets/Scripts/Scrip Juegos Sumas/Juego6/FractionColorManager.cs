using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FractionColorManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Text fractionText; // Muestra la fracción
    public Text levelText;
    public Text winLoseText;
    public GameObject gameOverText;

    [Header("UI Puntuación")]
    public Text scoreText;    // Arrastra aquí el texto de Puntos

    [Header("Juego")]
    public Transform circleContainer; // Objeto vacío donde nacen las rebanadas
    public GameObject slicePrefab;    // Tu prefab de la rebanada
    public Button checkButton;        // Botón comprobar

    // --- CAMBIO: Referencia vieja borrada ---
    // public LevelSaver databaseScript; // BORRADO

    // Variables internas
    private int currentLevel = 1;
    private int numerator;   // Cuántos hay que pintar
    private int denominator; // Total de rebanadas
    private List<SliceController> currentSlices = new List<SliceController>();

    // Variables de Puntuación
    private int currentScore = 0;
    private int pointsPerLevel = 50;

    // ID ÚNICO PARA ESTE JUEGO
    private string gameID = "JuegoFraccionesColor";

    void Start()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(false);

        checkButton.onClick.AddListener(CheckAnswer);

        // Iniciar puntos
        UpdateScoreUI();

        StartLevel();
    }

    void StartLevel()
    {
        // 1. Limpiar el círculo anterior
        foreach (Transform child in circleContainer) Destroy(child.gameObject);
        currentSlices.Clear();

        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (levelText) levelText.text = "Level: " + currentLevel;

        // 2. DIFICULTAD
        int minParts = 2;
        int maxParts = 3 + currentLevel;
        if (maxParts > 8) maxParts = 8; // Máximo visual recomendado

        denominator = Random.Range(minParts, maxParts + 1);
        numerator = Random.Range(1, denominator);

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
            SliceController controller = newSlice.GetComponent<SliceController>();

            float rotationZ = -(degreesPerSlice * i);
            controller.Setup(fillAmount, rotationZ);

            // Truco del botón
            Button btn = newSlice.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(controller.OnClick);

            currentSlices.Add(controller);
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

        checkButton.interactable = false;
        yield return new WaitForSeconds(1.5f);
        checkButton.interactable = true;

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
            // AL PERDER: Guardar PUNTAJE
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
            // Guardamos con el ID "JuegoFraccionesColor"
            DatabaseManager.Instance.SaveScore(myUserId, gameID, currentScore);
            Debug.Log($"Puntaje de Fracciones (Color) guardado: {currentScore}");
        }
    }

    IEnumerator GameOverSequence()
    {
        if (winLoseText) winLoseText.gameObject.SetActive(false);
        if (gameOverText) gameOverText.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar Puntos
        currentScore = 0;
        UpdateScoreUI();

        currentLevel = 1;
        StartLevel();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + currentScore.ToString();
    }
}