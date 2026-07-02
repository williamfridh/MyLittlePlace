using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour
{

    [Header("General")]
    [SerializeField] public GameObject character;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] protected float yScanOffset = -.35f;

    [Header("Jump")]
    [SerializeField] public float jumpTime = 1f;
    protected float jumpTimer = 0f;

    [Header("Interaction")]

    [SerializeField] public float interactTime = 1f;
    protected float interactTimer = 0f;

    [Header("Animation")]
    [SerializeField] protected Animator _animator;
    [SerializeField] public float visualJumpHeight = 1f;
    
    protected int defaultLayer;
    protected int jumpingLayer;
    public float currentHorizontal = 0; // -1 for left, 1 for right, 0 for no horizontal movement
    public float currentVertical = -1;   // -1 for down, 1 for up, 0
    public bool isMoving;
    protected bool jumpRequested = false;


    [Header("Damage")]
    [SerializeField] protected bool enableMovement = true;
    [SerializeField] protected float pushbackTime = 0.5f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected Coroutine flashCoroutine;
    protected Vector3 pushbackDirection;
    protected float pushbackTimer = 0f; 

    [Header("Health")]
    [SerializeField] protected int life = 5;
    [SerializeField] protected int maxLife = 5;

    [Header("Audio")]
    [SerializeField] protected CharacterAudioScript characterAudioManager;

    public abstract int Damage(int damge, float pushback, Vector3 hazardPosition);
    public abstract int Heal(int amount);

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
    protected IEnumerator FlashSequence(float duration)
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
    
    protected abstract void InnerAwake();
    protected virtual void Awake()
    {
        // Setup layers
        defaultLayer = LayerMask.NameToLayer("Player");
        jumpingLayer = LayerMask.NameToLayer("JumpingPlayer");
        // Set animation paramaters
        if (_animator)
        {
            _animator.SetFloat("Horizontal", 0f);
            _animator.SetFloat("Vertical", -1f);
            _animator.SetBool("isMoving", false);
            _animator.SetBool("isJumping", false);
        }

        InnerAwake();
    }
    protected abstract void InnerStart();
    protected virtual void  Start()
    {
        InnerStart();
    }

    protected abstract void InnerUpdate();
    protected virtual void Update()
    {

        InnerUpdate();
    }

    protected void HandleMovement(Vector2 moveInput)
    {
        // Stop if movement is disabled
        if (!enableMovement) return;

        isMoving = moveInput.magnitude > 0.001f; // Threshold as AI navigation may produce small movements
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
        }
    }

}