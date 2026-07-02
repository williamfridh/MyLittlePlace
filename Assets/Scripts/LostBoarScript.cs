using UnityEngine;
using UnityEngine.AI;

public class LostBoarScript : Unit, IInteractable
{

    private Transform playerTransform;
    private UnityEngine.AI.NavMeshAgent agent;
    private float lockedZ;
    private Vector3 spawnPosition = new Vector3();
    private bool spawnPositionInitlized = false;

    private Vector3 targetPosition = new Vector3();
    private float roamingTimer = 0f;

    [Header("General")]
    [SerializeField] private float inactivationDistance = 20f;

    [Header("Behaviour")]
    [SerializeField] private float aggroDistance = 2f;
    [SerializeField] float roamingMinTime = 2f;
    [SerializeField] float roamingMaxTime = 10f;
    [SerializeField] int hunger = 2;
    [SerializeField] int maxHunger = 2;

    public bool Interact()
    {
        Debug.Log("LostBoarScript: Interaction detected");
        if (hunger <= 0) return false;
        characterAudioManager.PlayGatherSound();
        hunger--;
        if (SaveManagerScript.Instance.playerSave.GetItemAmount("blueberry") > 0)
        {
            SaveManagerScript.Instance.playerSave.RemoveFromInventory("blueberry", 1);
        }
        else
        {
            SaveManagerScript.Instance.playerSave.RemoveFromInventory("raspberry", 1);
        }
        SaveManagerScript.Instance.playerSave.RemoveFromInventory("blueberry", 1);
        return true;
    }

    public bool CanInteract()
    {
        if (hunger <= 0) return false;
        if (SaveManagerScript.Instance.playerSave.GetItemAmount("blueberry") > 0)
        {
            return true;
        }
        else
        {
            if (SaveManagerScript.Instance.playerSave.GetItemAmount("raspberry") > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public override int Damage(int damage, float pushback, Vector3 hazardPosition)
    {
        life -= damage;
        if (life <= 0)
        {
            Destroy(gameObject);
        }
        return life;
    }

    public override int Heal(int amount)
    {
        // LostBoarScript does not heal
        return 0;
    }

    protected override void InnerStart()
    {
        // Do something
    }
    protected override void InnerAwake()
    {
        if (PlayerScript.Instance == null)
        {
            Debug.LogWarning("LostBoarScript: No playerTransform found. Deleting object...");
            Destroy(gameObject);
            return;
        }

        playerTransform = PlayerScript.Instance.transform;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        if (!spawnPositionInitlized) spawnPosition = transform.position;

        agent.enabled = false;

        // Activate agent (note that is spawned as disabled)
        agent.Warp(spawnPosition);

        // Lock Z axis rotation
        agent.updateRotation    = false;
        agent.updateUpAxis      = false;

        // Disable automatic position update
        agent.updatePosition = false;

        // Lock Z
        lockedZ = 0f;

        agent.enabled = true;

        // Set starting target location
        targetPosition = transform.position;
    }

    void Order()
    {
        Vector3 diff = playerTransform.position - transform.position;
        float distance = diff.magnitude;
        if (distance > inactivationDistance)
        {
            isMoving = false;
            _animator.SetBool("isMoving", false);
            agent.SetDestination(transform.position);
            hunger = maxHunger;
            return;
        }
        if (hunger <= 0)
        {
            Debug.Log("LostBoarScript: Within range of aggro");
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            Debug.Log("LostBoarScript: Outside range of aggro");
            if (!isMoving && Random.Range(0f, 1f) > 0.3)
            {
                roamingTimer = Random.Range(roamingMinTime, roamingMaxTime);
                
                Vector2Int? cell = SaveManagerScript.Instance.worldSave.GetRandomPosition(BiomeType.Meadow);
                if (cell == null) return;
                Vector2Int cellCasted = (Vector2Int)cell;
                agent.SetDestination(new Vector3((float)cellCasted.x, (float)cellCasted.y, 0f));
            }
        }
    }

    // Update is called once per frame
    protected override void InnerUpdate()
    {
        if (!agent.isOnNavMesh) 
        {
            Debug.LogWarning("LostBoarScript: Unit is completely off the NavMesh!");
            Vector2Int? cell = SaveManagerScript.Instance.worldSave.GetRandomPosition(BiomeType.Meadow);
            if (cell == null) Destroy(gameObject);
            Vector2Int cellCasted = (Vector2Int)cell;
            agent.Warp(new Vector3((float)cellCasted.x, (float)cellCasted.y, 0f));
            if (!agent.isOnNavMesh) Destroy(gameObject);
        }

        Order();

        Vector2 moveInput = new Vector2(
            agent.desiredVelocity.x,
            agent.desiredVelocity.y
        );

        HandleMovement(moveInput);
    }

    private void LateUpdate()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        Vector3 navPosition = agent.nextPosition;

        transform.position = new Vector3(
            navPosition.x,
            navPosition.y,
            lockedZ
        );
    }
}
