using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

public class OpponentController : MonoBehaviour, IRunner
{
    public bool IsStanding { get; set; }
    public int CurrentCheckpointIndex { get; set; }
    public string Username { get; private set; }
    public Transform RunnerTransform { get; private set; }
    public bool HasFinished { get; set; }
    
    private void Awake()
    {
        RunnerTransform = transform;
        Username = "Bot " + Random.Range(1, 9999);
        CurrentCheckpointIndex = 0;
    }

    private void Start()
    {
        GameManager.Instance.CurrentRunners.Add(this);
        transform.position = GameManager.Instance.GetSpawnPoint(this);
    }

    public void HandleRagdoll(bool newValue)
    {
        throw new System.NotImplementedException();
    }

    public void HandleRagdoll(float impulseForce, Vector3 impulseDirection)
    {
        throw new NotImplementedException();
    }

    public void Jump(float jumpPower)
    {
        throw new System.NotImplementedException();
    }

    public void Respawn()
    {
        throw new System.NotImplementedException();
    }

    public void Finish()
    {
        throw new System.NotImplementedException();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, PlayerController.CollisionType.Enter);
        }
    }
}
