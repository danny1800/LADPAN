using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOrientacion : MonoBehaviour
{
    [Header("¿Cómo debe verse esta escena?")]
    public bool esHorizontal = false; // Marcar casillas para juegos horizontales

    void Start()
    {
        VerificarOrientacion();
    }

    void VerificarOrientacion()
    {
        if (esHorizontal)
        {
            // Fuerza la pantalla a ponerse horizontal (tumbada)
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // Opcional: Si quieres que el usuario pueda girar el celular al otro lado horizontal
            // Descomenta la siguiente línea y borra la anterior:
            // Screen.orientation = ScreenOrientation.AutoRotation;
            // Screen.autorotateToPortrait = false;
            // Screen.autorotateToPortraitUpsideDown = false;
            // Screen.autorotateToLandscapeLeft = true;
            // Screen.autorotateToLandscapeRight = true;
        }
        else
        {
            // Fuerza la pantalla a ponerse vertical (pie)
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}
