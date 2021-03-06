using System;
using Interfaces;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour, IInteractable
{
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        switch (collisionType)
        {
            case PlayerController.CollisionType.Enter:
                runner.RunnerTransform.parent = transform;
                break;
            case PlayerController.CollisionType.Stay:
                runner.RunnerTransform.parent = transform;
                break;
            case PlayerController.CollisionType.Exit:
                runner.RunnerTransform.parent = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(collisionType), collisionType, null);
        }
    }
}
