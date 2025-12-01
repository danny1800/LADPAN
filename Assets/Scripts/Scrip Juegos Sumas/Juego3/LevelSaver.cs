using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSaver : MonoBehaviour
{
    [Header("Configuración Temporal")]
    // Clave para identificar el guardado (útil si tienes varios juegos)
    public string gameKey = "NivelJuegoSuma";

    // ----------------------------------------------------------------------
    // FUNCIÓN 1: GUARDAR
    // El GameManager llama a esto cuando el jugador pasa de nivel o pierde.
    // ----------------------------------------------------------------------
    public void SaveProgress(int levelReached)
    {
        Debug.Log("Intentando guardar nivel: " + levelReached);

        // === ZONA PARA TU COMPAÑERO DE BASE DE DATOS (SQLITE) ===
        /* TODO: Borrar la parte de abajo e insertar código SQL.
           Ejemplo:
           string query = "INSERT OR REPLACE INTO Puntuaciones (Juego, Nivel) VALUES ('" + gameKey + "', " + levelReached + ")";
           dbConnection.Execute(query);
        */

        // === ZONA TEMPORAL (PARA QUE FUNCIONE AHORA) ===
        // Usamos PlayerPrefs para que tú puedas probar el juego hoy mismo.
        // Solo guardamos si el nuevo nivel es mayor al récord anterior.
        int currentRecord = PlayerPrefs.GetInt(gameKey, 1);

        if (levelReached > currentRecord)
        {
            PlayerPrefs.SetInt(gameKey, levelReached);
            PlayerPrefs.Save();
            Debug.Log("¡Nuevo Récord Guardado en PlayerPrefs!: " + levelReached);
        }
        else
        {
            Debug.Log("No se guardó porque no superó el récord anterior (" + currentRecord + ")");
        }
        // =================================================
    }

    // ----------------------------------------------------------------------
    // FUNCIÓN 2: CARGAR (Opcional)
    // Puedes llamar a esto al inicio (Start) para mostrar "Récord: 5"
    // ----------------------------------------------------------------------
    public int LoadProgress()
    {
        // === ZONA PARA TU COMPAÑERO DE BASE DE DATOS (SQLITE) ===
        /* TODO: Consultar la base de datos y retornar el int.
           return dbConnection.QueryInt("SELECT Nivel FROM Puntuaciones WHERE...");
        */

        // === ZONA TEMPORAL ===
        return PlayerPrefs.GetInt(gameKey, 1);
    }

    // Función extra por si necesitas borrar datos para pruebas
    public void ResetData()
    {
        PlayerPrefs.DeleteKey(gameKey);
        Debug.Log("Datos borrados");
    }
}
