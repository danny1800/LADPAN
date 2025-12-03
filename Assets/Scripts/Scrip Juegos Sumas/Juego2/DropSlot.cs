using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public int expectedResult;
    public bool isFilled = false; // Para saber si ya tiene respuesta correcta

    public void OnDrop(PointerEventData eventData)
    {
        // 1. Si ya hay una ficha correcta aquí, no dejar poner otra
        if (isFilled) return;

        GameObject dropped = eventData.pointerDrag;

        // Obtenemos el script genérico (DraggableItem)
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            // 2. Buscamos al Manager
            // NOTA: Asegúrate que tu script manager se llame exactamente 'GameManagerrr'
            GameManagerrr manager = FindObjectOfType<GameManagerrr>();

            // PROTECCIÓN DE SEGURIDAD (Igual que en el script de SUMA)
            if (manager == null)
            {
                Debug.LogError("ERROR CRÍTICO: No encuentro el script 'GameManagerrr' en la escena.");
                return;
            }

            // 3. Verificar si el número es correcto
            if (item.numberValue == expectedResult)
            {
                Debug.Log("¡Correcto!");

                // Avisar a la ficha que este es su nuevo hogar
                item.parentAfterDrag = transform;

                // Bloquear la ficha para que no se mueva más (se queda pegada)
                CanvasGroup cg = item.GetComponent<CanvasGroup>();
                if (cg != null) cg.blocksRaycasts = false;

                isFilled = true;

                // --- CENTRADO PERFECTO (SNAP) ---
                item.transform.SetParent(transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                // --------------------------------

                // Sumar acierto en el manager
                manager.CheckAnswer(true);
            }
            else
            {
                // 4. Si el número no coincide
                Debug.Log("Incorrecto");
                manager.CheckAnswer(false);
            }
        }
    }
}