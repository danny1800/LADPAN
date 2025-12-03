using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProfessorLoginUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject panelPassword; // El panel que contiene el input de contraseña
    public InputField passwordInput;
    public Text errorText;

    [Header("Configuración")]
    public string sceneNameProfesor = "EscenaProfesor"; // Nombre de la escena nueva

    void Start()
    {
        // Asegurarnos de que el panel empiece oculto
        if (panelPassword) panelPassword.SetActive(false);
        if (errorText) errorText.text = "";
    }

    // Llama a esto desde un botón "Soy Profesor" en tu Canvas principal
    public void MostrarPanelPassword()
    {
        panelPassword.SetActive(true);
        passwordInput.text = ""; // Limpiar campo
        errorText.text = "";
    }

    // Llama a esto desde el botón "Cerrar" o "Cancelar" del panel
    public void OcultarPanel()
    {
        panelPassword.SetActive(false);
    }

    // Llama a esto desde el botón "Entrar" dentro del panel
    public void IntentarLogin()
    {
        string pass = passwordInput.text;

        if (DatabaseManager.Instance != null)
        {
            // Intentamos loguear con el usuario "Profesor" y la contraseña escrita
            Usuario profe = DatabaseManager.Instance.Login("Profesor", pass);

            if (profe != null)
            {
                // ¡Éxito!
                GameSession.CurrentUser = profe;
                SceneManager.LoadScene(sceneNameProfesor);
            }
            else
            {
                errorText.text = "Contraseña incorrecta";
                errorText.color = Color.red;
            }
        }
    }
}