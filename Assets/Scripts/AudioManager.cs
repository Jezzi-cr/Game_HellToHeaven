using UnityEngine;
using UnityEngine.Assertions;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource audioSource;
    public AudioSource ambientAudioSource;
    
    private float _volume;
    private float _fadeAmount;
    
    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load the volume
        _volume = PlayerPrefs.GetFloat("volume", 1f);
        // Update volume initially
        UpdateVolume();
    }

    public float GetVolume()
    {
        return _volume;
    }

    public void SetVolume(float vol)
    {
        _volume = vol;
        // Save the new value
        PlayerPrefs.SetFloat("volume", vol);
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        AudioListener.volume = _volume * Mathf.Clamp01(1f - _fadeAmount);
    }

    public void PlayAudio(AudioClip audioClip)
    {
        if (audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void SetAmbientAudio(AudioClip audioClip)
    {
        ambientAudioSource.clip = audioClip;
        ambientAudioSource.Play();
    }

    public void SetFadeAmount(float amount)
    {
        _fadeAmount = amount;
        UpdateVolume();
    }
}
