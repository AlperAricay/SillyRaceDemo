using System;
using Interfaces;
using UnityEngine;

public class FinishLine : MonoBehaviour, IInteractable
{
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        switch (collisionType)
        {
            case PlayerController.CollisionType.Enter:
                if (runner.HasFinished) return;
                GameManager.Instance.OnRunnerFinished(runner);
                break;
            case PlayerController.CollisionType.Stay:
                break;
            case PlayerController.CollisionType.Exit:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(collisionType), collisionType, null);
        }
    }
}