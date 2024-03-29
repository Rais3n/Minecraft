using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private Transform head;
    [SerializeField] private float speed = 6f;
    private float gravity = -25f;
    private Vector3 velocity;
    private float jumpHeight = 1.25f;
    public static Player Instance { get; private set; }
    private bool isGrounded;
    private bool isEquipmentOpen;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        Instance = this;
        isEquipmentOpen = false;
    }

    private void Start()
    {
        EquipmentManager equipmentManager = GameObject.Find("EquipmentManager").GetComponent<EquipmentManager>();
        equipmentManager.OnEPressed += EquipmentManager_OnEPressed;
    }

    private void EquipmentManager_OnEPressed(object sender, EquipmentManager.OnEPressedEventArgs e)
    {
        isEquipmentOpen = e.isEquipmentOpen;
    }

    private void LateUpdate()
    {
        if(!isEquipmentOpen)
            Move();
        CheckIfGrounded();
        movementInYAxis();
    }
    private void CheckIfGrounded()
    {
        int x = (int)transform.position.x;
        if (transform.position.x < 0)
            x--;

        int y = (int)transform.position.y;
        int z = (int)transform.position.z;

        if (transform.position.z < 0)
            z--;
        Vector3Int w = World.Instance.GlobalPosOfFirstChunkInList();
        if (GetBlock(x - w.x,y,z - w.z) != BlockData.kindOfBlock["none"])
        {
            isGrounded = true;
        }
        else isGrounded = false;
    }
    private int GetBlock(int x, int y, int z)
    {
        
        return World.Instance.GetTheBlockWithCoords(x,y,z);
    }
    private void movementInYAxis()
    {
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        if(!isGrounded)
        velocity.y += gravity * Time.deltaTime;
        
        characterController.Move(velocity * Time.deltaTime);
    }
    private void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Transform orientation = head;
        Quaternion savedOrientation = head.rotation;
        Vector3 temp = new Vector3(0, orientation.localEulerAngles.y, 0);
        Vector3 moveDirection;

        orientation.rotation = Quaternion.Euler(temp);
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Vector3 moveVelocity = moveDirection.normalized;

        Vector3 v = transform.position + moveVelocity/1.4f;
        int x = (int)v.x;
        if (v.x < 0)
            x--;

        int z = (int)v.z;
        if(v.z<0)
            z--;

        Vector3Int w = World.Instance.GlobalPosOfFirstChunkInList();
        if (GetBlock((int)x - w.x, (int)v.y + 1, (int)z - w.z) == BlockData.kindOfBlock["none"])
            characterController.Move(moveVelocity*speed*Time.deltaTime);
        head.rotation = savedOrientation;
    }
    public Vector3 GetPlayerGlobalPos()
    {
        return transform.position;
    }
}