using System;
using Interfaces;
using UnityEngine;

public class FinishLine : MonoBehaviour, IInteractable
{
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        if (collisionType == PlayerController.CollisionType.Enter)
        {
            if (runner.HasFinished) return;
            GameManager.Instance.OnRunnerFinished(runner);
        }
    }
}