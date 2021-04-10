using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalObstacleNavMeshHelper : MonoBehaviour
{
    [SerializeField] private Transform navMeshObstaclesParent;

    private GameObject _navMeshObstacle1, _navMeshObstacle2;

    private void Awake()
    {
        _navMeshObstacle1 = navMeshObstaclesParent.GetChild(0).gameObject;
        _navMeshObstacle2 = navMeshObstaclesParent.GetChild(1).gameObject;
    }

    public void SwitchNavMeshObstacle()
    {
        _navMeshObstacle1.SetActive(!_navMeshObstacle1.activeSelf);
        _navMeshObstacle2.SetActive(!_navMeshObstacle2.activeSelf);
    }
}
