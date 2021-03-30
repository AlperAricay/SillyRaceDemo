using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class TosserCollider : MonoBehaviour, IImpulse
{
    [SerializeField] private float impulseForce = 100f;

    public void Impulse(IRunner runner, Collision collision)
    {
        runner.HandleRagdoll(impulseForce, collision.GetContact(0).point);
    }
}
