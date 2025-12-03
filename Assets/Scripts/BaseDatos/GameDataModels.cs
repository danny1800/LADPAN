using SQLite;
using System;

// Tabla de Usuarios (Alumnos y Profesor)
public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Nombre { get; set; } // Nombre del niño o "Profesor"

    public bool EsProfesor { get; set; }
    public string Password { get; set; } // Solo para el profesor

    // --- PREPARACIÓN ONLINE ---
    public string GlobalId { get; set; } // Un ID único (GUID) para cuando subas a la nube
    public DateTime FechaRegistro { get; set; }
}

// Tabla para guardar Puntajes y Niveles de cada minijuego
public class ProgresoJuego
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UsuarioId { get; set; } // Relación con el alumno

    public string GameID { get; set; } // Ej: "JuegoFlores", "JuegoSumas"
    
    public int NivelMaximo { get; set; }
    public int PuntajeAcumulado { get; set; }
    
    // --- PREPARACIÓN ONLINE ---
    public bool Sincronizado { get; set; } // Para saber qué datos faltan subir a la nube
    public DateTime UltimaJugada { get; set; }
}