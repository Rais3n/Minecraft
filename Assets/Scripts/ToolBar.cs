using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    [SerializeField] private Image[] toolBarSlots;
    private int selectedItem;

    private void Start()
    {
        selectedItem = 0;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0)
        {
            toolBarSlots[selectedItem].GetComponent<Outline>().enabled = false;
            if (scroll > 0)
            {
                selectedItem++;
            }
            else
            { 
                selectedItem--;
            }
            

            if (selectedItem == toolBarSlots.Length)
                selectedItem = 0;
            else if(selectedItem < 0)
                selectedItem = toolBarSlots.Length - 1;

            toolBarSlots[selectedItem].GetComponent<Outline>().enabled = true;
        }
    }

}
