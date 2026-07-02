using UnityEngine;

public class CharacterAudioScript : MonoBehaviour
{
    
    [Header("Audio Sources")]
    [SerializeField] AudioSource jumpAudio;
    [SerializeField] AudioSource gatherAudio;
    [SerializeField] AudioSource gatherWoodAudio;
    [SerializeField] AudioSource punchOneAdudio;
    [SerializeField] AudioSource punchTwoAudio;
    [SerializeField] AudioSource walkForestAudio;

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
        if (punchOneAdudio != null) punchOneAdudio.Play();
    }

    public void PlayPunchTwoSound()
    {
        if (punchTwoAudio != null) punchTwoAudio.Play();
    }

    public void SetWalking(bool walking)
    {
        if (!walkForestAudio)
        {
            Debug.LogWarning("CharacterAudioScript: walkForestAudio not set. Stopping SetWalking()");
            return;
        }

        if (walking)
        {
            // Only start once
            if (!walkForestAudio.isPlaying)
            {
                walkForestAudio.Play();
            }
        }
        else
        {
            // Only stop once
            if (walkForestAudio.isPlaying)
            {
                walkForestAudio.Stop();
            }
        }
    }
}
