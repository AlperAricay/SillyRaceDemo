using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HalfDonutNavMeshHelper : MonoBehaviour
{
    [SerializeField] private List<GameObject> navMeshWarnings;
    [SerializeField] private DOTweenAnimation doTweenToWarnFor;

    public void StartWarningTimer() => StartCoroutine(WarnDelayed());

    public void StopWarning()
    {
        foreach (var warning in navMeshWarnings) warning.SetActive(false);
    }

    private IEnumerator WarnDelayed()
    {
        yield return new WaitForSeconds(doTweenToWarnFor.delay - 1);
        foreach (var warning in navMeshWarnings) warning.SetActive(true);
    }
}
