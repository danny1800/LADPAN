using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public Text targetText;
    public Transform bubbleArea; // El panel donde aparecen las burbujas
    public GameObject bubblePrefab; // El prefab de la burbuja

    [Header("UI Puntuación")]
    public Text scoreText;    // Arrastra aquí el texto de "Puntos: 0"

    [Header("UI del Juego")]
    public GameObject gameOverText; // Texto de perder/ganar

    [Header("Base de Datos")]
    public LevelSaver databaseScript; // Para guardar nivel

    [Header("Sonidos")]
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip popSound;

    private AudioSource audioSource;

    // Variables de juego
    private int currentTarget;
    private int currentLevel = 1;

    // Variables de Puntuación
    private int currentScore = 0;
    // Como ahora solo damos puntos al final, aumentamos el premio por nivel
    private int pointsPerLevel = 20;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (gameOverText) gameOverText.SetActive(false);

        // Iniciar UI de puntos
        UpdateScoreUI();

        StartLevel();
    }

    void StartLevel()
    {
        // Limpiar burbujas viejas si quedaron
        foreach (Transform child in bubbleArea)
        {
            Destroy(child.gameObject);
        }

        // Dificultad: El número objetivo crece con el nivel
        int minTarget = 10 + (currentLevel * 5);
        int maxTarget = 20 + (currentLevel * 10);
        currentTarget = Random.Range(minTarget, maxTarget);

        UpdateUI();
        SpawnBubbles();
    }

    void SpawnBubbles()
    {
        // Generamos 3 burbujas iniciales
        for (int i = 0; i < 3; i++)
        {
            CreateOneBubble();
        }
    }

    public void CreateOneBubble()
    {
        // Crear la burbuja visualmente dentro del BubbleArea
        GameObject newBubble = Instantiate(bubblePrefab, bubbleArea);

        // Posición Aleatoria dentro del área
        RectTransform rect = bubbleArea.GetComponent<RectTransform>();
        float x = Random.Range(-rect.rect.width / 2 + 50, rect.rect.width / 2 - 50);
        float y = Random.Range(-rect.rect.height / 2 + 50, rect.rect.height / 2 - 50);
        newBubble.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

        // Calcular valor: 
        int maxVal = currentTarget > 1 ? currentTarget : 1;
        int val = Random.Range(1, Mathf.Min(10, maxVal + 1));

        // Configurar el script de la burbuja
        newBubble.GetComponent<BubbleController>().Setup(val);
    }

    // Esta función la llama la burbuja al explotar
    public void ProcessBubble(int valueSubtracted)
    {
        currentTarget -= valueSubtracted; // RESTAR
        UpdateUI();

        if (currentTarget == 0)
        {
            if (winSound != null) audioSource.PlayOneShot(winSound);
            // --- GANASTE EL NIVEL ---
            // AQUÍ es el único momento donde sumamos puntos
            if (gameOverText)
            {
                Text txt = gameOverText.GetComponent<Text>();
                txt.text = "¡MUY BIEN!";
                txt.color = Color.green; // Aquí cambias el color
                gameOverText.SetActive(true);
            }

            // Sumar puntos
            currentScore += pointsPerLevel;
            UpdateScoreUI();

            Debug.Log("¡Nivel Completado!");
            if (databaseScript != null) databaseScript.SaveProgress(currentLevel);

            currentLevel++;

            // Ocultar mensaje y avanzar al siguiente nivel
            StartCoroutine(ShowSuccessThenNextLevel()); 
        }
        else if (currentTarget < 0)
        {
            if (loseSound != null) audioSource.PlayOneShot(loseSound);
            // --- PERDISTE (Te pasaste de la resta) ---
            Debug.Log("Juego Terminado - Te pasaste");
            if (databaseScript != null) databaseScript.SaveProgress(currentLevel);
            StartCoroutine(GameOverSequence());
        }
        else
        {
            // --- EL JUEGO SIGUE ---
            // NO sumamos puntos aquí (según tu petición)
            // Solo creamos otra burbuja para seguir jugando
            CreateOneBubble();
        }
    }

    void UpdateUI()
    {
        if (targetText) targetText.text = currentTarget.ToString();
    }

    // Función para actualizar el texto de puntos
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + currentScore.ToString();
        }
    }

    // Para obtener el puntaje desde la base de datos externamente
    public int GetCurrentScore()
    {
        return currentScore;
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverText)
        {
            Text txt = gameOverText.GetComponent<Text>();
            txt.text = "¡TE PASASTE!";
            txt.color = Color.red; // Aquí cambias el color
            gameOverText.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        if (gameOverText) gameOverText.SetActive(false);

        // Reiniciar puntos al perder
        currentScore = 0;
        UpdateScoreUI();

        currentLevel = 1; // Reiniciar nivel
        StartLevel();
    }

    IEnumerator ShowSuccessThenNextLevel()
    {
        yield return new WaitForSeconds(1.5f);

        if (gameOverText)
            gameOverText.SetActive(false);

        StartLevel();
    }

}