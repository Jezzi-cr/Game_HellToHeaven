using System;
using UnityEngine;

[ExecuteInEditMode]
public class Door : MonoBehaviour, IKeyInterface
{
    public bool startOpen;
    public float width = 3f;
    
    public Transform door;
    public Transform leftFrame;
    public Transform rightFrame;
    public float transitionSpeed = 0.5f;
    
    private bool _isOpen;
    private float _openRatio;

    private void Awake()
    {
        leftFrame.localPosition = new Vector3(width * -0.5f, 0f, 0f);
        rightFrame.localPosition = new Vector3(width * 0.5f, 0f, 0f);
        door.localScale = new Vector3(width, 1f, 1f);
        
        _isOpen = startOpen;
        _openRatio = _isOpen ? 1f : 0f;
        // Update initially
        UpdateDoor();
    }

    private void Update()
    {
        // Update the open ratio
        var targetOpenRatio = _isOpen ? 1f : 0f;
        var delta = targetOpenRatio - _openRatio;
        if (Math.Abs(delta) > 0.001f)
        {
            _openRatio = Mathf.Clamp01(_openRatio + Mathf.Sign(delta) * transitionSpeed * Time.deltaTime);
            UpdateDoor();
        }
    }

    private void UpdateDoor()
    {
        door.localPosition = Vector3.left * (width * _openRatio);
    }
    
    public void OnUnlocked()
    {
        if (!_isOpen)
        {
            _isOpen = true;
        }
    }
}
