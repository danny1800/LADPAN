using UnityEngine;
using UnityEngine.UI;
using System;

public class UIRegistroEjemplo : MonoBehaviour
{
    [Header("UI Referencias")]
    public InputField nombre;
    public InputField edad;
    public Text fechaActual;

    void Start()
    {
        // Mostrar fecha actual automáticamente
        if (fechaActual)
            fechaActual.text = DateTime.Now.ToString("yyyy-MM-dd");
    }

    public void Registrar()
    {
        // 1. Validaciones visuales
        if (string.IsNullOrEmpty(nombre.text))
        {
            Debug.LogError("El nombre no puede estar vacío");
            return;
        }

        // Parseo seguro de la edad (para que no crashee si está vacío)
        int edadNum = 0;
        int.TryParse(edad.text, out edadNum);

        // 2. Conexión con el nuevo DatabaseManager
        if (DatabaseManager.Instance != null)
        {
            Usuario nuevoUsuario;

            // Llamamos a la función de registro
            bool exito = DatabaseManager.Instance.RegistrarAlumno(nombre.text, out nuevoUsuario);

            if (exito)
            {
                Debug.Log($"Usuario {nuevoUsuario.Nombre} registrado con éxito. ID: {nuevoUsuario.Id}");

                // Opcional: Limpiar campos después de registrar
                nombre.text = "";
                edad.text = "";
            }
            else
            {
                Debug.LogWarning("Ese usuario ya existe en la base de datos.");
            }
        }
        else
        {
            Debug.LogError("Error: No se encontró DatabaseManager en la escena.");
        }
    }
}