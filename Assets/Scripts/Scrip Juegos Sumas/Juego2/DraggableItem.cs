using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    public int numberValue;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // SEGURIDAD: Si no encontró el canvas al inicio, búscalo ahora
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
        }

        parentAfterDrag = transform.parent;

        // Mover al Canvas para que salga del Layout Group y flote libremente
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling(); // Poner al frente
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ========================================================================
        // INICIO DE LA LÓGICA DE AUTO-AJUSTE (SNAP)
        // ========================================================================

        // 1. BUSCAR TODAS LAS CAJAS (SLOTS)
        DropSlot[] allSlots = FindObjectsOfType<DropSlot>();

        DropSlot closestSlot = null;
        float minDistance = 50f; // Distancia máxima para que el imán funcione
        float currentClosestDist = Mathf.Infinity;

        // 2. ENCONTRAR LA CAJA MÁS CERCANA
        foreach (DropSlot slot in allSlots)
        {
            // Opcional: Si quieres que solo detecte cajas vacías
            // if (!slot.isFilled) 
            {
                float dist = Vector3.Distance(transform.position, slot.transform.position);

                if (dist < minDistance && dist < currentClosestDist)
                {
                    currentClosestDist = dist;
                    closestSlot = slot;
                }
            }
        }

        // 3. DECIDIR QUÉ HACER
        if (closestSlot != null)
        {
            // CORRECCIÓN AQUÍ: Usamos 'GameManagerrr' (con 3 r) que es el que tiene el public void CheckAnswer
            GameManagerrr manager = FindObjectOfType<GameManagerrr>();

            // Verificamos si el número coincide con el resultado esperado de esa caja
            if (closestSlot.expectedResult == numberValue)
            {
                // --- CORRECTO ---
                Debug.Log("¡Correcto por distancia!");

                if (manager != null) manager.CheckAnswer(true);

                // Ajustar (Snap) a la caja
                closestSlot.isFilled = true;
                transform.SetParent(closestSlot.transform);
                rectTransform.anchoredPosition = Vector2.zero; // Se centra perfecto

                // Actualizamos el "padre seguro"
                parentAfterDrag = transform.parent;
            }
            else
            {
                // --- INCORRECTO ---
                Debug.Log("¡Incorrecto! (Estaba cerca pero no es el número)");

                if (manager != null) manager.CheckAnswer(false);

                // Regresar a su lugar original
                RegresarAInicio();
            }
        }
        else
        {
            // No se soltó cerca de nada -> Regresar
            RegresarAInicio();
        }
    }

    private void RegresarAInicio()
    {
        transform.SetParent(parentAfterDrag);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}