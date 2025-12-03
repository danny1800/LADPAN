using System;
using System.Collections;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject ttsObject;
    private bool isReady = false;
    private bool isSpeaking = false;
#endif

    // Propiedades públicas
    public bool IsReady
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return isReady;
#else
            return true; // En editor lo tratamos como listo
#endif
        }
    }

    public bool IsSpeaking
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return isSpeaking;
#else
            return false;
#endif
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // Inicializa TextToSpeech con el listener de init
                ttsObject = new AndroidJavaObject(
                    "android.speech.tts.TextToSpeech",
                    activity,
                    new TTSInitListener(result =>
                    {
                        isReady = result == 0;

                        if (isReady)
                        {
                            // Ajustes de voz
                            ttsObject.Call<int>("setPitch", 1);   // tono
                            ttsObject.Call<int>("setSpeechRate", 1); // velocidad

                            // Registrar el OnUtteranceProgressListener
                            ttsObject.Call("setOnUtteranceProgressListener", new UtteranceListener(
                                onStartAction: id => { isSpeaking = true; },
                                onDoneAction: id => { isSpeaking = false; },
                                onErrorAction: id => { isSpeaking = false; }
                            ));
                        }
                    })
                );
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("TTS init error: " + ex.Message);
        }
#endif
    }

    /// <summary>
    /// Pide que se diga un texto. NO bloqueante.
    /// </summary>
    public void Speak(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isReady || ttsObject == null)
        {
            Debug.LogWarning("TTS no está listo. Mensaje descartado: " + message);
            return;
        }

        string utteranceId = Guid.NewGuid().ToString();
        int QUEUE_ADD = 1;

        try
        {
            // speak(text, queueMode, paramsBundle, utteranceId)
            ttsObject.Call<int>("speak", message, QUEUE_ADD, null, utteranceId);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al llamar a speak: " + ex.Message);
        }
#else
        Debug.Log("TTS (Editor) dice: " + message);
#endif
    }

    // Listener para inicializar Android TTS
    private class TTSInitListener : AndroidJavaProxy
    {
        private readonly Action<int> callback;

        public TTSInitListener(Action<int> cb)
            : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            callback = cb;
        }

        public void onInit(int status)
        {
            callback?.Invoke(status);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Listener para detectar inicio/fin de utterances
    // AQUI ESTABA EL ERROR: Conflictos de nombres
    private class UtteranceListener : AndroidJavaProxy
    {
        // Variables renombradas con guion bajo (_)
        private readonly Action<string> _onStart;
        private readonly Action<string> _onDone;
        private readonly Action<string> _onError;

        public UtteranceListener(Action<string> onStartAction, Action<string> onDoneAction, Action<string> onErrorAction)
            : base("android.speech.tts.UtteranceProgressListener")
        {
            _onStart = onStartAction;
            _onDone = onDoneAction;
            _onError = onErrorAction;
        }

        // Métodos obligatorios de Java (estos nombres NO se pueden cambiar)
        public void onStart(string utteranceId)
        {
            // Ejecutamos la variable renombrada
            _onStart?.Invoke(utteranceId);
        }

        public void onDone(string utteranceId)
        {
            _onDone?.Invoke(utteranceId);
        }

        public void onError(string utteranceId)
        {
            _onError?.Invoke(utteranceId);
        }
    }
#endif
}