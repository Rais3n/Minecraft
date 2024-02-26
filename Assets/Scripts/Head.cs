using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{

    private float cameraVerticalRotation = 0f;
    private float cameraHorizontalRotation = 0f;
    private bool isEquipmentOpen;

    private void Start()
    {
        EquipmentManager equipmentManager = GameObject.Find("EquipmentManager").GetComponent<EquipmentManager>();
        equipmentManager.OnEPressed += EquipmentManager_OnEPressed;
        isEquipmentOpen = false;
    }

    private void EquipmentManager_OnEPressed(object sender, EquipmentManager.OnEPressedEventArgs e)
    {
        isEquipmentOpen = e.isEquipmentOpen;
    }

    private void Update()
    {
        RotateHead();
    }

    private void RotateHead()
    {
        if (!isEquipmentOpen)
        {
            float inputX = Input.GetAxis("Mouse X");
            float inputY = Input.GetAxis("Mouse Y");
            cameraVerticalRotation -= inputY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
            cameraHorizontalRotation -= inputX;
            transform.localEulerAngles = Vector3.right * cameraVerticalRotation - Vector3.up * cameraHorizontalRotation;
        }
    }
}
