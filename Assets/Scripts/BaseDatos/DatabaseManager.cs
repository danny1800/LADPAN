using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;
using System.IO;
using System.Linq;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    private string dbPath;
    private SQLiteConnection connection;

    void Awake()
    {
        // Singleton para acceder fácil desde cualquier minijuego
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        // Define la ruta segura para guardar datos en Android/iOS/PC
        dbPath = Path.Combine(Application.persistentDataPath, "GameDatabase.db");

        // Abre la conexión
        connection = new SQLiteConnection(dbPath);

        // Crea las tablas si no existen
        connection.CreateTable<UserProfile>();
        connection.CreateTable<GameScore>();

        Debug.Log("Base de datos inicializada en: " + dbPath);
    }

    // --- FUNCIONES PARA USUARIOS ---

    public UserProfile RegisterOrLoginUser(string username)
    {
        // Buscar si ya existe
        var existingUser = connection.Table<UserProfile>().Where(u => u.Username == username).FirstOrDefault();

        if (existingUser != null)
        {
            Debug.Log("Usuario logueado: " + username);
            return existingUser;
        }

        // Si no existe, crear uno nuevo
        var newUser = new UserProfile
        {
            Username = username,
            CreatedAt = System.DateTime.Now
        };
        connection.Insert(newUser);
        Debug.Log("Usuario registrado: " + username);

        return newUser;
    }

    // --- FUNCIONES PARA PUNTAJES ---

    public void SaveScore(int userId, string gameName, int score)
    {
        var newScore = new GameScore
        {
            UserId = userId,
            MiniGameName = gameName,
            Score = score,
            DatePlayed = System.DateTime.Now
        };

        connection.Insert(newScore);
        Debug.Log($"Puntaje guardado para {gameName}: {score}");
    }

    // --- FUNCIONES PARA PROGRESO DE NIVEL (NUEVAS) ---
    // Estas son las funciones que tu GameManager estaba buscando y no encontraba

    public void SaveProgress(int userId, int levelIndex)
    {
        // Guardamos el nivel como si fuera un puntaje especial llamado "LevelProgress"
        // Esto evita tener que modificar la tabla de Usuarios y borrar la base de datos vieja
        SaveScore(userId, "LevelProgress", levelIndex);
        Debug.Log($"Progreso guardado: Nivel {levelIndex}");
    }

    public int LoadLevel(int userId)
    {
        // Buscamos el registro más alto de "LevelProgress"
        var lastLevelRecord = connection.Table<GameScore>()
                                        .Where(s => s.UserId == userId && s.MiniGameName == "LevelProgress")
                                        .OrderByDescending(s => s.Score) // Ordenamos para obtener el nivel más alto
                                        .FirstOrDefault();

        if (lastLevelRecord != null)
        {
            return lastLevelRecord.Score; // Retorna el nivel guardado
        }

        return 1; // Si no ha jugado nunca, empieza en nivel 1
    }

    // --- FUNCIONES PARA ESTADÍSTICAS ---

    // Obtener los mejores puntajes de un minijuego específico
    public List<GameScoreView> GetHighScores(string gameName)
    {
        // Hacemos una consulta SQL (JOIN) para unir el nombre del usuario con su puntaje
        string query = @"
            SELECT u.Username, s.Score, s.DatePlayed 
            FROM GameScore s 
            INNER JOIN UserProfile u ON s.UserId = u.Id 
            WHERE s.MiniGameName = ? 
            ORDER BY s.Score DESC 
            LIMIT 10";

        return connection.Query<GameScoreView>(query, gameName);
    }
}

// Clase auxiliar solo para mostrar datos en la tabla (no se guarda en DB)
public class GameScoreView
{
    public string Username { get; set; }
    public int Score { get; set; }
    public System.DateTime DatePlayed { get; set; }
}