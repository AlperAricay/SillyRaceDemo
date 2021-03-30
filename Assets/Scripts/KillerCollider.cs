using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class KillerCollider : MonoBehaviour, IInteractable
{
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        if (collisionType == PlayerController.CollisionType.Enter) runner.Respawn();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
            other.gameObject.GetComponentInParent<IRunner>().Respawn();
    }
}
