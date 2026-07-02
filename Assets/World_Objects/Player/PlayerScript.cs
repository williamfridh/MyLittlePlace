using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerScript : Unit
{

    public static PlayerScript Instance { get; set; }

    [SerializeField] private InputActionReference moveActionReference;

    public Camera playerCamera;
    private BiomeType currentBiomeType;

    [Header("UI")]
    [SerializeField] private GameObject interactionInstruction;

    protected override void InnerAwake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    protected override void InnerStart()
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

    public override int Damage(int damage, float pushback, Vector3 hazardPosition)
    {
        if (SaveManagerScript.Instance.playerSave == null) return 0;
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

        return newLife;
    }

    public override int Heal(int amount)
    {
        int maxLife = SaveManagerScript.Instance.playerSave.maxLife;
        int oldLife = SaveManagerScript.Instance.playerSave.life;
        int newLife = oldLife + amount;
        if (newLife > maxLife) newLife = maxLife;
        SaveManagerScript.Instance.playerSave.life = newLife;
        SaveManagerScript.Instance.SavePlayer();
        return newLife;
    }

    // Update is called once per frame
    protected override void InnerUpdate()
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

        HandleMovement(moveInput);

        // Update current biome type
        BiomeType targetBiomeType = SaveManagerScript.Instance.worldSave.GetBiomeAtPosition(transform.position);
        if (currentBiomeType != targetBiomeType)
        {
            currentBiomeType = targetBiomeType;
            AmbienceAudioManager.Instance.TransitionToBiome(targetBiomeType);
            AmbienceMusicManager.Instance.TransitionToBiome(targetBiomeType);
        }
    }
}
