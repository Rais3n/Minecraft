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

    private bool isGrounded;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        
        Move();
        CheckIfGrounded();
        movementInYAxis();
    }
    private void CheckIfGrounded()
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        int z = (int)transform.position.z;
        if (GetBlock(x,y,z) != BlockData.kindOfBlock["none"])
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

        Vector3 v = transform.position + moveVelocity/1.4f ;
        if(GetBlock((int)v.x, (int)v.y + 1, (int)v.z) == BlockData.kindOfBlock["none"])
        characterController.Move(moveVelocity*speed*Time.deltaTime);

        head.rotation = savedOrientation;
    }
}