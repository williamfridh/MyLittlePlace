using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    public static PlayerScript Instance { get; set; }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveActionReference;

    public Camera playerCamera;
    [SerializeField] private Animator _animator;
    
    [Header("Life")]
    [SerializeField] private int life = 8;
    [SerializeField] private GameObject lifeCounter;
    [SerializeField] private GameObject lifeIcon;


    public float currentHorizontal; // -1 for left, 1 for right, 0 for no horizontal movement
    public float currentVertical;   // -1 for down, 1 for up, 0
    public bool isMoving;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void DrawLife()
    {
        
    }

    private void EnableJoystick()
    {
        if (moveActionReference != null) moveActionReference.action.Enable();
    }

    private void DisableJoystick()
    {
        if (moveActionReference == null) moveActionReference.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Pin camera to player location (x, y)
        Vector3 playerPos = transform.position;
        playerCamera.transform.position = new Vector3(playerPos.x, playerPos.y, playerCamera.transform.position.z);
        // Read movement inputs
        Vector2 moveInput = Vector2.zero;

        if (moveActionReference != null)
        {
            moveInput = moveActionReference.action.ReadValue<Vector2>();
        }

        if (moveInput.magnitude > 0)
        {
            isMoving = true;
            if (moveInput.x < 0)
            {
                currentHorizontal = -1f;
            }
            else if (moveInput.x > 0)
            {
                currentHorizontal = 1f;
            }
            else
            {
                currentHorizontal = 0f;
            }
            if (moveInput.y < 0)
            {
                currentVertical = -1f;
            }
            else if (moveInput.y > 0)
            {
                currentVertical = 1f;
            }
            else
            {
                currentVertical = 0f;
            }
        }
        else
        {
            isMoving = false;
        }

        // Apply the translation
        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        // Update directional parameters for animation
        _animator.SetBool("isMoving", isMoving);
        _animator.SetFloat("Horizontal", currentHorizontal);
        _animator.SetFloat("Vertical", currentVertical);
    }
}
