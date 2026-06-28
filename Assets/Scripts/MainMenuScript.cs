using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{

    [SerializeField] private GameObject startButton;

    public void HandlePlayClick()
    {
        Debug.Log("MainMenuScript: Detected 'play' click");
        MenuButtonAudio.Instance.PlayClickSound();
        SceneManager.LoadScene("World Load");
    }

    public void HandleNewGameClick()
    {
        Debug.Log("MainMenuScript: Detected 'new game' click");
        MenuButtonAudio.Instance.PlayClickSound();
        SceneManager.LoadScene("World Generate");
    }

    public void HandleExitGameClick()
    {
        Debug.Log("MainMenuScript: Exit game requested!");
        AmbienceAudioManager.Instance.TransitionToNone();
        AmbienceMusicManager.Instance.TransitionToNone();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void Start()
    {
        // Check if a save exists, it not, we hide the start game button
        if (SaveManagerScript.Instance == null)
        {
            Debug.LogError("MainMenuScript: Could not detect SaveManagerScript instance!");
        }
        else
        {
            Debug.Log("MainMenuScript: SaveManagerScript detected!");
            startButton.SetActive(SaveManagerScript.Instance.SaveExists());
        }
        // Play music
        AmbienceMusicManager.Instance.TransitionToBiome(BiomeType.Menu);
    }
}
