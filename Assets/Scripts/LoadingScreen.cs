using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class LoadingScreen : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI porcentajeTexto;

    void Start()
    {
        StartCoroutine(CargarEscena());
    }

    IEnumerator CargarEscena()
    {
        // Empezar carga real
        AsyncOperation operacion = SceneManager.LoadSceneAsync("Menu");
        operacion.allowSceneActivation = false;

        float cargaFalsa = 0f;

        while (!operacion.isDone)
        {
            // Unity carga hasta el 90%
            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);

            // Simulación de carga para que se vea bonito
            cargaFalsa = Mathf.MoveTowards(cargaFalsa, progresoReal, Time.deltaTime * 0.5f);

            slider.value = cargaFalsa;
            porcentajeTexto.text = Mathf.RoundToInt(cargaFalsa * 100f) + "%";

            // Cuando ya está en 100%
            if (cargaFalsa >= 1f)
            {
                yield return new WaitForSeconds(0.5f); // Pequeña pausa final
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

