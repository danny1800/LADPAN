using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudentDetailPanel : MonoBehaviour
{
    public Text txtTitulo;   // "Detalles de Pepito"
    public Text txtCuerpo;   // Lista de juegos y records
    public GameObject panelCompleto; // Para ocultarlo/mostrarlo

    public void Abrir(int alumnoId, string nombreAlumno)
    {
        panelCompleto.SetActive(true);
        txtTitulo.text = "Récords de: " + nombreAlumno;

        // Pedir datos a la base de datos
        if (DatabaseManager.Instance != null)
        {
            List<ProgresoJuego> lista = DatabaseManager.Instance.ObtenerStatsAlumno(alumnoId);
            GenerarTexto(lista);
        }
    }

    void GenerarTexto(List<ProgresoJuego> juegos)
    {
        if (juegos.Count == 0)
        {
            txtCuerpo.text = "Sin partidas jugadas.";
            return;
        }

        string resultado = "";
        foreach (var juego in juegos)
        {
            // Limpiamos el nombre (ej: "JuegoSumas" -> "Sumas")
            string nombreJuego = juego.GameID.Replace("Juego", "");

            resultado += $"<b>{nombreJuego}</b>\n";
            resultado += $"   • Nivel Máximo: {juego.NivelMaximo}\n";
            resultado += $"   • Puntos Totales: {juego.PuntajeAcumulado}\n\n";
        }
        txtCuerpo.text = resultado;
    }

    public void Cerrar()
    {
        panelCompleto.SetActive(false);
    }
}