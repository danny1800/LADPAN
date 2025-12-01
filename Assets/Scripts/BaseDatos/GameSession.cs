using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Current;
    public UserProfile CurrentUser;

    void Awake()
    {
        if (Current == null)
        {
            Current = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Login(string name)
    {
        CurrentUser = DatabaseManager.Instance.RegisterOrLoginUser(name);
    }

    public void SaveMyScore(string miniGame, int score)
    {
        if (CurrentUser != null)
        {
            DatabaseManager.Instance.SaveScore(CurrentUser.Id, miniGame, score);
        }
        else
        {
            Debug.LogError("No hay usuario logueado para guardar puntaje.");
        }
    }
}
