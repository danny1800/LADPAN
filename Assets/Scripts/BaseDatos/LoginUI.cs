using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [Header("UI Principal")]
    public InputField nameInput;
    public Button actionButton; // El botón "Jugar" o "Entrar"
    public Text errorText;

    [Header("UI Contraseña (Oculto al inicio)")]
    public GameObject passwordPanel; // Panel que contiene el campo de contraseña
    public InputField passwordInput;

    [Header("Configuración")]
    public string sceneMenuAlumnos = "MenuMinijuegos";
    public string scenePanelProfesor = "EscenaProfesor";

    // Estado interno
    private bool esperandoPassword = false;
    private string nombreCached = "";

    void Start()
    {
        if (passwordPanel) passwordPanel.SetActive(false);
        if (errorText) errorText.text = "";

        GameSession.CurrentUser = null;

        actionButton.onClick.AddListener(ProcesarEntrada);
    }

    void ProcesarEntrada()
    {
        if (esperandoPassword)
        {
            // FASE 2: Ya sabemos que es profe, verificamos la contraseña
            VerificarPasswordProfesor();
        }
        else
        {
            // FASE 1: Revisamos el nombre
            VerificarNombre();
        }
    }

    void VerificarNombre()
    {
        string nombre = nameInput.text.Trim();

        if (string.IsNullOrEmpty(nombre) || nombre.Length < 3)
        {
            MostrarError("Escribe un nombre válido (mínimo 3 letras).");
            return;
        }

        if (DatabaseManager.Instance == null)
        {
            MostrarError("Error: Base de Datos no conectada.");
            return;
        }

        // 1. ¿ES PROFESOR?
        if (DatabaseManager.Instance.EsProfesor(nombre))
        {
            // ¡Es profe! Pedir contraseña
            nombreCached = nombre;
            ActivarModoPassword();
        }
        else
        {
            // 2. NO ES PROFESOR -> Es alumno (Login o Registro directo)
            EntrarComoAlumno(nombre);
        }
    }

    void ActivarModoPassword()
    {
        esperandoPassword = true;
        passwordPanel.SetActive(true); // Aparece el campo de pass
        nameInput.interactable = false; // Bloqueamos el nombre para que no lo cambie
        errorText.text = "Hola Profe. Ingresa tu clave:";
        errorText.color = Color.blue;

        // Cambiamos el texto del botón si tiene componente de texto
        Text btnText = actionButton.GetComponentInChildren<Text>();
        if (btnText) btnText.text = "VERIFICAR";
    }

    void VerificarPasswordProfesor()
    {
        string pass = passwordInput.text;

        // Intentar Login
        Usuario profe = DatabaseManager.Instance.Login(nombreCached, pass);

        if (profe != null)
        {
            // Login Exitoso
            GameSession.CurrentUser = profe;
            SceneManager.LoadScene(scenePanelProfesor);
        }
        else
        {
            MostrarError("Contraseña incorrecta.");
            passwordInput.text = ""; // Limpiar campo para reintentar
        }
    }

    void EntrarComoAlumno(string nombre)
    {
        Usuario alumno;

        // Intentar registrar
        bool registro = DatabaseManager.Instance.RegistrarAlumno(nombre, out alumno);

        // Si ya existe, loguear
        if (!registro)
        {
            alumno = DatabaseManager.Instance.Login(nombre);
        }

        if (alumno != null)
        {
            GameSession.CurrentUser = alumno;
            errorText.text = "¡Bienvenido " + nombre + "!";
            errorText.color = Color.green;
            actionButton.interactable = false;
            Invoke("IrMenuAlumnos", 1.0f);
        }
    }

    void IrMenuAlumnos()
    {
        SceneManager.LoadScene(sceneMenuAlumnos);
    }

    void MostrarError(string msg)
    {
        if (errorText)
        {
            errorText.text = msg;
            errorText.color = Color.red;
        }
    }

    // Función extra para el botón "Cancelar" si el profe se equivocó de nombre
    public void CancelarLoginProfe()
    {
        esperandoPassword = false;
        passwordPanel.SetActive(false);
        nameInput.interactable = true;
        passwordInput.text = "";
        errorText.text = "";

        Text btnText = actionButton.GetComponentInChildren<Text>();
        if (btnText) btnText.text = "JUGAR";
    }
}