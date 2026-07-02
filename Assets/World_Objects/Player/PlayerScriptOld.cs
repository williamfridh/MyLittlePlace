using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerScriptOld : MonoBehaviour
{

    private int defaultLayer;
    private int jumpingLayer;

    public static PlayerScriptOld Instance { get; set; }

    [Header("Movement Settings")]
    [SerializeField] public GameObject character;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float interactTime = 1f;
    private float interactTimer = 0f;
    [SerializeField] public float jumpTime = 1f;
    private float jumpTimer = 0f;
    [SerializeField] public float visualJumpHeight = 1f;
    [SerializeField] private float yScanOffset = -.35f;
    [SerializeField] private InputActionReference moveActionReference;

    public Camera playerCamera;
    [SerializeField] private Animator _animator;


    public float currentHorizontal = 0; // -1 for left, 1 for right, 0 for no horizontal movement
    public float currentVertical = -1;   // -1 for down, 1 for up, 0
    public bool isMoving;
    private bool jumpRequested = false;

    private BiomeType currentBiomeType;
    private Vector3 pushbackDirection;
    [SerializeField] bool enableMovement = true; 
    [SerializeField] float pushbackTime = 0.5f; 
    float pushbackTimer = 0f; 

    [Header("Juice Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;

    [Header("UI")]
    [SerializeField] private GameObject interactionInstruction;

    [Header("Audio")]
    [SerializeField] private CharacterAudioScript characterAudioManager;

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

    void Start()
    {
        PlayerSaveState playerSave = SaveManagerScript.Instance.playerSave;
        if (!playerSave.position_initilized)
        {
            MoveToSpawn();
            playerSave.position_initilized = true;
            SaveManagerScript.Instance.SavePlayer();
        }
        else
        {
            MoveToSavedPosition();
        }
    }

    private void MoveToSpawn()
    {
        WorldSaveState worldSave = SaveManagerScript.Instance.worldSave;
        transform.position = new Vector3(worldSave.spawnX, worldSave.spawnY, 0f);
    }

    private void MoveToSavedPosition()
    {
        PlayerSaveState playerSave = SaveManagerScript.Instance.playerSave;
        transform.position = new Vector3(playerSave.x_pos, playerSave.y_pos, 0f);
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
        gameObject.layer = jumpingLayer;
        jumpTimer = jumpTime;
        _animator.SetBool("isJumping", true);
        characterAudioManager.PlayJumpSound();
    }

    public void RequestInteract()
    {
        if (_animator.GetBool("isInteracting")) return;
        Debug.Log("PlayerScript: Interaction requested");
        _animator.SetBool("isInteracting", true);
        interactTimer = interactTime;
        if (InteractionScript.Instance) InteractionScript.Instance.TriggerInteraction();
    }

    public void TakeDamage(int damage, float pushback, Vector3 hazardPosition)
    {
        if (SaveManagerScript.Instance.playerSave == null) return;
        Debug.Log($"PlayerScript: TakeDamage triggered (damage = {damage} and pushback = {pushback})");

        // Update life
        int oldLife = SaveManagerScript.Instance.playerSave.life;
        int newLife = Mathf.Max(0, oldLife - damage);
        SaveManagerScript.Instance.playerSave.life = newLife;
        SaveManagerScript.Instance.SavePlayer();

        // Push back
        if (pushback >= 0)
        {
            // Calculate direction away from what hit you
            pushbackDirection = (transform.position - hazardPosition).normalized * pushback;
            enableMovement = false; // Disable player input during pushback
            pushbackTimer = pushbackTime;
            TriggerFlash(pushbackTime);
        }

        // If death
        if (newLife == 0)
        {
            // Reset life and move player to spawn
            SaveManagerScript.Instance.playerSave.life = 3;
            MoveToSpawn();
        }
    }

    public int Heal(int amount)
    {
        int maxLife = SaveManagerScript.Instance.playerSave.maxLife;
        int oldLife = SaveManagerScript.Instance.playerSave.life;
        int newLife = oldLife + amount;
        if (newLife > maxLife) newLife = maxLife;
        SaveManagerScript.Instance.playerSave.life = newLife;
        SaveManagerScript.Instance.SavePlayer();
        return newLife;
    }

    /// <summary>
    /// Create flash routine to represent damage taken.
    /// </summary>
    /// <param name="duration"></param>
    public void TriggerFlash(float duration)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashSequence(duration));
    }

    /// <summary>
    /// Flash sequence to represent damage taken.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator FlashSequence(float duration)
    {
        // Access the unique material instance
        Material mat = spriteRenderer.material;
        // And turn it into white instantly
        mat.SetFloat("_FlashAmount", 1f);
        Debug.Log(mat.GetFloat("_FlashAmount"));

        yield return new WaitForSeconds(duration);

        // Return to normal sprite color
        mat.SetFloat("_FlashAmount", 0f);
        Debug.Log(mat.GetFloat("_FlashAmount"));
        flashCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Pin camera to player location (x, y)
        Vector3 playerPos = transform.position;
        playerCamera.transform.position = new Vector3(playerPos.x, playerPos.y, playerCamera.transform.position.z);

        // Update save state (don't trigger save)
        SaveManagerScript.Instance.playerSave.x_pos = playerPos.x;
        SaveManagerScript.Instance.playerSave.y_pos = playerPos.y;

        // Update UI
        if (InteractionScript.Instance && interactionInstruction != null)
        {
            interactionInstruction.SetActive(InteractionScript.Instance.CanInteract());
        }

        // Stop if game is paused
        if (GameStateScript.Instance.paused) return;

        // Stop if movement is disabled
        if (!enableMovement) return;

        // Read jump request
        if ( Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) RequestJump();

        // Read interact request
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) RequestInteract();

        // Read movement inputs
        Vector2 moveInput = Vector2.zero;

        if (moveActionReference != null)
        {
            moveInput = moveActionReference.action.ReadValue<Vector2>();
        }

        isMoving = moveInput.magnitude > 0;
        characterAudioManager.SetWalking(isMoving);

        if (moveInput.magnitude > 0)
        {
            currentHorizontal = moveInput.x;
            currentVertical = moveInput.y;
        }

        // Apply the translation
        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        pushbackDirection = moveDirection; // Store for pushback
        Vector3 targetTranslation = moveDirection * moveSpeed * Time.deltaTime;

        // Prevent moving into obstacle before applying translation

        Vector3 checkPosition = transform.position + targetTranslation + new Vector3(0f, yScanOffset, 0f);
        Collider2D waterHit = Physics2D.OverlapCircle(checkPosition, 0.2f, LayerMask.GetMask("Water"));
        Collider2D impassableHit = Physics2D.OverlapCircle(checkPosition, 0.2f, LayerMask.GetMask("Impassable"));
        if (!waterHit && !impassableHit)
        {
            transform.Translate(targetTranslation);
        }

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
        BiomeType targetBiomeType = SaveManagerScript.Instance.worldSave.GetBiomeAtPosition(transform.position);
        if (currentBiomeType != targetBiomeType)
        {
            currentBiomeType = targetBiomeType;
            AmbienceAudioManager.Instance.TransitionToBiome(targetBiomeType);
            AmbienceMusicManager.Instance.TransitionToBiome(targetBiomeType);
        }
    }

    void FixedUpdate()
    {
        // Handle Pushback
        if (pushbackTimer > 0)
        {
            // Move the actual root parent object through world space
            transform.position += pushbackDirection * Time.fixedDeltaTime;

            pushbackTimer -= Time.fixedDeltaTime;
            if (pushbackTimer <= 0)
            {
                pushbackTimer = 0f;
                enableMovement = true; // Return control to the player
            }
        }
        // Handle interacting
        if (interactTimer > 0)
        {
            interactTimer -= Time.fixedDeltaTime;
            if (interactTimer <= 0)
            {
                interactTimer = 0f;
                _animator.SetBool("isInteracting", false);
            }
        }
        // Handle jumping
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
