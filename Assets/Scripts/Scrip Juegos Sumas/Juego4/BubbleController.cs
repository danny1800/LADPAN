using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleController : MonoBehaviour
{
    public int myValue; // El valor de esta burbuja
    private BubbleManager manager; // Referencia al jefe
    private Text myText;

    void Awake()
    {
        myText = GetComponentInChildren<Text>();
        manager = FindObjectOfType<BubbleManager>(); // Buscar al jefe automáticamente
        GetComponent<Button>().onClick.AddListener(PopBubble); // Auto-conectar el clic
    }

    public void Setup(int val)
    {
        myValue = val;
        myText.text = myValue.ToString();
    }

    void PopBubble()
    {
        if (manager != null && manager.popSound != null)
        {
            manager.GetComponent<AudioSource>().PlayOneShot(manager.popSound);
        }
        if (manager != null)
        {
            manager.ProcessBubble(myValue); // Avisar al jefe

            // Aquí podrías poner sonido o animación de explosión
            Destroy(gameObject); // La burbuja desaparece
        }
    }
}
