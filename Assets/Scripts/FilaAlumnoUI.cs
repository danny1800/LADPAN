using UnityEngine;
using UnityEngine.UI;

public class FilaAlumnoUI : MonoBehaviour
{
    public Text txtNombre;
    public Text txtPuntosTotal;

    private int _alumnoId;
    private string _nombre;
    private PanelProfesorManager _manager;

    public void Configurar(int id, string nombre, int puntos, PanelProfesorManager manager)
    {
        _alumnoId = id;
        _nombre = nombre;
        _manager = manager;

        txtNombre.text = nombre;
        txtPuntosTotal.text = puntos + " PTS";
    }

    // Esta función la conectaremos al botón del prefab
    public void AlHacerClick()
    {
        if (_manager != null)
        {
            _manager.MostrarDetalleAlumno(_alumnoId, _nombre);
        }
    }
}