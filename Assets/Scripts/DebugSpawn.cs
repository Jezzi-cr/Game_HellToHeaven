using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpawn : MonoBehaviour
{
    private float _lastSpawnTime;

    public float GetTimeSinceLastSpawn()
    {
        return Time.time - _lastSpawnTime;
    }
    
    public void OnSpawned()
    {
        _lastSpawnTime = Time.time;
    }
}
