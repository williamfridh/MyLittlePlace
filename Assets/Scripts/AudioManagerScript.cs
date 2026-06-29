using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{

    public static AudioManagerScript Instance { get; set; }
    
    [Header("Audio Sources")]
    [SerializeField] AudioSource jumpAudio;
    [SerializeField] AudioSource gatherAudio;
    [SerializeField] AudioSource gatherWoodAudio;
    [SerializeField] AudioSource punchOne;
    [SerializeField] AudioSource punchTwo;

    void Awake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayJumpSound()
    {
        if (jumpAudio != null) jumpAudio.Play();
    }

    public void PlayGatherSound()
    {
        if (gatherAudio != null) gatherAudio.Play();
    }

    public void PlayGatherWoodSound()
    {
        if (gatherWoodAudio != null) gatherWoodAudio.Play();
    }

    public void PlayPunchOneSound()
    {
        if (punchOne != null) punchOne.Play();
    }

    public void PlayPunchTwoSound()
    {
        if (punchTwo != null) punchTwo.Play();
    }
}
