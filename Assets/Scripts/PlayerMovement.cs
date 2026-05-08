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
    public float jumpForwardForce;

    public bool isJump;
    private Rigidbody rb;

    public float rotationSpeed = 720f;
    public GameObject playerObj;
    public GameObject objectPlayerSpawn;

    public Transform cameraTransform; // drag Main Camera ke sini di Inspector
    //public float moveSpeed = 5f;
    //public float rotationSpeed = 10f;
    private Vector2 moveVector_play;
    public bool invertMovement;

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

        if (invertMovement)
        {
            InvertRotation();
        }

        if (!invertMovement)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Hilangkan komponen Y supaya movement tetap di tanah
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // Hitung arah gerak relatif kamera
            Vector3 moveDirection = cameraForward * moveVector.y + cameraRight * moveVector.x;

            if (moveDirection.magnitude > 0.1f)
            {
                // Gerak
                transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);

                // Rotasi ke arah gerak
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Update animasi
            if (animator != null)
                animator.SetFloat("Walking", moveDirection.magnitude);
            //NormalRotation();
        }
        
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

            // arah depan player
            Vector3 forwardDir = transform.forward;

            // velocity lompat + dorongan ke depan
            rb.velocity = new Vector3(
                forwardDir.x * jumpForwardForce,
                jumpForce,
                forwardDir.z * jumpForwardForce
            );

            isJump = false;
        }

        //if (isJump)
        //{
        //    animator.SetBool("Jumping", true);
        //    float direction = transform.localScale.x > 0 ? 1f : -1f;

        //    rb.velocity = new Vector2(rb.velocity.x + direction * jumpForwardForce,jumpForce);
        //    isJump = false;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isJump = true;
            animator.SetBool("Jumping", false);
        }
    }

    public void InvertRotation()
    {
        // Invert input
        Vector3 movement = new Vector3(-moveVector.x, 0, -moveVector.y);

        if (movement.magnitude > 0.1f)
        {
            // Gerakkan karakter ke arah yang sudah dibalik
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.World);

            // Rotasikan karakter ke arah yang sudah dibalik
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("Walking", movement.magnitude);
    }

    public void NormalRotation()
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

        //Vector3 inputDir = new Vector3(moveVector.x, 0, moveVector.y);

        //if (inputDir.magnitude > 0.1f)
        //{
        //    // Hitung arah gerak berdasarkan rotasi karakter
        //    Vector3 moveDir = transform.forward * moveVector.y + transform.right * moveVector.x;

        //    // Gerakkan karakter
        //    transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.World);

        //    // Rotasi mengikuti arah input (kalau mau auto rotate)
        //    Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //}
        animator.SetFloat("Walking", movement.magnitude);
    }
}
