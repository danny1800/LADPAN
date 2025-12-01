using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegistroUI : MonoBehaviour
{
    public TMP_InputField InputNombre;
    public TMP_InputField InputEdad;
    public TMP_Text texto;

    void Start()
    {
        // Mostrar fecha actual automáticamente
        texto.text = DateTime.Now.ToString("yyyy-MM-dd");
    }

    public void GuardarUsuario()
    {
        string nombre = InputNombre.text;
        int edad = int.Parse(InputEdad.text);
        string fechaReg = texto.text;

        UserData nuevo = new UserData();
        nuevo.nombre = nombre;
        nuevo.edad = edad;
        nuevo.fechaRegistro = fechaReg;

        DataBase.GuardarUsuario(nuevo);

        Debug.Log("Usuario guardado correctamente.");
    }
}
