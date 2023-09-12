using System.Collections;
using UnityEngine;

public interface IItemInterface
{
    public bool OnPickedUp(GameObject other);
}

public class Item : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float pickUpAnimLength = 0.25f;
    public float cannotPickUpAnimLength = 1f;
    public float cannotPickUpAnimFrequency = 2f;
    public float cannotPickUpAnimAmplitude = 0.2f;
    
    public AudioClip pickUpAudio;
    public AudioClip cannotPickUpAudio;

    private bool _isPickedUp;
    private GameObject _pickedUpBy;

    void Update()
    {
        // Rotate around the y axis
        var newRot = transform.localRotation.eulerAngles;
        newRot.y = rotationSpeed * Time.time;
        transform.localRotation = Quaternion.Euler(newRot);
    }

    public bool IsPickedUp()
    {
        return _isPickedUp;
    }

    public void PickUp(GameObject other)
    {
        if (IsPickedUp())
        {
            return;
        }
        
        // Stop previous coroutines
        StopAllCoroutines();
        
        // Try to pick up the item
        var itemInterface = gameObject.GetComponentInParent<IItemInterface>();
        if (itemInterface != null && itemInterface.OnPickedUp(other))
        {
            _isPickedUp = true;
            _pickedUpBy = other;
            
            // Start the pick up anim coroutine
            StartCoroutine(PickUpCoroutine());

            // Play the pick up sound
            AudioManager.Instance.PlayAudio(pickUpAudio);
        }
        else
        {
            // Couldn't be picked up, play animation
            StartCoroutine(CannotPickUpCoroutine());
            
            // Play the sound
            AudioManager.Instance.PlayAudio(cannotPickUpAudio);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PickUp(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Lava"))
        {
            DestroyItem();
        }
    }

    private void DestroyItem()
    {
        Destroy(transform.parent.gameObject);
    }

    private IEnumerator PickUpCoroutine()
    {
        var elapsed = 0f;
        while (elapsed < pickUpAnimLength)
        {
            elapsed += Time.deltaTime;
            var animRatio = elapsed / pickUpAnimLength;
            
            // Interpolate toward the pick upper
            if (_pickedUpBy)
            {
                var startPos = transform.parent.position;
                var endPos = _pickedUpBy.transform.position;
                transform.position = Vector3.Lerp(startPos, endPos, animRatio);
            }

            // Update scale
            var newScale = Mathf.Lerp(1f, 0.25f, animRatio);
            transform.localScale = new Vector3(newScale, newScale, newScale);
            
            yield return null;
        }
        
        DestroyItem();
    }
    
    private IEnumerator CannotPickUpCoroutine()
    {
        var elapsed = 0f;
        while (elapsed < cannotPickUpAnimLength)
        {
            elapsed += Time.deltaTime;
            var animRatio = elapsed / cannotPickUpAnimLength;
            
            // Wiggle along the x axis
            var newPos = transform.localPosition;
            newPos.x = Mathf.Sin(animRatio * cannotPickUpAnimFrequency * Mathf.PI) * cannotPickUpAnimAmplitude;
            transform.localPosition = newPos;

            yield return null;
        }
    }
}
