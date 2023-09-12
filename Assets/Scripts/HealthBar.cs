using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Vector3 offset;

    public RectTransform canvas;
    public Slider slider;
    public Image fill;
    public Image border;
    public Gradient gradient;
    public float followSpeed = 16f;

    private RectTransform _rectTransform;
    private float _health = -1f;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        var character = GameManager.Instance.GetPlayerCharacter();
        if (Camera.current && character)
        {
            // Update health
            SetHealth(character.GetHealth());
            
            // Update position
            var viewportPos = Camera.current.WorldToViewportPoint(character.transform.position + offset);
            var targetPos = viewportPos * canvas.sizeDelta;
            var currentPos = _rectTransform.anchoredPosition;
            _rectTransform.anchoredPosition = Vector2.Lerp(currentPos, targetPos, followSpeed * Time.deltaTime);
        }
    }

    private void SetHealth(float val)
    {
        if (Math.Abs(val - _health) > Mathf.Epsilon)
        {
            _health = val;
            // Update the slider
            slider.value = _health;
            
            // Update color and opacity
            var alpha = _health < 1f - Mathf.Epsilon ? 1f : 0f;
            var fillColor = gradient.Evaluate(_health);
            fillColor.a = alpha;
            fill.color = fillColor;
            
            var borderColor = border.color;
            borderColor.a = alpha;
            border.color = borderColor;
        }
    }
}
