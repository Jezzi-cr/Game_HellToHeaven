using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.Instance.LoadGame();
    }

    public void ResumeGame()
    {
        GameManager.Instance.CloseInGameMenu();
    }
    
    public void GoMenu()
    {
        GameManager.Instance.LoadMenu();
    }

    public void GoCharacterSelection()
    {
        GameManager.Instance.LoadCharacterSelection();
    }
    
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();    
    }
}
