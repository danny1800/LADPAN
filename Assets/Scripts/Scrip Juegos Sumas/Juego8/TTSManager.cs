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

                            // Registrar el OnUtteranceProgressListener para saber cuando termina de hablar
                            ttsObject.Call("setOnUtteranceProgressListener", new UtteranceListener(
                                onStartId => { isSpeaking = true; },
                                onDoneId => { isSpeaking = false; },
                                onErrorId => { isSpeaking = false; }
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
    /// Pide que se diga un texto. NO bloqueante. Usa QUEUE_ADD para no cortar lo que ya esté hablando.
    /// </summary>
    public void Speak(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isReady || ttsObject == null)
        {
            Debug.LogWarning("TTS no está listo. Mensaje descartado: " + message);
            return;
        }

        // Generar ID único por frase
        string utteranceId = Guid.NewGuid().ToString();

        // QUEUE_ADD = 1 (no flush)
        int QUEUE_ADD = 1;

        // Llamada a speak(text, queueMode, paramsBundle, utteranceId)
        // Para compatibilidad con versiones antiguas algunos usan null bundle; usamos null
        try
        {
            ttsObject.Call<int>("speak", message, QUEUE_ADD, null, utteranceId);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al llamar a speak: " + ex.Message);
        }
#else
        Debug.Log("TTS dice: " + message);
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
    private class UtteranceListener : AndroidJavaProxy
    {
        private readonly Action<string> onStart;
        private readonly Action<string> onDone;
        private readonly Action<string> onError;

        public UtteranceListener(Action<string> onStart, Action<string> onDone, Action<string> onError)
            : base("android.speech.tts.UtteranceProgressListener")
        {
            this.onStart = onStart;
            this.onDone = onDone;
            this.onError = onError;
        }

        // Firma exacta requerida por la interfaz
        public void onStart(string utteranceId)
        {
            onStart?.Invoke(utteranceId);
        }

        public void onDone(string utteranceId)
        {
            onDone?.Invoke(utteranceId);
        }

        public void onError(string utteranceId)
        {
            onError?.Invoke(utteranceId);
        }
    }
#endif
}

