using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropSlot_SUMA : MonoBehaviour, IDropHandler
{
    public int expectedResult;
    public bool isFilled = false;

    public void OnDrop(PointerEventData eventData)
    {
        // Si ya hay una ficha correcta aquí, no dejar poner otra
        if (isFilled) return;

        GameObject dropped = eventData.pointerDrag;

        // CORRECCIÓN: Buscamos el script específico de SUMA
        DraggableItem_SUMA item = dropped.GetComponent<DraggableItem_SUMA>();

        if (item != null)
        {
            GameManagerrr_suma manager = FindObjectOfType<GameManagerrr_suma>();

            if (manager == null)
            {
                Debug.LogError("ERROR: No encuentro el 'GameManagerrr_suma' en la escena.");
                return;
            }

            // Verificar si el número es correcto
            if (item.numberValue == expectedResult)
            {
                Debug.Log("¡Correcto!");

                // Avisar a la ficha que este es su nuevo hogar
                item.parentAfterDrag = transform;

                // Bloquear la ficha para que no se mueva más
                item.GetComponent<CanvasGroup>().blocksRaycasts = false;
                isFilled = true;

                // Centrar visualmente la ficha en la caja verde
                item.transform.SetParent(transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                // Sumar acierto en el manager
                manager.CheckAnswer(true);
            }
            else
            {
                // Si el número no coincide
                Debug.Log("Incorrecto");
                manager.CheckAnswer(false);
            }
        }
    }
}