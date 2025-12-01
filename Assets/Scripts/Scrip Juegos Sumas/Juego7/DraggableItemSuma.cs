using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItemSuma : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        Debug.Log("Intentando arrastrar..."); // Esto saldrá en consola si el clic funciona

        // SEGURIDAD: Si por alguna razón no encontró el canvas al inicio, búscalo ahora
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            // Si sigue nulo, busca cualquiera en la escena (Plan C)
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
        }

        parentAfterDrag = transform.parent;

        // Mover al Canvas para que salga del Layout Group y flote libremente
        transform.SetParent(canvas.transform);

        transform.SetAsLastSibling(); // Poner al frente de todo
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
        // Si no encontró slot (sigue siendo hijo del canvas), regresa a casa
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(parentAfterDrag);
            // Reiniciar posición local para que el Layout Group lo acomode
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}