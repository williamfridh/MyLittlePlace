using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AmbienceMusicManager : MonoBehaviour
{

    public static AmbienceMusicManager Instance { get; private set; }

    [System.Serializable]
    public class BiomeAudioTrack
    {
        public BiomeType biomeType;
        public AudioSource audioSource;
        [Range(0f, 1f)] public float maxVolume = 1f;
        public List<AudioClip> playlist;

        [HideInInspector] public int currentTrackIndex = 0;
        [HideInInspector] public Coroutine trackTimerCoroutine;
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

    public void TransitionToBiome(BiomeType newBiome)
    {
        BiomeAudioTrack targetTrack = biomeTracks.Find(t => t.biomeType.Equals(newBiome));
        if (targetTrack == null || targetTrack == currentActiveTrack) return;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        currentActiveTrack = targetTrack;
        transitionCoroutine = StartCoroutine(CrossFadeAudioCoroutine(targetTrack));
    }

    void StartNextTrack(BiomeAudioTrack track)
    {
        if (track.playlist == null || track.playlist.Count == 0) return;

        // Stop any active clip timing watcher for the biome
        if (track.trackTimerCoroutine != null) StopCoroutine(track.trackTimerCoroutine);

        // Assign next clip from playlist
        AudioClip clipToPlay = track.playlist[track.currentTrackIndex];
        track.audioSource.clip = clipToPlay;

        // Ensure audio source is playing
        track.audioSource.Play();

        // Start a coroutine that wait for this specific tsong duration to finish
        track.trackTimerCoroutine = StartCoroutine(WaitForTrackToEnd(track, clipToPlay.length));
    }

    IEnumerator WaitForTrackToEnd(BiomeAudioTrack track, float duration)
    {
        yield return new WaitForSeconds(duration);
        track.currentTrackIndex = (track.currentTrackIndex + 1) % track.playlist.Count;
        StartNextTrack(track);
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
        // Ensure target biome music is actually playing
        if (targetTrack.audioSource.clip == null && targetTrack.playlist.Count > 0)
        {
            StartNextTrack(targetTrack);
        }
        else if (!targetTrack.audioSource.isPlaying && targetTrack.audioSource.clip != null)
        {
            // Resume
            targetTrack.audioSource.UnPause();
        }

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
                else
                {
                    // Pause the audio source so sace resources
                    if (targetVolume == 0f && track.audioSource.isPlaying)
                    {
                        track.audioSource.Pause();
                    }
                }
            }
            yield return null; // Wait for next frame process
        }
    }
    
}
