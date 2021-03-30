using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<IRunner> CurrentRunners = new List<IRunner>();
    public CinemachineVirtualCamera thirdPersonCamera;

    [SerializeField] private Transform finishLine, rankingsParent, checkpointsParent;
    [SerializeField] private GameObject winPanel, losePanel;
    [SerializeField] private Text playerRankText;
    
    private IRunner _player;
    private readonly List<IRunner> _finishedRunners = new List<IRunner>();
    private List<Text> _rankingUsernameTexts = new List<Text>();
    private List<Checkpoint> _checkpoints = new List<Checkpoint>();
    private int _spawnPointIndex;

    private void Awake()
    {
        Instance = this;
        CurrentRunners.Clear();
        _finishedRunners.Clear();
        _checkpoints.Clear();
        foreach (Transform child in rankingsParent) _rankingUsernameTexts.Add(child.GetChild(1).GetComponent<Text>());
        for (int i = 0; i < checkpointsParent.childCount; i++)
            _checkpoints.Add(checkpointsParent.GetChild(i).GetComponent<Checkpoint>());
        _spawnPointIndex = 0;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    private void Update()
    {
        CurrentRunners = CurrentRunners.OrderBy(runner => Vector3.Distance(runner.RunnerTransform.position, finishLine.position)).ToList();
        playerRankText.text = (CurrentRunners.FindIndex(runner => runner == _player) + _finishedRunners.Count + 1).ToString();
    }

    public Vector3 GetSpawnPoint(IRunner runner)
    {
        var pos = _checkpoints[runner.CurrentCheckpointIndex].spawnPoints[_spawnPointIndex].position;
        
        if (_spawnPointIndex == _checkpoints[runner.CurrentCheckpointIndex].spawnPoints.Count - 1)
        {
            _spawnPointIndex = 0;
        }
        else
        {
            _spawnPointIndex++;
        }
        return pos;
    }

    public void OnRunnerFinished(IRunner runner)
    {
        if (runner.HasFinished || _finishedRunners.Count == 10) return;
        runner.HasFinished = true;
        CurrentRunners.Remove(runner);
        _finishedRunners.Add(runner);

        if (_finishedRunners.Count == 10)
            EndGame();
        else if (runner == _player)
        {
            var c = 0;
            CurrentRunners = CurrentRunners.OrderBy(runnerInList => Vector3.Distance(runnerInList.RunnerTransform.position, finishLine.position)).ToList();
            while (_finishedRunners.Count < 10)
            {
                _finishedRunners.Add(CurrentRunners[c]);
                c++;
            }
            EndGame();
        }
        /*if (runner == _player)
        {
            var c = 0;
            CurrentRunners = CurrentRunners.OrderBy(runnerInList => Vector3.Distance(runnerInList.RunnerTransform.position, finishLine.position)).ToList();
            while (FinishedRunners.Count < 10)
            {
                FinishedRunners.Add(CurrentRunners[c]);
                c++;
            }
            EndGame();
        }
        else if (FinishedRunners.Count == 10) EndGame();*/
    }
    
    public void EndGame()
    {
        playerRankText.transform.parent.gameObject.SetActive(false);
        if (_finishedRunners.Contains(_player))
        {
            //qualified
            for (var i = 0; i < _finishedRunners.Count; i++)
            {
                var runner = _finishedRunners[i];
                if (runner == _player)
                {
                    _rankingUsernameTexts[i].text = runner.Username.ToUpper();
                    _rankingUsernameTexts[i].color = Color.yellow;
                }
                else
                {
                    _rankingUsernameTexts[i].text = runner.Username.ToUpper();
                }
            }
            winPanel.SetActive(true);
        }
        else
        {
            //eliminated
            losePanel.SetActive(true);
        }
    }
}
