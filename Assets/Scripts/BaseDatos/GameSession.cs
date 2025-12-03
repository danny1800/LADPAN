using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GameSession.cs
public static class GameSession
{
    // Aquí guardamos quién inició sesión (Alumno o Profe)
    // Es estático para acceder desde cualquier lado sin arrastrar scripts
    public static Usuario CurrentUser;
}
