using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static event Action<int> EnemyRemovedEvent;

    public GameObject playerObject;
    public PlayerController player;
    public SoundManager soundManager;
    public string levelMusic;
    public string bgSound;

    public bool hasBeatenPreviousWaveTime { get; set; }
    public bool waveComplete { get; set; }
    public bool startNextWave { get; set; }
    public bool levelComplete { get; set; }

    //enemy spawn variables
    public bool spawnShips = false;
    public GameObject enemyShipObject;
    public float spawnShipDelay = 5f;
    public float spawnShipDistance = 500f;

    public bool spawnInfiniteWaves = false;
    public float timeToLevelCompletionInSeconds = 60f;
    public int[] enemiesPerWave;

    public int currentWave { get; set; }
    public float currentWaveTime { get; set; }
    public float currentNextWaveStartDelay { get; set; }

    float nextWaveStartDelay = 6.0f;
    
    float[] waveTimes;

    public Transform target;

    //rock spawn variables
    public bool spawnRocks = false;
    public GameObject rock;
    public int rockCount = 100;
    public float spawnMinRange = 100f;
    public float spawnMaxRange = 1000f;
    public float minRockScale = 30.0f;
    public float maxRockScale = 50.0f;

    public float randomAttackerSelectionInterval = 6f;
    float randomAttackerSelectionTimer = 0f;

    int spawnedShipsCount = 0;
    float spawnTimer = 0f;
    bool canSpawnShips = false;

    //game variables
    public CinemachineBrain brain;
    public CinemachineVirtualCamera[] cameras;

    public bool pauseGame { get; set; }
    float fixedDeltaTime;

    //[HideInInspector]
    public List<EnemyShip> enemyShips;

    int scoreCounter = 0;

    int cameraIndex = 0;
    CinemachineVirtualCamera currentCamera;

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Pause.performed += ctx => pauseGame = !pauseGame;
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();    
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        waveTimes = new float[enemiesPerWave.Length];

        if (brain != null)
        {
            currentCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        }

        if (spawnRocks)
        {
            SpawnRocks();
        }

        currentNextWaveStartDelay = nextWaveStartDelay;

        waveComplete = true;
        startNextWave = false; //use this with events to control when the next wave will start

        Time.timeScale = 1f;

        soundManager = SoundManager.instance;

        SoundManager.instance.PlaySound(levelMusic);
        SoundManager.instance.PlaySound(bgSound);

        EnemyShip.DestroyEvent += OnEnemyKilled;
    }

    void Update()
    {
        //if(Input.GetButtonDown("Pause"))
        //{
        //    pauseGame = !pauseGame;
        //}

        if(pauseGame)
        {
            PauseGame();
        }
        else
        {
            UnpauseGame();

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (currentCamera != null)
                {
                    currentCamera.Priority = 0;
                }

                cameraIndex++;
                if (cameraIndex > cameras.Length - 1)
                {
                    cameraIndex = 0;
                }

                currentCamera = cameras[cameraIndex];
                currentCamera.Priority = 1;
            }


            if (levelComplete == false)
            {
                if (spawnInfiniteWaves)
                {

                }
                else
                {
                    if (currentWave < enemiesPerWave.Length)
                    {
                        if (waveComplete && startNextWave)
                        {
                            //Debug.Log("start wave " + (currentWave + 1));

                            waveComplete = false;
                            startNextWave = false;

                            currentWaveTime = 0f;

                            canSpawnShips = true;
                            hasBeatenPreviousWaveTime = false;
                        }

                        if (spawnedShipsCount == enemiesPerWave[currentWave] && player.isDead == false)
                        {
                            canSpawnShips = false;

                            if (randomAttackerSelectionTimer == 0f)
                            {
                                SelectRandomAttacker();
                            }

                            randomAttackerSelectionTimer += Time.deltaTime;

                            if (randomAttackerSelectionTimer > randomAttackerSelectionInterval)
                            {
                                randomAttackerSelectionTimer = 0f;
                            }
                        }

                        if (scoreCounter == enemiesPerWave[currentWave])
                        {
                            waveComplete = true;

                            SaveBestTime();

                            waveTimes[currentWave] = currentWaveTime;
                            currentWave++;

                            scoreCounter = 0;
                            spawnedShipsCount = 0;
                        }
                    }
                    else
                    {
                        levelComplete = true;
                    }
                }

                if (waveComplete && startNextWave == false)
                {
                    currentNextWaveStartDelay -= Time.deltaTime;

                    if (currentNextWaveStartDelay < 0f)
                    {
                        startNextWave = true;
                        currentNextWaveStartDelay = nextWaveStartDelay;
                    }
                }
            }

            if (canSpawnShips)
            {
                SpawnEnemies();
            }

            currentWaveTime += Time.deltaTime;

            if (levelComplete)
            {
                PauseGame();
                player.isGamePaused = true;
            }

            //if(player.isDead)
            //{
            //    PauseGame();
            //}

        }
    }

    /// <summary>
    /// This works but instead reloading the scene to restart
    /// </summary>
    /*
    public void ResetLevel()
    {
        if (player != null)
        {
            player.OnDestroy();
        }

        player = Instantiate(playerObject).GetComponent<PlayerController>();
        target = player.transform;

        waveComplete = true;
        startNextWave = false; //use this with events to control when the next wave will start

        currentNextWaveStartDelay = nextWaveStartDelay;

        scoreCounter = 0;
        spawnedShipsCount = 0;

        currentWave = 0;
        currentWaveTime = 0f;

        for(int i = enemyShips.Count - 1; i >= 0; i--)
        {
            EnemyRemovedEvent.Invoke(i);

            EnemyShip enemyShip = enemyShips[i];
            enemyShips.RemoveAt(i);

            enemyShip.OnDestroy();
        }

        canSpawnShips = false;

        //start again
        levelComplete = false;

        UnpauseGame();

    }
    */

    public void UnpauseGame()
    {
        pauseGame = false;
        player.isGamePaused = false;

        Time.timeScale = 1f;

        foreach (EnemyShip enemyShip in enemyShips)
        {
            enemyShip.isGamePaused = pauseGame;
        }

        //Time.fixedDeltaTime = fixedDeltaTime;
    }

    public void PauseGame()
    {
        pauseGame = true;
        player.isGamePaused = true;

        Time.timeScale = 0f;

        foreach(EnemyShip enemyShip in enemyShips)
        {
            enemyShip.isGamePaused = pauseGame;
        }
    }

    void SpawnRocks()
    {
        if (rock != null)
        {
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 randomPoint = Random.insideUnitSphere * Random.Range(spawnMinRange, spawnMaxRange);
                GameObject rockInstance = Instantiate(rock);
                rockInstance.transform.position = randomPoint;
                rockInstance.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
                rockInstance.transform.localScale *= Random.Range(minRockScale, maxRockScale);
            }
        }
    }

    void SpawnEnemies()
    {
        if (spawnShips)
        {
            if (spawnTimer > spawnShipDelay)
            {
                spawnTimer = 0f;
            }

            if (spawnedShipsCount < enemiesPerWave[currentWave] && spawnTimer == 0.0f)
            {
                var enemy = Instantiate(enemyShipObject);
                enemy.transform.position = transform.position + (Random.onUnitSphere * spawnShipDistance);

                EnemyShip enemyShip = enemy.GetComponent<EnemyShip>();
                enemyShip.targetTransform = target;
                spawnedShipsCount++;

                enemyShips.Add(enemyShip);
            }

            spawnTimer += Time.deltaTime;
        }
    }

    void SelectRandomAttacker()
    {
        //Debug.Log("selecting random attacker");

        if(enemyShips.Count > 0)
        {
            int id = Random.Range(0, enemyShips.Count);
            EnemyShip enemyShip = enemyShips[id];

            if (enemyShip.isAttacking == false)
            {
                enemyShip.isAttacking = true;
                enemyShip.attackDuration = randomAttackerSelectionInterval * 0.3333f;
            }
            else
            {
                enemyShip.isAttacking = false;
                enemyShips[(id + 1) % enemyShips.Count].isAttacking = true;
            }

            //Debug.Log("enemy " + id + " is attacking");
        }
    }

    void OnEnemyKilled(EnemyShip enemyShip)
    {
        scoreCounter++;

        int id = enemyShips.IndexOf(enemyShip);
        EnemyRemovedEvent.Invoke(id);

        enemyShips.Remove(enemyShip);
    }

    void SaveBestTime()
    {
        string key = ("Wave" + currentWave + "Time").ToString();
        if (PlayerPrefs.HasKey(key))
        {
            float previousTime = PlayerPrefs.GetFloat(key);
            if(currentWaveTime < previousTime)
            {
                PlayerPrefs.SetFloat(key, currentWaveTime);
                hasBeatenPreviousWaveTime = true;
            }
        }
        else
        {
            PlayerPrefs.SetFloat(key, currentWaveTime);
        }

        PlayerPrefs.Save();
    }

    public void StopSounds()
    {
        soundManager.StopAllSounds();
    }

    private void OnDestroy()
    {
        EnemyShip.DestroyEvent -= OnEnemyKilled;
    }
}
