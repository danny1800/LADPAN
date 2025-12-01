using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem_SUMA : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    public int numberValue;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // --- LÓGICA DE DETECCIÓN POR DISTANCIA ---

        DropSlot_SUMA[] allSlots = FindObjectsOfType<DropSlot_SUMA>();

        DropSlot_SUMA closestSlot = null;
        float minDistance = 50f; // Distancia máxima para detectar
        float currentClosestDist = Mathf.Infinity;

        // 1. Buscamos cuál es la caja más cercana (sea la correcta o no)
        foreach (DropSlot_SUMA slot in allSlots)
        {
            if (!slot.isFilled) // Solo revisamos cajas vacías
            {
                float dist = Vector3.Distance(transform.position, slot.transform.position);

                // Si está dentro del rango y es más cercana que la anterior encontrada
                if (dist < minDistance && dist < currentClosestDist)
                {
                    currentClosestDist = dist;
                    closestSlot = slot;
                }
            }
        }

        // 2. Evaluamos qué hacer
        if (closestSlot != null)
        {
            // ¡ENCONTRAMOS UNA CAJA CERCA!
            GameManagerrr_suma manager = FindObjectOfType<GameManagerrr_suma>();

            if (closestSlot.expectedResult == numberValue)
            {
                // --- CORRECTO ---
                Debug.Log("¡Correcto por distancia!");
                if (manager != null) manager.CheckAnswer(true);

                // Pegar la ficha
                closestSlot.isFilled = true;
                transform.SetParent(closestSlot.transform);
                rectTransform.anchoredPosition = Vector2.zero;
                parentAfterDrag = transform.parent;
            }
            else
            {
                // --- INCORRECTO ---
                Debug.Log("¡Incorrecto por distancia!");
                if (manager != null) manager.CheckAnswer(false);

                // Regresar a casa (porque falló)
                transform.SetParent(parentAfterDrag);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // No se soltó cerca de ninguna caja -> Regresar a casa sin penalización
            transform.SetParent(parentAfterDrag);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}