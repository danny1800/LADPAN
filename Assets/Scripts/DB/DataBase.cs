using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataBase : MonoBehaviour
{
    private static string filePath = Application.persistentDataPath + "/user.json";

    public static void GuardarUsuario(UserData datos)
    {
        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Usuario guardado en: " + filePath);
    }

    public static UserData CargarUsuario()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<UserData>(json);
        }
        else
        {
            return null;
        }
    }

    public static bool ExisteUsuario()
    {
        return File.Exists(filePath);
    }

    public static void GuardarPuntaje(string nombreJuego, int nuevoPuntaje)
    {
        UserData datos = CargarUsuario();

        if (datos == null)
            return;

        // Si no existe el juego, crearlo
        if (!datos.puntajes.ContainsKey(nombreJuego))
        {
            datos.puntajes[nombreJuego] = new Puntaje();
        }

        Puntaje pj = datos.puntajes[nombreJuego];

        // Guardar en historial
        pj.historial.Add(nuevoPuntaje);

        // Actualizar mejor puntaje
        if (nuevoPuntaje > pj.mejorPuntaje)
        {
            pj.mejorPuntaje = nuevoPuntaje;
        }

        GuardarUsuario(datos); // guardamos el JSON
    }

    public static Puntaje ObtenerPuntaje(string nombreJuego)
    {
        UserData datos = CargarUsuario();

        if (datos != null && datos.puntajes.ContainsKey(nombreJuego))
            return datos.puntajes[nombreJuego];

        return null;
    }
}
