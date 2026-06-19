using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    private int defaultLayer;
    private int jumpingLayer;

    public static PlayerScript Instance { get; set; }

    [Header("Movement Settings")]
    [SerializeField] public GameObject character;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpTime = 1f;
    private float jumpTimer = 0f;
    [SerializeField] public float visualJumpHeight = 1f;
    [SerializeField] private InputActionReference moveActionReference;

    public Camera playerCamera;
    [SerializeField] private Animator _animator;
    
    [Header("Life")]
    [SerializeField] public int life = 10;

    [Header("Audio")]
    [SerializeField] AudioSource jumpAudio;


    public float currentHorizontal = 0; // -1 for left, 1 for right, 0 for no horizontal movement
    public float currentVertical = -1;   // -1 for down, 1 for up, 0
    public bool isMoving;
    private bool jumpRequested = false;

    private WorldGeneratorScript.BiomeType currentBiomeType;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        // Setup layers
        defaultLayer = LayerMask.NameToLayer("Player");
        jumpingLayer = LayerMask.NameToLayer("JumpingPlayer");
        // Set animation paramaters
        _animator.SetFloat("Horizontal", 0f);
        _animator.SetFloat("Vertical", -1f);
        _animator.SetBool("isMoving", false);
        _animator.SetBool("isJumping", false);
    }

    private void EnableJoystick()
    {
        if (moveActionReference != null) moveActionReference.action.Enable();
    }

    private void DisableJoystick()
    {
        if (moveActionReference != null) moveActionReference.action.Disable();
    }

    public void RequestJump()
    {
        if (_animator.GetBool("isJumping")) return;
        //jumpRequested = true;
        gameObject.layer = jumpingLayer;
        jumpTimer = jumpTime;
        _animator.SetBool("isJumping", true);
        if (jumpAudio != null) jumpAudio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // Pin camera to player location (x, y)
        Vector3 playerPos = transform.position;
        playerCamera.transform.position = new Vector3(playerPos.x, playerPos.y, playerCamera.transform.position.z);

        // Stop if game is paused
        if (WorldStateScript.Instance.paused) return;

        // Read jump request
        if ( Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) RequestJump();

        // Read movement inputs
        Vector2 moveInput = Vector2.zero;

        if (moveActionReference != null)
        {
            moveInput = moveActionReference.action.ReadValue<Vector2>();
        }

        isMoving = moveInput.magnitude > 0;
        if (moveInput.magnitude > 0)
        {
            currentHorizontal = moveInput.x;
            currentVertical = moveInput.y;
        }

        // Apply the translation
        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        Vector3 targetTranslation = moveDirection * moveSpeed * Time.deltaTime;

        // Prevent moving into obstacle before applying translation
        //Collider2D hit = Physics2D.OverlapCircle(transform.position + targetTranslation, 0.2f, LayerMask.GetMask("Obstacle"));
        //if (!hit || gameObject.layer == jumpingLayer)
        //{
        transform.Translate(targetTranslation);
        //}

        // Update directional parameters for animation
        _animator.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                _animator.SetFloat("Horizontal", moveInput.x > 0 ? 1f : -1f);
                _animator.SetFloat("Vertical", 0f);
            }
            else
            {
            {
                _animator.SetFloat("Horizontal", 0f);
                _animator.SetFloat("Vertical", moveInput.y > 0 ? 1f : -1f);
            }}
        }

        // Update current biome type
        WorldGeneratorScript.BiomeType targetBiomeType = WorldGeneratorScript.Instance.GetBiomeAtPosition(transform.position);
        if (currentBiomeType != targetBiomeType)
        {
            currentBiomeType = targetBiomeType;
            AmbienceAudioManager.Instance.TransitionToBiome(targetBiomeType);
        }
    }

    void FixedUpdate()
    {
        if (jumpTimer > 0)
        {
            // Normalize time slace
            float normalizedTime = (jumpTime - jumpTimer) / jumpTime;
            normalizedTime = Mathf.Clamp01(normalizedTime);
            // Convert to parapolic shape
            float heightRatio = 4f * normalizedTime * (1f - normalizedTime);
            // Get curent visual height
            float currentVisualHeight = heightRatio * visualJumpHeight;
            character.transform.localPosition = new Vector3(0f, currentVisualHeight, 0f);
            // Subtract time
            jumpTimer -= Time.fixedDeltaTime;
            if (jumpTimer <= 0)
            {
                jumpTimer = 0f;
                character.transform.localPosition = Vector3.zero;
                gameObject.layer = defaultLayer;
                _animator.SetBool("isJumping", false);
            }
        }
        else
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Obstacles"));
            if (hit) return;
            // (Make sure your jump input event triggers "gameObject.layer = jumpingLayer" and sets "jumpTimer = jumpTime" to fire the arc above!
            //_animator.SetBool("isJumping", false);
            //gameObject.layer = defaultLayer;
        }
    }
}
