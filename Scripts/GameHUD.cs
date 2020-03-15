using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public LevelManager levelManager;

    [Header("In-game UI Object references")]
    public Image crossHair;
    public Image mouseCursor;
    public Slider playerHealthSlider;
    public TMP_Text speedText;
    public TMP_Text waveTimerText;
    public TMP_Text bestTimeText;
    public TMP_Text notificationText;
    public TMP_Text countdownText;
    public GameObject menuGroup;
    public TMP_Text menuTitleText;
    public Button restartButton;
    public Button resumeButton;
    public Button backButton;

    [Header("Enemy UI Prefab references")]
    public GameObject indicatorsGroup;
    public Image enemyIndicator;
    public Image enemyDirectionIndicator;
    public Slider enemyHealthSlider;

    List<Image> enemyIndicators;
    List<Image> enemyDirectionIndicators;
    List<Slider> enemyHealthSliders;
    TMP_Text waveTimer;

    Vector3 playerPosition;
    Vector3 playerForward;
    Vector3 playerToEnemyDirection;
    Quaternion indicatorRotation;

    public Vector3 healthSliderPositionOffset;

    Vector3 enemyScreenPosition;
    Vector3 enemyScreenPositionRounded;

    Vector3 enemyDirectionIndicatorPosition;

    Bounds screenBounds;
    Vector3 screenMax;
    Vector3 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        crossHair.gameObject.SetActive(true);
        crossHair.rectTransform.localPosition = Vector3.zero;

        mouseCursor.gameObject.SetActive(true);
        mouseCursor.rectTransform.localPosition = Vector3.zero;

        enemyIndicators = new List<Image>();
        enemyDirectionIndicators = new List<Image>();
        enemyHealthSliders = new List<Slider>();

        playerHealthSlider.transform.gameObject.SetActive(true);

        waveTimerText.transform.gameObject.SetActive(false);
        bestTimeText.transform.gameObject.SetActive(false);
        notificationText.transform.gameObject.SetActive(false);

        menuGroup.SetActive(false);
        menuTitleText.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        screenMax = new Vector3(Screen.width, Screen.height, 1.0f);
        screenBounds.SetMinMax(Vector3.zero, screenMax);

        LevelManager.EnemyRemovedEvent += EnemyRemoved;
        EnemyShip.HitEvent += EnemyHit;
        PlayerController.HitEvent += PlayerHit;
    }

    void Update()
    {
        if (levelManager.pauseGame || levelManager.player.isDead)
        {
            HideGameUI();
            HideGameTextUI();
            ShowMenu();
        }
        else
        {
            HideMenu();
            ShowGameUI();
        }
    }

    void FixedUpdate()
    {
        if(screenMax.x != Screen.width && screenMax.y != Screen.height)
        {
            screenMax.x = Screen.width;
            screenMax.y = Screen.height;
            screenBounds.SetMinMax(Vector3.zero, screenMax);
        }

        mousePos = levelManager.player.mousePos;
        mousePos.z = 0;
        mouseCursor.rectTransform.position = mousePos;

        if(enemyIndicators.Count < levelManager.enemyShips.Count)
        {
            for(int i = 0; i < levelManager.enemyShips.Count - enemyIndicators.Count; i++)
            {
                Image indicator = Instantiate(enemyIndicator);
                indicator.transform.SetParent(indicatorsGroup.transform);
                indicator.rectTransform.localPosition = Vector3.zero;
                enemyIndicators.Add(indicator);

                Image directionIndicator = Instantiate(enemyDirectionIndicator);
                directionIndicator.transform.SetParent(indicatorsGroup.transform);
                directionIndicator.rectTransform.localPosition = Vector3.zero;
                enemyDirectionIndicators.Add(directionIndicator);

                Slider healthSlider = Instantiate(enemyHealthSlider);
                healthSlider.transform.SetParent(indicatorsGroup.transform);
                healthSlider.GetComponent<RectTransform>().localPosition = Vector3.zero;
                enemyHealthSliders.Add(healthSlider);
            }
        }


        if (!levelManager.pauseGame || !levelManager.player.isDead)
        {
            if (levelManager.player != null && (levelManager.player.isDead == false || levelManager.levelComplete == false))
            {

                speedText.text = (levelManager.player.forwardThrottle * 100f).ToString("F0");

                indicatorsGroup.gameObject.SetActive(true);

                if (enemyIndicators.Count > 0)
                {
                    playerPosition = levelManager.player.transform.position;
                    playerForward = levelManager.player.transform.forward;

                    for (int i = 0; i < enemyIndicators.Count; i++)
                    {
                        enemyScreenPosition = Camera.main.WorldToScreenPoint(levelManager.enemyShips[i].transform.position);
                        float enemyScreenZ = enemyScreenPosition.z;
                        enemyScreenPosition.z = 1f;

                        playerToEnemyDirection = levelManager.enemyShips[i].transform.position - playerPosition;

                        enemyScreenPositionRounded.x = Mathf.Round(enemyScreenPosition.x);
                        enemyScreenPositionRounded.y = Mathf.Round(enemyScreenPosition.y);
                        enemyScreenPositionRounded.z = Mathf.Round(enemyScreenPosition.z);

                        enemyIndicators[i].rectTransform.position = enemyScreenPositionRounded;
                        enemyHealthSliders[i].GetComponent<RectTransform>().position = enemyScreenPositionRounded;
                        enemyHealthSliders[i].GetComponent<RectTransform>().position += healthSliderPositionOffset;

                        enemyScreenPosition.z = enemyScreenZ;

                        //left
                        if (enemyScreenPosition.x < 0f)
                        {
                            indicatorRotation = Quaternion.AngleAxis(180f, Vector3.forward);
                        }
                        //right
                        else if (enemyScreenPosition.x > Screen.width)
                        {
                            indicatorRotation = Quaternion.AngleAxis(0f, Vector3.forward);
                        }
                        //down
                        else if (enemyScreenPosition.y < 0f)
                        {
                            indicatorRotation = Quaternion.AngleAxis(-90f, Vector3.forward);
                        }
                        //up
                        else if (enemyScreenPosition.y > Screen.height)
                        {
                            indicatorRotation = Quaternion.AngleAxis(90f, Vector3.forward);
                        }

                        //top left
                        if (enemyScreenPosition.x < 0f && enemyScreenPosition.y > Screen.height)
                        {
                            indicatorRotation = Quaternion.AngleAxis(-45f, Vector3.forward);
                        }
                        //top right
                        else if (enemyScreenPosition.x > Screen.width && enemyScreenPosition.y > Screen.height)
                        {
                            indicatorRotation = Quaternion.AngleAxis(45f, Vector3.forward);
                        }
                        //bottom left
                        else if (enemyScreenPosition.x < 0f && enemyScreenPosition.y < 0f)
                        {
                            indicatorRotation = Quaternion.AngleAxis(-135f, Vector3.forward);
                        }
                        //bottom right
                        else if (enemyScreenPosition.x > Screen.width && enemyScreenPosition.y < 0f)
                        {
                            indicatorRotation = Quaternion.AngleAxis(135f, Vector3.forward);
                        }

                        if (Vector3.Dot(playerForward, playerToEnemyDirection.normalized) < 0f)
                        {
                            enemyDirectionIndicatorPosition.x = Mathf.Clamp(enemyScreenPosition.x, 0f, Screen.width);
                            enemyDirectionIndicatorPosition.y = Mathf.Clamp(enemyScreenPosition.y, 0f, Screen.height);
                            enemyDirectionIndicatorPosition.z = 1f;

                            if (screenBounds.Contains(enemyDirectionIndicatorPosition))
                            {
                                //left side of the screen
                                if (FloatInRange(enemyScreenPosition.x, 0.0f, Screen.width * 0.5f) &&
                                    FloatInRange(enemyScreenPosition.y, 0.0f, Screen.height))
                                {
                                    enemyDirectionIndicatorPosition.x = 0f;
                                    indicatorRotation = Quaternion.AngleAxis(180f, Vector3.forward);

                                }
                                //right side of the screen
                                else if (FloatInRange(enemyScreenPosition.x, Screen.width * 0.5f, Screen.width) &&
                                    FloatInRange(enemyScreenPosition.y, 0.0f, Screen.height))
                                {
                                    enemyDirectionIndicatorPosition.x = Screen.width;
                                    indicatorRotation = Quaternion.AngleAxis(0f, Vector3.forward);
                                }
                                //top half of the screen
                                else if (FloatInRange(enemyScreenPosition.y, Screen.height * 0.5f, Screen.height) &&
                                    FloatInRange(enemyScreenPosition.x, 0.0f, Screen.width))
                                {
                                    enemyDirectionIndicatorPosition.y = Screen.height;
                                    indicatorRotation = Quaternion.AngleAxis(90f, Vector3.forward);
                                }
                                //bottom half of the screen
                                else if (FloatInRange(enemyScreenPosition.y, 0f, Screen.height * 0.5f) &&
                                    FloatInRange(enemyScreenPosition.x, 0.0f, Screen.width))
                                {
                                    enemyDirectionIndicatorPosition.y = 0f;
                                    indicatorRotation = Quaternion.AngleAxis(-90f, Vector3.forward);
                                }
                                //top left of the screen
                                else if (FloatInRange(enemyScreenPosition.x, 0.0f, Screen.width * 0.5f) &&
                                    FloatInRange(enemyScreenPosition.y, Screen.height * 0.5f, Screen.height))
                                {
                                    enemyDirectionIndicatorPosition.x = 0f;
                                    enemyDirectionIndicatorPosition.y = Screen.height;
                                    indicatorRotation = Quaternion.AngleAxis(-45f, Vector3.forward);
                                }
                                //top right of the screen
                                else if (FloatInRange(enemyScreenPosition.x, Screen.width * 0.5f, Screen.width) &&
                                    FloatInRange(enemyScreenPosition.y, Screen.height * 0.5f, Screen.height))
                                {
                                    enemyDirectionIndicatorPosition.x = Screen.width;
                                    enemyDirectionIndicatorPosition.y = Screen.height;
                                    indicatorRotation = Quaternion.AngleAxis(45f, Vector3.forward);
                                }
                                //bottom right of the screen
                                else if (FloatInRange(enemyScreenPosition.x, Screen.width * 0.5f, Screen.width) &&
                                    FloatInRange(enemyScreenPosition.y, 0f, Screen.height * 0.5f))
                                {
                                    enemyDirectionIndicatorPosition.x = Screen.width;
                                    enemyDirectionIndicatorPosition.y = 0f;
                                    indicatorRotation = Quaternion.AngleAxis(135f, Vector3.forward);
                                }
                                //bottom left of the screen
                                else if (FloatInRange(enemyScreenPosition.x, 0.0f, Screen.width * 0.5f) &&
                                    FloatInRange(enemyScreenPosition.y, 0f, Screen.height * 0.5f))
                                {
                                    enemyDirectionIndicatorPosition.x = 0f;
                                    enemyDirectionIndicatorPosition.y = 0f;
                                    indicatorRotation = Quaternion.AngleAxis(-135f, Vector3.forward);
                                }
                            }
                        }
                        else
                        {
                            enemyDirectionIndicatorPosition.x = Mathf.Clamp(enemyScreenPosition.x, 0f, Screen.width);
                            enemyDirectionIndicatorPosition.y = Mathf.Clamp(enemyScreenPosition.y, 0f, Screen.height);

                            if (screenBounds.Contains(enemyIndicators[i].rectTransform.position))
                            {
                                enemyDirectionIndicators[i].gameObject.SetActive(false);
                            }
                            else
                            {
                                enemyDirectionIndicators[i].gameObject.SetActive(true);
                            }
                        }

                        enemyDirectionIndicators[i].rectTransform.position = enemyDirectionIndicatorPosition;
                        enemyDirectionIndicators[i].rectTransform.localRotation = Quaternion.RotateTowards(enemyDirectionIndicators[i].rectTransform.localRotation, indicatorRotation, 100f);

                        if (Vector3.Dot(playerForward, playerToEnemyDirection) > 0f)
                        {
                            enemyIndicators[i].gameObject.SetActive(true);
                            enemyHealthSliders[i].gameObject.SetActive(true);
                        }
                        else
                        {
                            enemyIndicators[i].gameObject.SetActive(false);
                            enemyHealthSliders[i].gameObject.SetActive(false);
                        }
                    }
                }

                if (levelManager.waveComplete && levelManager.startNextWave == false && levelManager.levelComplete == false)
                {
                    if (levelManager.currentNextWaveStartDelay < 3.0f)
                    {
                        bestTimeText.gameObject.SetActive(false);
                        waveTimerText.gameObject.SetActive(false);

                        notificationText.gameObject.SetActive(true);
                        countdownText.text = levelManager.currentNextWaveStartDelay.ToString("F2");
                    }
                    else
                    {
                        if (levelManager.hasBeatenPreviousWaveTime)
                        {
                            bestTimeText.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    notificationText.gameObject.SetActive(false);

                    if (levelManager.levelComplete == false)
                    {
                        waveTimerText.gameObject.SetActive(true);
                        waveTimerText.text = levelManager.currentWaveTime.ToString("F2");
                    }
                    else
                    {
                        waveTimerText.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    void ShowMenu()
    {
        Cursor.visible = true;

        menuGroup.SetActive(true);

        menuTitleText.gameObject.SetActive(true);
        if(levelManager.pauseGame)
        {
            menuTitleText.text = "Game Paused";
        }
        if(levelManager.player.isDead)
        {
            menuTitleText.text = "You died";
        }
        if(levelManager.levelComplete)
        {
            menuTitleText.text = "Level complete";
        }

        if(levelManager.pauseGame && levelManager.levelComplete == false)
        {
            resumeButton.gameObject.SetActive(true);
        }
        else
        {
            resumeButton.gameObject.SetActive(false);
        }

        if (levelManager.player.isDead || levelManager.levelComplete)
        {
            restartButton.gameObject.SetActive(true);
        }
        else
        {
            restartButton.gameObject.SetActive(false);
        }
        backButton.gameObject.SetActive(true);
    }

    public void HideMenu()
    {
        Cursor.visible = false;

        menuGroup.SetActive(false);
        menuTitleText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    void ShowGameUI()
    {
        crossHair.gameObject.SetActive(true);
        mouseCursor.gameObject.SetActive(true);
        playerHealthSlider.gameObject.SetActive(true);
        speedText.gameObject.SetActive(true);
        indicatorsGroup.gameObject.SetActive(true);
    }

    public void HideGameUI()
    {
        crossHair.gameObject.SetActive(false);
        mouseCursor.gameObject.SetActive(false);
        playerHealthSlider.gameObject.SetActive(false);
        speedText.gameObject.SetActive(false);
        indicatorsGroup.gameObject.SetActive(false);
    }

    void ShowGameTextUI()
    {
        waveTimerText.gameObject.SetActive(true);
        notificationText.gameObject.SetActive(true);
    }

    void HideGameTextUI()
    {
        waveTimerText.gameObject.SetActive(false);
        notificationText.gameObject.SetActive(false);
    }

    public void ResetPlayerHealthSlider()
    {
        playerHealthSlider.value = 1f;
    }

    float ClampScreenPosition(float clampScreenValue, float clampScreenMax )
    {
        float Result = 0f;
        if (clampScreenValue > 0f && clampScreenValue < clampScreenMax * 0.5f)
        {
            Result = 0f;
        }
        else if (clampScreenValue >= clampScreenMax * 0.5f && clampScreenValue <= clampScreenMax)
        {
            Result = clampScreenMax;
        }

        return Result;
    }

    //inclusive
    bool FloatInRange(float value, float minRange, float maxRange)
    {
        return value > minRange && value <= maxRange;
    }

    void OnDestroy()
    {
        LevelManager.EnemyRemovedEvent -= EnemyRemoved;
        EnemyShip.HitEvent -= EnemyHit;
        PlayerController.HitEvent -= PlayerHit;
    }

    void EnemyRemoved(int id)
    {
        Image indicator = enemyIndicators[id];
        Image directionIndicator = enemyDirectionIndicators[id];
        Slider enemyHealthSlider = enemyHealthSliders[id];

        indicator.gameObject.SetActive(false);
        directionIndicator.gameObject.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);

        enemyIndicators.RemoveAt(id);
        enemyDirectionIndicators.RemoveAt(id);
        enemyHealthSliders.RemoveAt(id);

        Destroy(indicator.gameObject);
        Destroy(directionIndicator.gameObject);
        Destroy(enemyHealthSlider.gameObject);
    }

    void PlayerHit(PlayerController player)
    {
        playerHealthSlider.value = (1f / player.health) * player.currentHealth;
    }

    void EnemyHit(EnemyShip enemyShip)
    {
        int id = levelManager.enemyShips.IndexOf(enemyShip);

        Slider healthSlider = enemyHealthSliders[id];
        healthSlider.value = (1f / enemyShip.health) * enemyShip.currentHealth;
    }
}
