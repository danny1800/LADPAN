using UnityEngine;
using UnityEngine.UI; // Necesario para controlar la UI
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class LoginUI : MonoBehaviour
{
    [Header("Arrastra los objetos aquí")]
    public InputField nameInput;  // La caja de texto
    public Button loginButton;    // El botón
    public Text errorText;        // El texto para mensajes

    [Header("Configuración")]
    public string nextSceneName = "GameScene"; // EL NOMBRE EXACTO DE TU ESCENA DE JUEGO

    void Start()
    {
        // Limpiamos el texto de error al iniciar
        if (errorText) errorText.text = "";

        // Le decimos al botón qué hacer cuando le hagan click
        loginButton.onClick.AddListener(HacerLogin);
    }

    void HacerLogin()
    {
        string nombreUsuario = nameInput.text.Trim();

        // 1. Validar que escribió algo
        if (string.IsNullOrEmpty(nombreUsuario))
        {
            errorText.text = "¡Escribe un nombre para jugar!";
            errorText.color = Color.red;
            return;
        }

        if (nombreUsuario.Length < 3)
        {
            errorText.text = "El nombre es muy corto (mínimo 3 letras).";
            errorText.color = Color.red;
            return;
        }

        // 2. Conectar con la Base de Datos
        // Usamos GameSession para registrar al usuario
        if (GameSession.Current != null)
        {
            GameSession.Current.Login(nombreUsuario);

            errorText.text = "¡Bienvenido " + nombreUsuario + "!";
            errorText.color = Color.green;

            // 3. Cambiar de escena después de 1 segundo
            Invoke("CargarJuego", 1.0f);
        }
        else
        {
            Debug.LogError("¡Falta el objeto SystemManagers en la escena!");
            errorText.text = "Error interno del sistema.";
        }
    }

    void CargarJuego()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}