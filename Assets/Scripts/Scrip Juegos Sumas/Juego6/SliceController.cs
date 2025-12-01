using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliceController : MonoBehaviour
{
    public bool isSelected = false;
    private Image myImage;

    // Usamos un gris muy claro para deseleccionado, para que se note la diferencia con el fondo si quieres
    private Color defaultColor = new Color(1f, 1f, 1f, 1f); // Blanco puro
    private Color activeColor = new Color(0f, 0.6f, 1f, 1f); // Azul

    void Awake()
    {
        myImage = GetComponent<Image>();

        // ESTA ES LA CLAVE DEL CLIC:
        // Requiere que el Sprite "BaseCircle" tenga "Read/Write Enabled" activado en el editor.
        myImage.alphaHitTestMinimumThreshold = 0.5f;
    }

    public void Setup(float fillAmount, float rotationZ)
    {
        myImage.fillAmount = fillAmount;
        transform.localEulerAngles = new Vector3(0, 0, rotationZ);

        // Asegurarnos de que la escala sea 1 (a veces se deforma)
        transform.localScale = Vector3.one;

        // Reiniciar estado
        isSelected = false;
        myImage.color = defaultColor;
    }

    public void OnClick()
    {
        isSelected = !isSelected;
        myImage.color = isSelected ? activeColor : defaultColor;
        Debug.Log("Clic en rebanada. Estado: " + isSelected);
    }
}
