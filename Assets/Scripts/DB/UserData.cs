using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public string nombre;
    public int edad;
    public string fechaRegistro;

    public Dictionary<string, Puntaje> puntajes = new Dictionary<string, Puntaje>();

    // Constructor vacío (NECESARIO para JsonUtility)
    public UserData() { }

    // Constructor opcional (si lo quieres usar)
    public UserData(string nombre, int edad, string fechaRegistro)
    {
        this.nombre = nombre;
        this.edad = edad;
        this.fechaRegistro = fechaRegistro;
    }
}
