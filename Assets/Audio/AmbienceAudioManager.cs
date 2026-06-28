using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AmbienceAudioManager : MonoBehaviour
{

    public static AmbienceAudioManager Instance { get; private set; }

    [System.Serializable]
    public class BiomeAudioTrack
    {
        public BiomeType biomeType;
        public AudioSource audioSource;
        [Range(0f, 1f)] public float maxVolume = 1f;
    }

    [Header("Biome Track Configuration")]
    [SerializeField] private List<BiomeAudioTrack> biomeTracks;
    [SerializeField] float fadeSpeed = 0.5f;

    private Coroutine transitionCoroutine;
    private BiomeAudioTrack currentActiveTrack;

    void Awake()
    {
        // Establish Singleton lifecycle pattern
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
    }

    void Start()
    {
        // Initilize each track to 0 volume and ensure they are playing a loop
        foreach (BiomeAudioTrack track in biomeTracks)
        {
            if (track.audioSource == null) continue;
            track.audioSource.volume = 0f;
            track.audioSource.loop = true;
            track.audioSource.Play();
        }
    }

    public void TransitionToBiome(BiomeType biomeType)
    {
        BiomeAudioTrack targetTrack = biomeTracks.Find(t => t.biomeType.Equals(biomeType));
        if (targetTrack == null || targetTrack == currentActiveTrack) return;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(CrossFadeAudioCoroutine(targetTrack));
    }

    public void TransitionToNone()
    {
        transitionCoroutine = StartCoroutine(FadeAudioCoroutine());
    }

    public IEnumerator FadeAudioCoroutine()
    {
        bool elementsStillBlending = true;
        while (elementsStillBlending)
        {
            elementsStillBlending = false;
            foreach (BiomeAudioTrack track in biomeTracks)
            {
                if (track.audioSource == null) continue;

                // Get target baseline
                float targetVolume = 0f;

                // Move towards objective independednt of frame drops
                track.audioSource.volume = Mathf.MoveTowards(track.audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

                // If track hasn't fully reached goal, keep looping
                if (!Mathf.Approximately(track.audioSource.volume, targetVolume))
                {
                    elementsStillBlending = true;
                }
            }
            yield return null; // Wait for next frame process
        }
    }

    private IEnumerator CrossFadeAudioCoroutine(BiomeAudioTrack targetTrack)
    {
        bool elementsStillBlending = true;
        while (elementsStillBlending)
        {
            elementsStillBlending = false;
            foreach (BiomeAudioTrack track in biomeTracks)
            {
                if (track.audioSource == null) continue;

                // Get target baseline
                float targetVolume = (track == targetTrack) ? track.maxVolume : 0f;

                // Move towards objective independednt of frame drops
                track.audioSource.volume = Mathf.MoveTowards(track.audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

                // If track hasn't fully reached goal, keep looping
                if (!Mathf.Approximately(track.audioSource.volume, targetVolume))
                {
                    elementsStillBlending = true;
                }
            }
            yield return null; // Wait for next frame process
        }
    }
    
}
