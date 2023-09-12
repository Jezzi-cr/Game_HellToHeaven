using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    public GameObject gameManagerPrefab;
    public GameObject audioManagerPrefab;
    
    void Awake()
    {
        // Create the manager singletons if needed
        if (!GameManager.Instance)
        {
            Instantiate(gameManagerPrefab);
        }

        if (!AudioManager.Instance)
        {
            Instantiate(audioManagerPrefab);
        }
    }
}
