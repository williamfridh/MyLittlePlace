using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    public static PlayerScript Instance { get; set; }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

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

    // Update is called once per frame
    void Update()
    {
        // Pin camera to player location (x, y)
        Vector3 playerPos = transform.position;
        playerCamera.transform.position = new Vector3(playerPos.x, playerPos.y, playerCamera.transform.position.z);
        // Read movement inputs
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput.x = -1f;
                isMoving = true;
                currentHorizontal = -1f;
                currentVertical = 0f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput.x = 1f;
                isMoving = true;
                currentHorizontal = 1f;
                currentVertical = 0f;
            }
            else if (Keyboard.current.upArrowKey.isPressed)
            {
                moveInput.y = 1f;
                isMoving = true;
                currentVertical = 1f;
                currentHorizontal = 0f;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                moveInput.y = -1f;
                isMoving = true;
                currentVertical = -1f;
                currentHorizontal = 0f;
            }
            else
            {
                isMoving = false;
            }
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
