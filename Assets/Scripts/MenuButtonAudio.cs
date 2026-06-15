using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Adds if missing
public class MenuButtonAudio : MonoBehaviour
{

    public static MenuButtonAudio Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// This function cal be called from any UI button.
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
