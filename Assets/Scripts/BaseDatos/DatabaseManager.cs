using UnityEngine;
using SQLite;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    private SQLiteConnection _connection;
    private string _dbPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            IniciarBaseDeDatos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void IniciarBaseDeDatos()
    {
        string fileName = "EscuelaGames.db";
        _dbPath = Path.Combine(Application.persistentDataPath, fileName);

        _connection = new SQLiteConnection(_dbPath);

        // Crear tablas si no existen
        _connection.CreateTable<Usuario>();
        _connection.CreateTable<ProgresoJuego>();

        Debug.Log("Base de datos iniciada en: " + _dbPath);
        CrearProfesorDefault();
    }

    // --- GESTIÓN DE USUARIOS ---

    private void CrearProfesorDefault()
    {
        // Verificar si ya existe el profe
        var profe = _connection.Table<Usuario>().FirstOrDefault(u => u.EsProfesor);
        if (profe == null)
        {
            var nuevoProfe = new Usuario
            {
                Nombre = "Profesor",
                EsProfesor = true,
                Password = "admin", // Contraseña simple
                GlobalId = System.Guid.NewGuid().ToString(),
                FechaRegistro = System.DateTime.Now
            };
            _connection.Insert(nuevoProfe);
            Debug.Log("Profesor creado por defecto (Pass: admin)");
        }
    }

    public bool RegistrarAlumno(string nombre, out Usuario usuarioCreado)
    {
        usuarioCreado = null;

        // Validar duplicados (Case Insensitive)
        var existente = _connection.Table<Usuario>()
                            .FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && !u.EsProfesor);

        if (existente != null) return false; // Ya existe

        usuarioCreado = new Usuario
        {
            Nombre = nombre,
            EsProfesor = false,
            GlobalId = System.Guid.NewGuid().ToString(),
            FechaRegistro = System.DateTime.Now
        };

        _connection.Insert(usuarioCreado);
        return true;
    }

    public Usuario Login(string nombre, string password = "")
    {
        // Si intenta entrar como profesor (revisa si es profesor en la BD)
        if (EsProfesor(nombre))
        {
            return _connection.Table<Usuario>().FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && u.EsProfesor && u.Password == password);
        }
        else
        {
            return _connection.Table<Usuario>().FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && !u.EsProfesor);
        }
    }

    public bool EsProfesor(string nombreCompleto)
    {
        var profe = _connection.Table<Usuario>()
                    .FirstOrDefault(u => u.Nombre.ToLower() == nombreCompleto.ToLower() && u.EsProfesor);

        return profe != null;
    }

    // --- GESTIÓN DE JUEGO ---

    public void GuardarProgreso(int usuarioId, string gameID, int puntajeSumar, int nivelAlcanzado)
    {
        var progreso = _connection.Table<ProgresoJuego>()
                            .FirstOrDefault(p => p.UsuarioId == usuarioId && p.GameID == gameID);

        if (progreso == null)
        {
            progreso = new ProgresoJuego
            {
                UsuarioId = usuarioId,
                GameID = gameID,
                NivelMaximo = nivelAlcanzado,
                PuntajeAcumulado = puntajeSumar,
                Sincronizado = false,
                UltimaJugada = System.DateTime.Now
            };
            _connection.Insert(progreso);
        }
        else
        {
            progreso.PuntajeAcumulado += puntajeSumar;
            if (nivelAlcanzado > progreso.NivelMaximo)
            {
                progreso.NivelMaximo = nivelAlcanzado;
            }
            progreso.Sincronizado = false;
            progreso.UltimaJugada = System.DateTime.Now;

            _connection.Update(progreso);
        }
    }

    public int LoadLevel(int usuarioId, string gameID)
    {
        var progreso = _connection.Table<ProgresoJuego>()
                            .FirstOrDefault(p => p.UsuarioId == usuarioId && p.GameID == gameID);

        return progreso != null ? progreso.NivelMaximo : 1;
    }

    // --- PARA EL PROFESOR (ESTADÍSTICAS) ---

    public List<Usuario> ObtenerAlumnos()
    {
        return _connection.Table<Usuario>().Where(u => !u.EsProfesor).ToList();
    }

    public List<ProgresoJuego> ObtenerStatsAlumno(int alumnoId)
    {
        return _connection.Table<ProgresoJuego>().Where(p => p.UsuarioId == alumnoId).ToList();
    }

    // --- NUEVO: RANKING Y CLASE AUXILIAR ---

    public List<AlumnoRanking> ObtenerRankingAlumnos()
    {
        List<AlumnoRanking> listaRanking = new List<AlumnoRanking>();

        var alumnos = _connection.Table<Usuario>().Where(u => u.EsProfesor == false).ToList();

        foreach (var alumno in alumnos)
        {
            var progresos = _connection.Table<ProgresoJuego>()
                                .Where(p => p.UsuarioId == alumno.Id).ToList();

            int totalPuntos = 0;
            string detalles = "";

            foreach (var juego in progresos)
            {
                totalPuntos += juego.PuntajeAcumulado;
                string nombreJuego = juego.GameID.Replace("Juego", "");
                detalles += $"{nombreJuego}: {juego.PuntajeAcumulado} pts | ";
            }

            listaRanking.Add(new AlumnoRanking
            {
                Id = alumno.Id,
                Nombre = alumno.Nombre,
                PuntajeTotal = totalPuntos,
                DetalleJuegos = detalles
            });
        }

        return listaRanking.OrderByDescending(x => x.PuntajeTotal).ToList();
    }
}

// Clase auxiliar para el reporte (fuera de la clase DatabaseManager, pero en el mismo archivo)
public class AlumnoRanking
{
    public int Id;
    public string Nombre;
    public int PuntajeTotal;
    public string DetalleJuegos;
}