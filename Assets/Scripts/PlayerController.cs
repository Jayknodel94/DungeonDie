using FishNet.Object;
using System;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public Transform groundCheck;
    public LayerMask groundMask;
    public new GameObject camera;
    public GameObject UI_GameObject;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -19.62f;
    public float groundDistance = 0.4f;
    public float jumpHeight = 3f;
    public float sprintIncrease = 1.3f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2.0f;  // Mouse sensitivity for looking around
    public float verticalLookLimit = 80.0f; // Limit to how far the player can look up and down

    [Header("Interaction Settings")]
    public float interactionRange = 3f; // How far away from the player an item can be interacted with

    public bool canLook = true;

    CharacterController controller;
    PlayerUiController playerUiController;
    Animator animator;
    Vector3 velocity;
    bool isGrounded;
    float speed;
    float verticalRotation = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            camera.SetActive(true);
            UI_GameObject.SetActive(true);

            GetComponent<PlayerController>().enabled = true;
            GetComponent<CombatController>().enabled = true;
            UI_GameObject.GetComponent<PlayerUiController>().enabled = true;

            playerUiController = UI_GameObject.GetComponent<PlayerUiController>();
        }
    }

    void Start()
    {
        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        speed = walkSpeed;
    }

    void Update()
    {
        Movement();
        if (canLook) MouseLook();

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(Controls.Interact)) // E
        {
            // did you hit anything?
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, interactionRange))

                print(hit.transform.name);
            {
                // did we hit an Item?
                if (hit.collider && hit.transform.TryGetComponent<Item>(out Item item))
                {
                    // do we have room in our inventory?
                    if (playerUiController.GetRoomInInventory() > 0)
                    {
                        // add item to inventory
                        playerUiController.AddItemToInventory(item);

                        // delete item from ground
                        DespawnItem(item.gameObject);
                    }
                }
            }
        }
    }

    private void MouseLook()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player left and right
        transform.Rotate(0, mouseX, 0);

        // Rotate the camera up and down
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
        camera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        HandleSprint();

        controller.Move(speed * Time.deltaTime * move);

        AnimateMovement();
        
        HandleJumping();
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(Controls.Jump) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleSprint()
    {
        if (Input.GetKeyDown(Controls.Sprint))
        {
            speed = runSpeed;
        }
        else if (Input.GetKeyUp(Controls.Sprint))
        {
            speed = walkSpeed;
        }
    }

    private void AnimateMovement()
    {
        float speedPercent = controller.velocity.magnitude / runSpeed;
        animator.SetFloat("speed", speedPercent, .1f, Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnItem(GameObject item)
    {
        ServerManager.Despawn(item);
    }
}
