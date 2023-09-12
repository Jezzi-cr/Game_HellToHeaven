using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class NewHighscore : MonoBehaviour
{
    [Header("Component")] public TextMeshProUGUI highScoreText;
    [Header("Component")] public TextMeshProUGUI currentScoreText;
    [Header("Component")] public TextMeshProUGUI newHighScoreText;
    
    void Start()
    {
        var score = GameManager.Instance.GetScore();
        var highScore = GameManager.Instance.GetLastHighScore();
        
        currentScoreText.text = score.ToShortString();
        highScoreText.text = highScore.ToShortString();

        if (GameManager.Instance.IsNewHighScore())
        {
            newHighScoreText.text = "NEW HIGHSCORE!";
            highScoreText.text = score.ToShortString();
        }
    }

   
}
