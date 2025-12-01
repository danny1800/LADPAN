using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegacionMenu : MonoBehaviour
{
    // Esta función recibe el nombre exacto del archivo de la escena
    public void IrAEscena(string nombreEscena)
    {
        // Verifica si la escena está agregada al Build Settings antes de cargarla
        if (Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("Error: La escena '" + nombreEscena + "' no se encuentra o no está en el Build Settings.");
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo de la aplicación...");
        Application.Quit();
    }
}
