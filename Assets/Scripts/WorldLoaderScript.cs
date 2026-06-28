using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldLoaderScript : MonoBehaviour
{
    void Start()
    {
        
        if (SaveManagerScript.Instance)
        {
            if (SaveManagerScript.Instance.SaveExists())
            {
                SaveManagerScript.Instance.Load(false, false, false);
            }
            else
            {
                SceneManager.LoadScene("World Generate");
            }
        }
        else
        {
            Debug.LogError("WorldLoaderScript: Could not access SaveManagerScript instance");
        }
    }
}
