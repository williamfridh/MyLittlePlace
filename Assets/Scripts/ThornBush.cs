using UnityEngine;

public class ThornBush : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (AudioManagerScript.Instance) AudioManagerScript.Instance.PlayPunchTwoSound();
    }
}
