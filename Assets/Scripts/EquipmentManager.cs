using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class EquipmentManager : MonoBehaviour
{
    public event EventHandler<OnEPressedEventArgs> OnEPressed;
    public class OnEPressedEventArgs : EventArgs
    {
        public bool isEquipmentOpen;
    }


    [SerializeField] private GameObject equipment;
    [SerializeField] private Transform equipmentFirstRow;
    [SerializeField] private Transform equipmentSecondRow;
    [SerializeField] private Transform equipmentThirdRow;
    [SerializeField] private Transform equipmentToolBar;
    [SerializeField] private Transform toolBar;
    private bool isEquipmentOpen;
    private int[,] itemId = new int[4,9];

    private void Start()
    {
        for(int x = 0; x < itemId.GetLength(0); x++)
            for(int z=0; z < itemId.GetLength(1); z++)
                itemId[x,z] = BlockData.kindOfBlock["none"];

        Transform temp;
        for (int i = 0; i < 4; i++)    //this loop is made only to test dragging items
        {
            temp = equipmentToolBar.Find("ToolBarInventorySlot (" + i + ")");
            temp = temp.Find("Image");
            temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlockIcons_" + i);
            temp = toolBar.Find("ToolBarSlot (" + i + ")");
            temp = temp.Find("Image");
            temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlockIcons_" + i);
        }

        equipment.SetActive(false);
        isEquipmentOpen = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandlePlayerMovement();
            ToggleCursorVisibility();
        }
    }

    private void HandlePlayerMovement()
    {
        equipment.SetActive(!equipment.activeSelf);
        isEquipmentOpen = equipment.activeSelf;
        OnEPressed?.Invoke(this, new OnEPressedEventArgs { isEquipmentOpen = isEquipmentOpen });
    }
    private void ToggleCursorVisibility()
    {
        if (isEquipmentOpen)
            Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
    }
}
