using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    public static PlayerScript Instance { get; set; }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    public Camera playerCamera;
    [SerializeField] private Animator _animator;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
                _animator.SetBool("isWalkingWest", true);
                _animator.SetBool("isWalkingEast", false);
                _animator.SetBool("isWalkingNorth", false);
                _animator.SetBool("isWalkingSouth", false);
            }
            else if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput.x = 1f;
                _animator.SetBool("isWalkingEast", true);
                _animator.SetBool("isWalkingWest", false);
                _animator.SetBool("isWalkingNorth", false);
                _animator.SetBool("isWalkingSouth", false);
            }
            else if (Keyboard.current.upArrowKey.isPressed)
            {
                moveInput.y = 1f;
                _animator.SetBool("isWalkingNorth", true);
                _animator.SetBool("isWalkingWest", false);
                _animator.SetBool("isWalkingEast", false);
                _animator.SetBool("isWalkingSouth", false);
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                moveInput.y = -1f;
                _animator.SetBool("isWalkingSouth", true);
                _animator.SetBool("isWalkingWest", false);
                _animator.SetBool("isWalkingEast", false);
                _animator.SetBool("isWalkingNorth", false);
            }
        }

        // Apply the translation
        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
