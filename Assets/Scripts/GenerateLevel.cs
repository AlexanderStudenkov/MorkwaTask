using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GenerateLevel : MonoBehaviour
{
    [SerializeField] private int gameFieldSize;
    [SerializeField] private int enemyCount;
    [SerializeField] private float speed;
    [SerializeField] private float maxNoiseLevel;
    [SerializeField] private float noiseUpSpeed;
    [SerializeField] private float noiseDownSpeed;

    [SerializeField] private float safeZoneRadius;

    private NavMeshPath _navMeshPath;
    private NavMeshSurface _navMesh;
    private List<GameObject> enemies;
    private GameObject startPoint, endPoint;
    private List<GameObject> emptyPoints;

    [SerializeField] private GameObject emptyPointPrefab;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private GameObject finishPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject floorCellPrefab;
    
    [SerializeField] private Text gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Player _player;

    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private AudioSource winSound;

    
    void Start()
    {
        _navMesh = GetComponent<NavMeshSurface>();
        _navMeshPath = new NavMeshPath();
        emptyPoints = new List<GameObject>();
        enemies = new List<GameObject>();

        gameOverPanel.SetActive(false);

        for(int i=0; i<gameFieldSize; i++)
        {
            for(int j = 0; j < gameFieldSize; j++)
            {
                Instantiate(floorCellPrefab, new Vector3(i, 0, j), Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90, 0));
                emptyPoints.Add(Instantiate(emptyPointPrefab, new Vector3(i, 0.5f, j), Quaternion.identity));
            }
        }

        startPoint = emptyPoints[0];
        endPoint = emptyPoints[emptyPoints.Count-1];

        emptyPoints.RemoveAt(0);
        emptyPoints.RemoveAt(emptyPoints.Count-1);

        GetDownPoint(startPoint.transform);
        GetDownPoint(endPoint.transform);

        _navMesh.transform.position = new Vector3(gameFieldSize/2f-0.5f, _navMesh.transform.position.y, gameFieldSize/2f-0.5f);
        _navMesh.size = new Vector3(gameFieldSize, 1, gameFieldSize);

        var lastBlock = CreateBlockInRandomPoint();
        _navMesh.BuildNavMesh();

        while (CheckPath(startPoint,endPoint))
        {
            lastBlock = CreateBlockInRandomPoint();
            _navMesh.RemoveData();
            _navMesh.BuildNavMesh();
        }
        lastBlock.SetActive(false);
        Destroy(lastBlock);
        _navMesh.RemoveData();
        _navMesh.BuildNavMesh();

        Instantiate(finishPrefab, endPoint.transform.position, Quaternion.identity);

        for(int i=emptyPoints.Count-1; i>=0; i--)
        {
            GetDownPoint(emptyPoints[i].transform);
            if (!CheckPath(emptyPoints[i], endPoint))
            {
                Destroy(emptyPoints[i]);
                emptyPoints.RemoveAt(i);
            }
        }

        for (int i = emptyPoints.Count-1; i>=0; i--)
        {
            if ((startPoint.transform.position - emptyPoints[i].transform.position).sqrMagnitude < safeZoneRadius*safeZoneRadius)
            {
                Destroy(emptyPoints[i]);
                emptyPoints.RemoveAt(i);
            }
        }

        for (int i=0; i<enemyCount; i++)
        {
            CreateEnemy();
        }

        for (int i = 0; i < emptyPoints.Count; i++)
        {
            Destroy(emptyPoints[i]);
        }
        emptyPoints.Clear();

        SetSpeed(_player.gameObject);
        _player.GetComponent<Player>().SetNoise(maxNoiseLevel, noiseUpSpeed, noiseDownSpeed);

        _player.MaxNoise.AddListener(Alarm);
        _player.Die.AddListener(GameOver);
        _player.Win.AddListener(PlayerWin);
    }

    private void EndGame(string message, AudioSource sound)
    {
        gameOverText.text = message;
        sound.Play();
        for (int i = 0; i < enemyCount; i++) enemies[i].GetComponent<Enemy>().StopFollow();
        StartCoroutine(ShowMenu());
    }

    private void PlayerWin()
    {
        EndGame("You win!", winSound);
    }

    private void GameOver()
    {
        EndGame("Game Over", gameOverSound);
    }

    IEnumerator ShowMenu()
    {
        yield return new WaitForSeconds(5);
        gameOverPanel.SetActive(true);
        StopCoroutine("Timer");
    }

    private void Alarm()
    {
        for(int i=0; i<enemies.Count; i++)
        {
            enemies[i].GetComponent<Enemy>().SetTargetAndStartFollow(_player.transform);
        }
        _player.MaxNoise.RemoveListener(Alarm);
    }

    private void CreateEnemy()
    {
        if (emptyPoints.Count < 2) return;

        int i = UnityEngine.Random.Range(0, emptyPoints.Count);
        int angle = UnityEngine.Random.Range(0, 360);

        var enemy = Instantiate(enemyPrefab, emptyPoints[i].transform.position, Quaternion.Euler(0, angle, 0));
        enemies.Add(enemy);

        Destroy(emptyPoints[i]);
        emptyPoints.RemoveAt(i);

        i = UnityEngine.Random.Range(0, emptyPoints.Count);
        var pos = new Vector3(emptyPoints[i].transform.position.x, 0, emptyPoints[i].transform.position.z);
        var enemyComp = enemy.GetComponent<Enemy>();
        enemyComp.SetEndPoint(pos);
        SetSpeed(enemy);
        Destroy(emptyPoints[i]);
        emptyPoints.RemoveAt(i);
    }

    
    private GameObject CreateBlockInRandomPoint()
    {
        int i = UnityEngine.Random.Range(0, emptyPoints.Count);
        
        var block = Instantiate(blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)], emptyPoints[i].transform.position, Quaternion.Euler(UnityEngine.Random.Range(0, 4) * 90, UnityEngine.Random.Range(0, 4) * 90, UnityEngine.Random.Range(0, 4) * 90));

        Destroy(emptyPoints[i]);
        emptyPoints.RemoveAt(i);
        return block;
    }

    private bool CheckPath(GameObject p1, GameObject p2)
    {
        NavMesh.CalculatePath(p1.transform.position, p2.transform.position, NavMesh.AllAreas, _navMeshPath);
        if (_navMeshPath.status == NavMeshPathStatus.PathComplete)
            return true;
        else
            return false;
    }

    private void GetDownPoint(Transform transform)
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void SetSpeed(GameObject obj)
    {
        var anim = obj.GetComponentInChildren<Animator>();
        anim.speed = speed * 0.5f;
        var navAgent = obj.GetComponent<NavMeshAgent>();
        navAgent.speed = speed;
    }
}
