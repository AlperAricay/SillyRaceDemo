using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    public List<Transform> spawnPoints = new List<Transform>();
    public int checkPointIndex;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++) spawnPoints.Add(transform.GetChild(i));
    }
    
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        if (collisionType == PlayerController.CollisionType.Enter)
        {
            if (runner.CurrentCheckpointIndex >= checkPointIndex) return;
            runner.CurrentCheckpointIndex = checkPointIndex;
        }
    }
}
