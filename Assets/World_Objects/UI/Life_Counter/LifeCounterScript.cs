using UnityEngine;
using UnityEngine.UI;

public class LifeCounterScript : MonoBehaviour
{
    [Header("Prefabs & Objects")]
    [SerializeField] private GameObject lifeIconPrefab;

    [Header("Interfal")]
    [SerializeField] private float timer = 0.0f;
    [SerializeField] private float interval = 0.5f;

    [Header("Sound")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSource;
    

    /// <summary>
    /// DrawLife draws or removes single life counters form the HUD panel.
    /// It's supposed to be called using an interval for smooth experience.
    /// </summary>
    void DrawLife()
    {
        PlayerSaveState player = SaveManagerScript.Instance.playerSave;
        if (player == null) return;
        int life = player.life;

        if (lifeIconPrefab == null) return;
        int oldAmount = transform.childCount;
        if (life == oldAmount) // No action if same amount
        {
            return;
        }
        else if (life < oldAmount) // Lost life
        {
            int lastChildIndex = transform.childCount - 1;
            Destroy(transform.GetChild(lastChildIndex).gameObject);
        }
        else if (life > oldAmount) // Gained life
        {
            GameObject newLife = Instantiate(lifeIconPrefab, transform.position, Quaternion.identity);
            newLife.transform.SetParent(transform, false);
            PlayClickSound();
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            DrawLife();
            timer -= interval;
        }
    }
}
