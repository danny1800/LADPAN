using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropSloSuma : MonoBehaviour, IDropHandler
{
    public int expectedResult;
    public bool isFilled = false; // Para saber si ya tiene respuesta

    public void OnDrop(PointerEventData eventData)
    {
        // Si ya está lleno, no dejar poner otra encima
        if (isFilled) return;

        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            // Buscamos al Manager para avisarle el resultado
            GameManagerrr manager = FindObjectOfType<GameManagerrr>();

            if (item.numberValue == expectedResult)
            {
                // ACERTO
                Debug.Log("¡Correcto!");
                item.parentAfterDrag = transform;

                // Bloquear la ficha
                item.GetComponent<CanvasGroup>().blocksRaycasts = false;
                isFilled = true;

                // --- NUEVO: ESTO CENTRA LA FICHA PERFECTAMENTE ---
                // Le decimos que su nuevo padre es este Slot
                item.transform.SetParent(transform);
                // Reseteamos su posición local para que quede en el centro exacto (0,0)
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                // ------------------------------------------------

                manager.CheckAnswer(true);
            }
            else
            {
                // FALLO
                Debug.Log("Incorrecto - Game Over");
                manager.CheckAnswer(false); // Avisar al Manager que falló
            }
        }
    }
}