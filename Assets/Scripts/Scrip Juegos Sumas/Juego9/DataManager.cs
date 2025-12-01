using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GuardarMaxPuntuacion(int nuevoPuntaje)
    {
        int puntajeActual = PlayerPrefs.GetInt("HighScore", 0);
        if (nuevoPuntaje > puntajeActual)
        {
            PlayerPrefs.SetInt("HighScore", nuevoPuntaje);
            PlayerPrefs.Save();
            Debug.Log("¡Nuevo récord guardado!");
        }
    }

    public int ObtenerMaxPuntuacion()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }
}
