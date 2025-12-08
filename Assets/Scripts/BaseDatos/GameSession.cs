using UnityEngine;

public static class GameSession
{
    // Esta es la variable principal
    public static Usuario CurrentUser;

    // ESTO ES EL TRUCO:
    // Creamos una propiedad "Current" que apunta a "CurrentUser".
    // Así, si un script pide Current, se le da CurrentUser.
    // Si otro pide CurrentUser, también funciona.
    public static Usuario Current
    {
        get { return CurrentUser; }
        set { CurrentUser = value; }
    }
}