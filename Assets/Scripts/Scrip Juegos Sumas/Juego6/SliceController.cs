using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliceController : MonoBehaviour
{
    public bool isSelected = false;
    private Image myImage;

    // COLORES
    // Blanco puro para cuando NO está seleccionado
    private Color defaultColor = new Color(1f, 1f, 1f, 1f);
    // Azul (puedes cambiarlo aquí) para cuando SÍ está seleccionado
    private Color activeColor = new Color(0f, 0.6f, 1f, 1f);

    void Awake()
    {
        myImage = GetComponent<Image>();

        // --- CORRECCIÓN CLAVE ---
        // Cambiamos a 1f. Esto significa que solo detectará el clic si el píxel es TOTALMENTE visible.
        // Ayuda a evitar que bordes transparentes o sombras "roben" el clic.
        // REQUIERE: Que la textura tenga "Read/Write Enabled" en el Inspector.
        try
        {
            myImage.alphaHitTestMinimumThreshold = 1f;
        }
        catch
        {
            Debug.LogError("ERROR: Recuerda activar 'Read/Write Enabled' en la configuración de la imagen " + myImage.sprite.name);
        }
    }

    public void Setup(float fillAmount, float rotationZ)
    {
        myImage.fillAmount = fillAmount;

        // Aplicamos la rotación
        transform.localEulerAngles = new Vector3(0, 0, rotationZ);

        // Asignamos un nombre único para identificarlo en la consola si hay errores
        gameObject.name = "Rebanada_Rot_" + rotationZ;

        // Asegurarnos de que la escala sea 1 para evitar deformaciones
        transform.localScale = Vector3.one;

        // Reiniciar estado visual
        isSelected = false;
        myImage.color = defaultColor;
    }

    public void OnClick()
    {
        // Cambiamos el estado (si era true pasa a false, y viceversa)
        isSelected = !isSelected;

        // Cambiamos el color
        myImage.color = isSelected ? activeColor : defaultColor;

        // --- DEPURACIÓN ---
        // Esto aparecerá en la consola. Si haces clic en la derecha pero dice "Rebanada_Rot_0" (que es la de arriba),
        // sabrás que hay una superposición invisible.
        Debug.Log($"Clic registrado en: {gameObject.name} | Estado actual: {isSelected}");
    }
}