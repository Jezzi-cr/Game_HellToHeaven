using System;
using UnityEngine;

public class Lava : MonoBehaviour, IKeyInterface
{
    public float riseSpeed = 2f;
    public float riseSpeedVariance = 0.25f;
    public float riseSpeedFrequency = 1f;
    public float acceleration = 0.25f;
    public float deceleration = 1f;
    public float minBrightness = 0.1f;
    
    // Height to which the lava rises
    public float riseHeight = 10f;
    public bool startLocked;

    private Renderer _renderer;
    private AudioSource _lavaAudio;

    private bool _isLocked;
    private float _speedRatio;
    private float _cooldown;
    private float _defaultBrightness;
    private float _defaultMaxHeight;
    private static readonly int BrightnessParam = Shader.PropertyToID("_Brightness");
    private static readonly int MaxHeightParam = Shader.PropertyToID("_MaxHeight");

    private void Start()
    {
        _isLocked = startLocked;
        _renderer = GetComponent<Renderer>();
        _lavaAudio = GetComponent<AudioSource>();
        // Get the default material parameter values
        _defaultBrightness = _renderer.material.GetFloat(BrightnessParam);
        _defaultMaxHeight = _renderer.material.GetFloat(MaxHeightParam);
        // Update the material initially
        UpdateMaterial();
    }

    void FixedUpdate()
    {
        var isCoolingDown = _cooldown > 0f;
        var targetSpeedRatio = _isLocked || isCoolingDown || transform.position.y > riseHeight ? 0f : 1f;

        // Update the cooldown
        if (isCoolingDown)
        {
            _cooldown -= Time.deltaTime;
        }
        
        // Update the speed
        var deltaSpeed = targetSpeedRatio - _speedRatio;
        if (Math.Abs(deltaSpeed) > 0.001f)
        {
            var accel = deltaSpeed > 0f ? acceleration : deceleration;
            _speedRatio = Mathf.Clamp01(_speedRatio + Mathf.Sign(deltaSpeed) * accel * Time.deltaTime);
            UpdateMaterial();
        }

        // Update the transform
        if (_speedRatio > 0f)
        {
            var speed = riseSpeed * _speedRatio * ((Mathf.PerlinNoise(Time.time * riseSpeedFrequency, 0f) - 0.5f) * riseSpeedVariance + 1f);
            transform.Translate(new Vector3(0f, speed * Time.deltaTime, 0f));
        }
        
        // Update audio
        if (_speedRatio > 0.01f)
        {
            if (!_lavaAudio.isPlaying)
            {
                _lavaAudio.Play();
            }
            // Update the volume
            _lavaAudio.volume = _speedRatio;
        }
        else if (_lavaAudio.isPlaying)
        {
            _lavaAudio.Stop();
        }
    }

    public void Extinguish(float cooldownTime)
    {
        _cooldown += cooldownTime;
    }

    public void OnUnlocked()
    {
        _isLocked = false;
    }

    private void UpdateMaterial()
    {
        _renderer.material.SetFloat(BrightnessParam, Mathf.Lerp(minBrightness, 1f, _speedRatio) * _defaultBrightness);
        _renderer.material.SetFloat(MaxHeightParam, _speedRatio * _defaultMaxHeight);
    }
}
