using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public List<IRunner> CurrentRunners = new List<IRunner>(), FinishedRunners = new List<IRunner>();
    public List<Transform> spawnPoints = new List<Transform>();
    
    [SerializeField] private Transform finishLine, rankingsParent;
    [SerializeField] private GameObject winPanel, losePanel;
    [SerializeField] private Text playerRankText;

    private IRunner _player;
    private List<Text> _rankingUsernameTexts = new List<Text>();
    private int _spawnPointIndex;
    
    private void Awake()
    {
        Instance = this;
        CurrentRunners.Clear();
        FinishedRunners.Clear();
        foreach (Transform child in rankingsParent) _rankingUsernameTexts.Add(child.GetChild(1).GetComponent<Text>());
        _spawnPointIndex = 0;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    private void Update()
    {
        CurrentRunners = CurrentRunners.OrderBy(runner => Vector3.Distance(runner.RunnerTransform.position, finishLine.position)).ToList();
        playerRankText.text = (CurrentRunners.FindIndex(runner => runner == _player) + FinishedRunners.Count + 1).ToString();
    }

    public Vector3 GetSpawnPoint()
    {
        var pos = spawnPoints[_spawnPointIndex].position;
        if (_spawnPointIndex == spawnPoints.Count - 1)
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
        if (runner.HasFinished || FinishedRunners.Count == 10) return;
        runner.HasFinished = true;
        CurrentRunners.Remove(runner);
        FinishedRunners.Add(runner);

        if (FinishedRunners.Count == 10)
            EndGame();
        else if (runner == _player)
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
        if (FinishedRunners.Contains(_player))
        {
            //qualified
            for (var i = 0; i < FinishedRunners.Count; i++)
            {
                var runner = FinishedRunners[i];
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
