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
    public Animator animator;
    public RunSystem runSystem;
    Vector2 moveVector;

    public float moveSpeed;
    public float jumpForce;

    public bool isJump;
    private Rigidbody rb;

    public float rotationSpeed = 720f;
    public GameObject playerObj;

    // Start is called before the first frame update

    private void Awake()
    {
        inputActions = new Player();
        Instance = this;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }
    
    private void OnEnable()
    {
        inputActions.Movement.Enable();
        inputActions.Movement.JoystickInput.performed += ctx => moveVector = ctx.ReadValue<Vector2>();
        inputActions.Movement.JoystickInput.canceled += ctx => moveVector = Vector2.zero;
        inputActions.Movement.Jump.performed += ctx => Jump();
        inputActions.Movement.Run.started += ctx => runSystem.StartRunning();
        inputActions.Movement.Run.canceled += ctx => runSystem.StopRunning();
    }

    private void OnDisable()
    {
        inputActions.Movement.Disable();
        inputActions.Movement.Run.canceled -= ctx => runSystem.StartRunning();
        inputActions.Movement.Run.canceled -= ctx => runSystem.StopRunning();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(moveVector.x, 0, moveVector.y);

        //movement.Normalize();
        transform.Translate(moveSpeed * movement.normalized * Time.deltaTime, Space.World);

        if (movement.magnitude > 0.1f)
        {
            // ROTASI
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            playerObj.transform.localRotation = Quaternion.RotateTowards(playerObj.transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("Walking", movement.magnitude);
    }

    public void InputPlayer(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
    }

    public void Jump()
    {
        if (isJump)
        {
            animator.SetBool("Jumping", true);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isJump = true;
            animator.SetBool("Jumping", false);
        }
    }
}
