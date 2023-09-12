using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text timeText;
    public Text coinsText;

    private void Update()
    {
        var elapsedTime = Mathf.FloorToInt(GameManager.Instance.GetGameTime());
        timeText.text = $"{elapsedTime / 60:D2}:{elapsedTime % 60:D2}";
        coinsText.text = $"{GameManager.Instance.GetCoinsCollected()}";
    }
}
