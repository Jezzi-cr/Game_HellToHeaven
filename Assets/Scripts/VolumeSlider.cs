using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private Slider _slider;
    
    void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.value = AudioManager.Instance.GetVolume();
        _slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float val)
    {
        AudioManager.Instance.SetVolume(val);
    }
}
