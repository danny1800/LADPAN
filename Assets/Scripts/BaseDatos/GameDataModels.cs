using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;
using System;

// Tabla de Usuarios
public class UserProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique] // Evita nombres duplicados
    public string Username { get; set; }

    public DateTime CreatedAt { get; set; }
}

// Tabla de Puntajes (Historial de partidas)
public class GameScore
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; } // Relación con el usuario
    public string MiniGameName { get; set; } // Ejemplo: "Carreras", "Puzzle", etc.
    public int Score { get; set; }
    public DateTime DatePlayed { get; set; }
}
