using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManagueJ9 : MonoBehaviour
{
    [Header("UI Referencias")]
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI textoRespuestaUsuario;
    public TextMeshProUGUI textoPuntaje;
    public Image barraTiempo;
    public GameObject[] iconosVidas; // Arrastra aquí las 3 imagenes de vacas
    public GameObject panelGameOver;

    [Header("Configuración Juego")]
    public float tiempoMaximo = 10f;

    // Variables internas
    private int puntajeActual = 0; // Puntos ganados en ESTA partida
    private int vidas = 3;
    private float tiempoRestante;
    private int resultadoCorrecto;
    private string respuestaActual = "";
    private bool juegoActivo = true;

    // --- NUEVO: Variables de Nivel y DB ---
    private int currentLevel = 1;
    private string gameID = "JuegoMultiplicacion"; // ID único para la BD

    void Start()
    {
        // --- 1. CARGAR NIVEL DE DIFICULTAD ---
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            currentLevel = DatabaseManager.Instance.LoadLevel(GameSession.CurrentUser.Id, gameID);
            if (currentLevel < 1) currentLevel = 1;
        }
        else
        {
            currentLevel = 1; // Modo prueba
        }

        StartCoroutine(LogicaTiempo());
        GenerarNuevaOperacion();
        ActualizarUI();
    }

    void Update()
    {
        if (juegoActivo)
        {
            barraTiempo.fillAmount = tiempoRestante / tiempoMaximo;
        }
    }

    // --- Lógica del Juego ---

    void GenerarNuevaOperacion()
    {
        // --- 2. DIFICULTAD DINÁMICA ---
        // A medida que sube el nivel, los números son más grandes.
        // Nivel 1: hasta 10. Nivel 5: hasta 12. Nivel 10: hasta 15.
        int maxRango = 10 + (currentLevel / 2);

        int a = Random.Range(1, maxRango + 1);
        int b = Random.Range(1, 11); // Mantenemos uno de los factores controlado al inicio

        resultadoCorrecto = a * b;
        textoPregunta.text = a.ToString() + " X " + b.ToString();

        // Resetear tiempo y respuesta
        tiempoRestante = tiempoMaximo;
        respuestaActual = "";
        textoRespuestaUsuario.text = "";
    }

    IEnumerator LogicaTiempo()
    {
        while (juegoActivo)
        {
            yield return new WaitForSeconds(0.1f);
            tiempoRestante -= 0.1f;

            if (tiempoRestante <= 0)
            {
                PerderVida();
                GenerarNuevaOperacion();
            }
        }
    }

    public void PerderVida()
    {
        vidas--;

        if (vidas >= 0 && vidas < iconosVidas.Length)
        {
            iconosVidas[vidas].SetActive(false);
        }

        if (vidas <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        juegoActivo = false;
        Debug.Log("Juego Terminado");

        // --- 3. GUARDAR PROGRESO AL PERDER ---
        // Guardamos los puntos que hizo en esta sesión y el nivel actual
        SaveProgress(puntajeActual);

        if (panelGameOver != null) panelGameOver.SetActive(true);
    }

    // Función auxiliar para guardar
    void SaveProgress(int puntosGanados)
    {
        if (DatabaseManager.Instance != null && GameSession.CurrentUser != null)
        {
            int myUserId = GameSession.CurrentUser.Id;

            // Enviamos a la BD: ID Usuario, ID Juego, Puntos obtenidos, Nivel Actual
            DatabaseManager.Instance.GuardarProgreso(myUserId, gameID, puntosGanados, currentLevel);

            Debug.Log($"Progreso Multiplicación guardado. Puntos sesión: {puntosGanados}");
        }
    }

    // --- Métodos para los Botones (UI) ---

    public void PresionarNumero(string numero)
    {
        if (!juegoActivo) return;
        if (respuestaActual.Length < 4)
        {
            respuestaActual += numero;
            textoRespuestaUsuario.text = respuestaActual;
        }
    }

    public void Borrar()
    {
        if (!juegoActivo) return;
        respuestaActual = "";
        textoRespuestaUsuario.text = respuestaActual;
    }

    public void Confirmar()
    {
        if (!juegoActivo || string.IsNullOrEmpty(respuestaActual)) return;

        int respuestaInt = int.Parse(respuestaActual);

        if (respuestaInt == resultadoCorrecto)
        {
            // Respuesta Correcta
            puntajeActual += 10; // 10 puntos por acierto

            // Opcional: Si acierta muchas, subimos el nivel en la misma partida
            if (puntajeActual % 100 == 0) // Cada 100 puntos sube dificultad
            {
                currentLevel++;
            }

            GenerarNuevaOperacion();
        }
        else
        {
            // Respuesta Incorrecta
            PerderVida();
            respuestaActual = "";
            textoRespuestaUsuario.text = "";
            GenerarNuevaOperacion();
        }
        ActualizarUI();
    }

    void ActualizarUI()
    {
        textoPuntaje.text = "Puntaje: " + puntajeActual.ToString();
    }
}