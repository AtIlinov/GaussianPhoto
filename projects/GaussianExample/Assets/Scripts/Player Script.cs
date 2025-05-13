using UnityEngine;
using System;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    // Variables for character movement \\
    private Rigidbody rb;
    public Vector2 moveInput;
    public float speed = 5f;
    //----------------------------\\

    // Variables for camera movement \\
    // public float sensitivity = 50f;
    [Range(0.0f, 1.0f)] public float sensitivity = 0.5f;
    public Vector2 lookInput;
    public Camera cam;
    float horizRotate;
    float vertRotate;
    //----------------------------\\

    // Variables for interaction \\
    private RaycastHit hit;
    public float interactDistance = 8f;
    public Transform cam4ray;
    public float MaxUseDistance = 8f;
    public Image pointer; // centre screen yellow point indicator for finding interactable items
    //----------------------------\\
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
    }

    void OnMove(InputValue value)
    {
        // read WASD or arrow key input
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        // read mouse input
        lookInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        // Character Movement \\
        // read x and z coordinates from input and set y to 0 as the character won't be jumping
        // normalize for consistent movement, regardless of direction
        Vector3 movement = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
        // transform the character location based on player input
        rb.velocity = (transform.right * movement.x + transform.forward * movement.z) * speed * Time.fixedDeltaTime + transform.up * rb.velocity.y ;
        // Character and Camera Rotation \\
        // accumulate vertical and horizontal rotation based on mouse input
        vertRotate += lookInput.y * sensitivity;
        horizRotate += lookInput.x * sensitivity;
        // clamp value so camera doesn't look up too high or too far down.
        vertRotate = Mathf.Clamp(vertRotate, -65, 65);
        // rotate the character body so that it's facing the same direction as the camera
        rb.MoveRotation(UnityEngine.Quaternion.Euler(0, horizRotate, 0));

        // Interaction \\
        // Draw a ray line from the centre of the camera in the scene window
        Debug.DrawLine(cam4ray.position, cam4ray.position + cam4ray.forward * MaxUseDistance, Color.white);

        // If the raycast hits a an object that has the "interactable" tag then make the yellow pointer active to indicate the item can be interacted with
        if (Physics.Raycast(cam4ray.position, cam4ray.forward, out hit, MaxUseDistance))
        {
            if (hit.collider.CompareTag("Interactable")){
                pointer.enabled = true;
            }
            else
            {
                pointer.enabled = false;
            }
        }
        else
        {
            pointer.enabled = false;
        }
    }

    private void LateUpdate()
    {

        // transform the camera rotation based on input
        // Unity automatically inverts vertical values, hence why the vertical rotation value here is negative
        cam.transform.eulerAngles = new Vector3(-vertRotate, horizRotate, 0);        
    }

    public void OnInteract()
    {
        // Use Raycast to detect how far away the player's front is from an object
        if (Physics.Raycast(cam4ray.position, cam4ray.forward, out hit, MaxUseDistance))
        {
            // Debug.Log("Raycast hit: " + hit.collider.name);
            Debug.DrawRay(cam4ray.position, cam4ray.forward, Color.yellow);

            if (hit.collider.CompareTag("Interactable")){
                InteractWithObject(hit.collider.gameObject);
            }
        }
    }


    // Interaction Function
    void InteractWithObject(GameObject obj)
    {
        // The object that has been detected by the raycast is moved smoothly to half the position between
        // its original location and the location of the camera
        obj.transform.position = Vector3.Lerp(obj.transform.position, cam4ray.position, 0.5f);
        // Debug.Log("Picked up: " + obj.name);
    }
}