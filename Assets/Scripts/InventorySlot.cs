using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        if (eventData.pointerDrag != null)
        {
            bool isItem;
            Transform transform = this.transform.GetChild(0);
            isItem = transform.GetComponent<Image>().IsActive();

            if (!isItem)
            {
                DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
                transform.SetParent(draggableItem.GetParent());

                transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                transform.GetComponent<RectTransform>().localScale = new Vector3(0.81f, 0.81f, 0.81f);

                draggableItem.AssignParent(this.transform);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DraggableItem.IsDraggedItem())
        {
            Debug.Log("Slot clicked!");
            Transform draggedItemTransform = DraggableItem.GetDraggedItem();
            DraggableItem draggedItem = draggedItemTransform.GetComponent<DraggableItem>();
            if (transform.childCount != 0)
            {
                Transform image = transform.GetChild(0);
                
                
                image.SetParent(draggedItem.GetParent());
                draggedItem.ToggleIsDragging();
                draggedItemTransform.SetParent(transform);

                image.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                image.GetComponent<RectTransform>().localScale = new Vector3(0.81f, 0.81f, 0.81f);
            }
            else
            {
                draggedItemTransform.SetParent(transform);
                draggedItem.ToggleIsDragging();
            }
            draggedItemTransform.GetComponent<Image>().raycastTarget = true;
            draggedItemTransform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            draggedItemTransform.GetComponent<RectTransform>().localScale = new Vector3(0.81f, 0.81f, 0.81f);
        }
    }
}
