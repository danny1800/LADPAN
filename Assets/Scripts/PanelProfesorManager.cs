using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PanelProfesorManager : MonoBehaviour
{
    [Header("Conexiones")]
    public Transform contenedorLista; // Dónde se crean las filas
    public GameObject prefabFila;     // El diseño de la fila
    public StudentDetailPanel panelDetalle; // La ventana emergente

    void Start()
    {
        // Al iniciar, ocultamos el detalle y cargamos la lista
        if (panelDetalle) panelDetalle.Cerrar();
        CargarLista();
    }

    void CargarLista()
    {
        // 1. Borrar lista anterior (limpieza)
        foreach (Transform child in contenedorLista) Destroy(child.gameObject);

        if (DatabaseManager.Instance == null) return;

        // 2. Obtener datos
        var ranking = DatabaseManager.Instance.ObtenerRankingAlumnos();

        // 3. Crear filas
        foreach (var alumno in ranking)
        {
            GameObject nuevaFila = Instantiate(prefabFila, contenedorLista);

            FilaAlumnoUI script = nuevaFila.GetComponent<FilaAlumnoUI>();
            if (script != null)
            {
                // Le pasamos "this" para que la fila pueda llamar a este manager
                script.Configurar(alumno.Id, alumno.Nombre, alumno.PuntajeTotal, this);
            }
        }
    }

    // Esta función es llamada por la fila al hacer clic
    public void MostrarDetalleAlumno(int id, string nombre)
    {
        panelDetalle.Abrir(id, nombre);
    }

    public void Salir()
    {
        SceneManager.LoadScene("LoginScene"); // Cambia por el nombre de tu login
    }
}