using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Interfaces;
using PaintIn3D;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<IRunner> CurrentRunners = new List<IRunner>();
    public CinemachineVirtualCamera thirdPersonCamera, thirdPersonCameraUpwards, paintingCamera;
    public Transform paintingTransform, paintingSphere;
    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    
    [SerializeField] private Transform finishLine, rankingsParent, checkpointsParent;
    [SerializeField] private GameObject winPanel, losePanel, rankingPanel, paintingPanel;
    [SerializeField] private Text playerRankText;
    [Range(0,1)][SerializeField] private float requiredPaintingPercentage = 0.95f;
    
    private IRunner _player;
    private readonly List<IRunner> _finishedRunners = new List<IRunner>();
    private List<Text> _rankingUsernameTexts = new List<Text>();

    private int _currentID;
    private int _spawnPointIndex;
    private PlayerController _playerControllerInstance;
    private P3dChangeCounter _p3dChangeCounter;

    private void Awake()
    {
        Instance = this;
        _currentID = 0;
        CurrentRunners.Clear();
        _finishedRunners.Clear();
        checkpoints.Clear();
        foreach (Transform child in rankingsParent) _rankingUsernameTexts.Add(child.GetChild(1).GetComponent<Text>());
        for (int i = 0; i < checkpointsParent.childCount; i++)
            checkpoints.Add(checkpointsParent.GetChild(i).GetComponent<Checkpoint>());
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
                CurrentRunners = CurrentRunners.OrderByDescending(runner => runner.CurrentCheckpointIndex)
                    .ThenBy(runner => Vector3.Distance(runner.RunnerTransform.position,
                        checkpoints.Count <= runner.CurrentCheckpointIndex + 1
                            ? checkpoints[runner.CurrentCheckpointIndex].transform.position
                            : checkpoints[runner.CurrentCheckpointIndex + 1].transform.position))
                    .ToList();
                // CurrentRunners = CurrentRunners.OrderByDescending(runner => runner.CurrentCheckpointIndex)
                //     .ThenBy(runner => Vector3.Distance(runner.RunnerTransform.position,
                //         finishLine.position))
                //     .ToList();
                playerRankText.text = (CurrentRunners.FindIndex(runner => runner == _player) + _finishedRunners.Count + 1).ToString();
                break;
            case PlayerController.GameplayPhases.PaintingPhase:
                if (_p3dChangeCounter.Ratio <= 1 - requiredPaintingPercentage)
                {
                    EndGame();
                    _playerControllerInstance.OnGameFinished();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public int GetRunnerID()
    {
        var returningID = _currentID;
        _currentID++;
        return returningID;
    }
    
    public Vector3 GetSpawnPoint(IRunner runner)
    {
        var pos = checkpoints[runner.CurrentCheckpointIndex].spawnPoints[_spawnPointIndex].position;
        
        if (_spawnPointIndex == checkpoints[runner.CurrentCheckpointIndex].spawnPoints.Count - 1)
        {
            _spawnPointIndex = 0;
        }
        else
        {
            _spawnPointIndex++;
        }
        return pos;
    }
    
    public Vector3 GetSpawnPoint(IRunner runner, int runnerID)
    {
        var pos = checkpoints[runner.CurrentCheckpointIndex].spawnPoints[runnerID].position;
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

            foreach (var currentRunner in CurrentRunners)
            {
                currentRunner.HasFinished = true;
            }
        }
        else if (_finishedRunners.Count == 10)
            EndGame();
    }

    public void PaintingPanelActivator()
    {
        rankingPanel.SetActive(false);
        paintingPanel.SetActive(true);
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    private void EndGame()
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
