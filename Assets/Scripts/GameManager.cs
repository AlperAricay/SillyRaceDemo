using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Interfaces;
using PaintIn3D;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<IRunner> CurrentRunners = new List<IRunner>();
    public CinemachineVirtualCamera thirdPersonCamera;
    public Transform paintingTransform, paintingSphere;

    [SerializeField] private Transform finishLine, rankingsParent, checkpointsParent;
    [SerializeField] private GameObject winPanel, losePanel, rankingPanel, paintingPanel;
    [SerializeField] private Text playerRankText;
    [Range(0,1)][SerializeField] private float requiredPaintingPercentage = 0.95f;
    
    private IRunner _player;
    private readonly List<IRunner> _finishedRunners = new List<IRunner>();
    private List<Text> _rankingUsernameTexts = new List<Text>();
    private List<Checkpoint> _checkpoints = new List<Checkpoint>();
    private int _spawnPointIndex;
    private PlayerController _playerControllerInstance;
    private P3dChangeCounter _p3dChangeCounter;

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
        _p3dChangeCounter = paintingSphere.parent.GetComponent<P3dChangeCounter>();
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _playerControllerInstance = PlayerController.Instance;
    }

    private void Update()
    {
        switch (_playerControllerInstance.currentPhase)
        {
            case PlayerController.GameplayPhases.StartingPhase:
                break;
            case PlayerController.GameplayPhases.RacingPhase:
                CurrentRunners = CurrentRunners.OrderBy(runner => Vector3.Distance(runner.RunnerTransform.position, finishLine.position)).ToList();
                playerRankText.text = (CurrentRunners.FindIndex(runner => runner == _player) + _finishedRunners.Count + 1).ToString();
                break;
            case PlayerController.GameplayPhases.PaintingPhase:
                if (_p3dChangeCounter.Ratio <= 1 - requiredPaintingPercentage)
                {
                    EndGame();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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

        if (runner == _player)
        {
            var c = 0;
            CurrentRunners = CurrentRunners.OrderBy(runnerInList =>
                Vector3.Distance(runnerInList.RunnerTransform.position, finishLine.position)).ToList();
            while (_finishedRunners.Count < 10)
            {
                _finishedRunners.Add(CurrentRunners[c]);
                c++;
            }

            _playerControllerInstance.SwitchPhase(PlayerController.GameplayPhases.PaintingPhase);
        }
        else if (_finishedRunners.Count == 10)
            EndGame();
        
        /*if (_finishedRunners.Count == 10)
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
            EndGame();//end it when the painting is done
        }*/
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

    public void PaintingPanelActivator()
    {
        rankingPanel.SetActive(false);
        paintingPanel.SetActive(true);
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
