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

    public Usuario UsuarioActivo;

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

        try
        {
            _connection = new SQLiteConnection(_dbPath);
            _connection.CreateTable<Usuario>();
            _connection.CreateTable<ProgresoJuego>();

            Debug.Log("Base de datos iniciada en: " + _dbPath);
            CrearProfesorDefault();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("ERROR CRÍTICO DB: " + ex.Message);
            _connection = null;
        }
    }

    // --- GESTIÓN DE USUARIOS ---

    private void CrearProfesorDefault()
    {
        if (_connection == null) return;

        var profe = _connection.Table<Usuario>().FirstOrDefault(u => u.EsProfesor);
        if (profe == null)
        {
            var nuevoProfe = new Usuario
            {
                Nombre = "Profesor",
                EsProfesor = true,
                Password = "admin",
                GlobalId = System.Guid.NewGuid().ToString(),
                FechaRegistro = System.DateTime.Now
            };
            _connection.Insert(nuevoProfe);
        }
    }

    public bool RegistrarAlumno(string nombre, out Usuario usuarioCreado)
    {
        usuarioCreado = null;
        if (_connection == null) return false;

        var existente = _connection.Table<Usuario>()
                            .FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && !u.EsProfesor);

        if (existente != null) return false;

        usuarioCreado = new Usuario
        {
            Nombre = nombre,
            EsProfesor = false,
            GlobalId = System.Guid.NewGuid().ToString(),
            FechaRegistro = System.DateTime.Now
        };

        _connection.Insert(usuarioCreado);

        UsuarioActivo = usuarioCreado;
        if (GameSession.Current == null) GameSession.Current = new Usuario();
        GameSession.Current = usuarioCreado;

        return true;
    }

    public Usuario Login(string nombre, string password = "")
    {
        if (_connection == null) return null;

        Usuario usuarioEncontrado = null;

        if (EsProfesor(nombre))
        {
            usuarioEncontrado = _connection.Table<Usuario>().FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && u.EsProfesor && u.Password == password);
        }
        else
        {
            usuarioEncontrado = _connection.Table<Usuario>().FirstOrDefault(u => u.Nombre.ToLower() == nombre.ToLower() && !u.EsProfesor);
        }

        if (usuarioEncontrado != null)
        {
            UsuarioActivo = usuarioEncontrado;
            GameSession.Current = usuarioEncontrado;
            Debug.Log("Login exitoso: " + usuarioEncontrado.Nombre);
        }

        return usuarioEncontrado;
    }

    public bool EsProfesor(string nombreCompleto)
    {
        if (_connection == null) return false;
        var profe = _connection.Table<Usuario>()
                    .FirstOrDefault(u => u.Nombre.ToLower() == nombreCompleto.ToLower() && u.EsProfesor);
        return profe != null;
    }

    // --- GESTIÓN DE JUEGO ---

    public void GuardarProgreso(int usuarioId, string gameID, int puntajeSumar, int nivelAlcanzado)
    {
        if (_connection == null) return;

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
            if (puntajeSumar > 0) progreso.PuntajeAcumulado += puntajeSumar;
            if (nivelAlcanzado > progreso.NivelMaximo) progreso.NivelMaximo = nivelAlcanzado;

            progreso.Sincronizado = false;
            progreso.UltimaJugada = System.DateTime.Now;

            _connection.Update(progreso);
        }
    }

    public int LoadLevel(int usuarioId, string gameID)
    {
        if (_connection == null) return 1;
        var progreso = _connection.Table<ProgresoJuego>()
                            .FirstOrDefault(p => p.UsuarioId == usuarioId && p.GameID == gameID);
        return progreso != null ? progreso.NivelMaximo : 1;
    }

    // --- MÉTODOS PUENTE ---

    public void SaveScore(int userId, string gameId, int score)
    {
        GuardarProgreso(userId, gameId, score, 0);
    }

    public void SaveProgress(int userId, int level)
    {
        GuardarProgreso(userId, "JuegoSumas", 0, level);
    }

    // --- ESTADÍSTICAS ---

    public List<Usuario> ObtenerAlumnos()
    {
        if (_connection == null) return new List<Usuario>();
        return _connection.Table<Usuario>().Where(u => !u.EsProfesor).ToList();
    }

    public List<ProgresoJuego> ObtenerStatsAlumno(int alumnoId)
    {
        if (_connection == null) return new List<ProgresoJuego>();
        return _connection.Table<ProgresoJuego>().Where(p => p.UsuarioId == alumnoId).ToList();
    }

    public List<AlumnoRanking> ObtenerRankingAlumnos()
    {
        if (_connection == null) return new List<AlumnoRanking>();

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

// ==========================================
// AQUÍ ESTÁ LA CLASE QUE TE FALTABA
// ==========================================

[System.Serializable]
public class AlumnoRanking
{
    public int Id;
    public string Nombre;
    public int PuntajeTotal;
    public string DetalleJuegos;
}

// NOTA: Si te salen errores diciendo que 'Usuario' o 'ProgresoJuego' no existen,
// DESCOMENTA (quita el /* y */) el código de abajo. 
// Si NO te salen errores de eso, déjalo comentado.

/*
public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nombre { get; set; }
    public bool EsProfesor { get; set; }
    public string Password { get; set; }
    public string GlobalId { get; set; }
    public System.DateTime FechaRegistro { get; set; }
}

public class ProgresoJuego
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string GameID { get; set; }
    public int NivelMaximo { get; set; }
    public int PuntajeAcumulado { get; set; }
    public bool Sincronizado { get; set; }
    public System.DateTime UltimaJugada { get; set; }
}

public static class GameSession 
{
    public static Usuario Current;
}
*/