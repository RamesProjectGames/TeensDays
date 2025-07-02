using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    private Player inputActions;
    Vector2 moveVector;

    public float moveSpeed;
    public float jumpForce;

    public bool isJump;
    private Rigidbody rb;

    //public float rotationSpeed = 720f;

    // Start is called before the first frame update

    private void Awake()
    {
        inputActions = new Player();
        Instance = this;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void OnEnable()
    {
        inputActions.Movement.Enable();
        inputActions.Movement.JoystickInput.performed += ctx => moveVector = ctx.ReadValue<Vector2>();
        inputActions.Movement.JoystickInput.canceled += ctx => moveVector = Vector2.zero;
        //inputActions.Movement.Jump.performed += ctx => Jump();
    }

    private void OnDisable()
    {
        inputActions.Movement.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(moveVector.x, 0, moveVector.y);

        movement.Normalize();
        transform.Translate(moveSpeed * movement * Time.deltaTime, Space.World);
    }

    public void InputPlayer(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
    }

    public void Jump()
    {
        if (isJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isJump = true;
        }
    }
}
