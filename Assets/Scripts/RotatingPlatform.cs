using Interfaces;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour, IInteractable
{
    public void Interact(IRunner runner, bool entered)
    {
        runner.RunnerTransform.parent = entered ? transform : null;
    }
}
