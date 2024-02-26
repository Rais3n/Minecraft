using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
{
    private Image image;
    private static Transform parentAfterDrag;
    private static Transform draggedItem;
    private static bool isDragging = false;

    private void Update()
    {
        if(isDragging)
        {
            draggedItem.position = Input.mousePosition;
        }
    }
    public void ToggleIsDragging()
    {
        isDragging = !isDragging;
    }
    public static Transform GetDraggedItem()
    {
        return draggedItem;
    }
    public static bool IsDraggedItem()
    {
        return isDragging;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("DOWN");
        if (!isDragging)
        {
            draggedItem = transform;
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image = GetComponent<Image>();
            image.raycastTarget = false;
            isDragging = true;
        }
        else
        {
            draggedItem.GetComponent<DraggableItem>().image.raycastTarget = true;
            draggedItem.SetParent(transform.parent);
            draggedItem.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            draggedItem.GetComponent<RectTransform>().localScale = new Vector3(0.81f, 0.81f, 0.81f);

            draggedItem = transform;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image = GetComponent<Image>();
            image.raycastTarget = false;
        }
    }
    public void AssignParent(Transform transform)
    {
        parentAfterDrag = transform;
    }
    public Transform GetParent()
    {
        return parentAfterDrag;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

        Debug.Log("Begin drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        Debug.Log("End drag");
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        eventData.pointerDrag.GetComponent<RectTransform>().localScale = new Vector3(0.81f, 0.81f, 0.81f);
    }
}
