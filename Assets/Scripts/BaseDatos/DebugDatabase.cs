using UnityEngine;
using System.Collections.Generic;

public class DebugDatabase : MonoBehaviour
{
    void Update()
    {
        // Al presionar la tecla "P" (de Prueba), mostramos los datos
        if (Input.GetKeyDown(KeyCode.P))
        {
            MostrarDatosEnConsola();
        }
    }

    void MostrarDatosEnConsola()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("No hay DatabaseManager.");
            return;
        }

        // 1. Mostrar la ruta del archivo (Para que sepas dónde buscarlo luego)
        Debug.LogWarning("📂 RUTA DE LA BASE DE DATOS: " + Application.persistentDataPath);

        // 2. Ver Usuarios
        var alumnos = DatabaseManager.Instance.ObtenerAlumnos();
        Debug.Log($"--- USUARIOS ({alumnos.Count}) ---");
        foreach (var alumno in alumnos)
        {
            Debug.Log($"ID: {alumno.Id} | Nombre: {alumno.Nombre} | Pass: {alumno.Password}");

            // 3. Ver Progreso de este alumno
            var stats = DatabaseManager.Instance.ObtenerStatsAlumno(alumno.Id);
            foreach (var stat in stats)
            {
                Debug.Log($"   >>> Juego: {stat.GameID} | Nivel Max: {stat.NivelMaximo} | Puntos: {stat.PuntajeAcumulado}");
            }
        }
    }
}