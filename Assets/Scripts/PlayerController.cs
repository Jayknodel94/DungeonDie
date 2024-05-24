using FishNet.Object;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5.0f;          // Movement speed
    public float mouseSensitivity = 2.0f;  // Mouse sensitivity for looking around
    public float verticalLookLimit = 80.0f; // Limit to how far the player can look up and down
    
    public GameObject camera;

    private float verticalRotation = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            camera.SetActive(true);
        }
        else
        {
            GetComponent<PlayerController>().enabled = false;
        }
    }

    void Start()
    {
        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
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

        // Movement
        float moveForwardBackward = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float moveLeftRight = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        Vector3 move = transform.right * moveLeftRight + transform.forward * moveForwardBackward;
        transform.position += move;
    }
}
