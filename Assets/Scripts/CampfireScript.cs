using UnityEngine;

public class CampfireScript : MonoBehaviour
{

    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private float fuel = 10f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Campfire Versions")]
    [SerializeField] private GameObject campfireAlive;
    [SerializeField] private GameObject campfireDead;
    
    [Header("Fuel Economy")]
    [SerializeField] private float burnRatePerGameHour = 1.0f;

    public void IgniteFire()
    {
        fireParticles.Play();
        audioSource.Play();
        campfireAlive.SetActive(true);
        campfireDead.SetActive(false);
    }

    public void ExtinguishFire()
    {
        fireParticles.Stop();
        audioSource.Stop();
        campfireAlive.SetActive(false);
        campfireDead.SetActive(true);
    }

    public void AddFuel(float amount)
    {
        fuel += amount;
        if (fuel >0 && fireParticles != null && !fireParticles.isPlaying)
        {
            IgniteFire();
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
