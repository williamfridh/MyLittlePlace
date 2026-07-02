using UnityEngine;

public class CampfireScript : MonoBehaviour, IInteractable
{

    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private float maxFuel = 10f;
    [SerializeField] private float fuel = 10f;
    [SerializeField] private float fuelInteractLimit = 9f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Campfire Versions")]
    [SerializeField] private GameObject campfireAlive;
    [SerializeField] private GameObject campfireDead;

    [Header("Light")]
    [SerializeField] private GameObject lightSource;
    
    [Header("Fuel Economy")]
    [SerializeField] private float burnRatePerGameHour = 1.0f;
    
    [Header("Effect")]
    [SerializeField] private GameObject healingArea;

    public void IgniteFire()
    {
        fireParticles.Play();
        audioSource.Play();
        campfireAlive.SetActive(true);
        campfireDead.SetActive(false);
        lightSource.SetActive(true);
        healingArea.SetActive(true);
    }

    public void ExtinguishFire()
    {
        fireParticles.Stop();
        audioSource.Stop();
        campfireAlive.SetActive(false);
        campfireDead.SetActive(true);
        lightSource.SetActive(false);
        healingArea.SetActive(false);
    }

    public void AddFuel(float amount)
    {
        fuel += amount;
        if (fuel >0 && fireParticles != null && !fireParticles.isPlaying)
        {
            IgniteFire();
        }
    }
    public bool Interact()
    {
        Debug.Log("Campfire: Interaction detected");
        if (fuel >= maxFuel) return false;

        if (SaveManagerScript.Instance == null) return false;
        if (SaveManagerScript.Instance.playerSave.GetItemAmount("wood") == 0)
        {
            Debug.Log("CampfireScript: No wood pile to add.");
            return false;
        }

        SaveManagerScript.Instance.playerSave.RemoveFromInventory("wood", 1);
        if (AudioManagerScript.Instance && fuel <= 0) AudioManagerScript.Instance.PlayFireStartSound();
        fuel++;
        if (fuel > maxFuel) fuel = maxFuel;
        if (AudioManagerScript.Instance) AudioManagerScript.Instance.PlayGatherWoodSound();
        return true;
    }

    public bool CanInteract()
    {
        if (fuel > fuelInteractLimit) return false;
        if (SaveManagerScript.Instance.playerSave.GetItemAmount("wood") > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Start()
    {
        if (fuel > 0)
        {
            campfireAlive.SetActive(true);
            campfireDead.SetActive(false);
        }
        else
        {
            campfireAlive.SetActive(false);
            campfireDead.SetActive(true);
        }
    }

    void Update()
    {
        if (fireParticles == null) return;
        if (campfireAlive == null || campfireDead == null) return;
        if (audioSource == null) return;
        if (lightSource == null) return;

        if (fuel > 0)
        {
            float gameSecondsPassed = Time.deltaTime * TimeManagerScript.Instance.inGameSecondsPerRealSecond;
            float gameHoursPassed = gameSecondsPassed / 3600f;
            fuel -= burnRatePerGameHour * gameHoursPassed;
            if (!fireParticles.isPlaying) IgniteFire();
        }
        else
        {
            fuel = 0;
            if (fireParticles.isPlaying)
            {
                ExtinguishFire();
            }
        }
    }


}
