using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject camObject;
    public UIManager uIManager;
    public ScoreManager scoreManager;

    [Header("Game settings")]
    [Space(5)]
    public Color[] colorTable;
    [Space(5)]
    public Color goodColor, wrongColor;
    [Space(5)]
    public GameObject obstaclePrefab;
    [Space(5)]
    [Range(1, 10)]
    public float minObstacleSpeed = 1;
    [Range(2, 10)]
    public int maxObstacleSpeed = 2;
    [Space(5)]
    public List<GameObject> obstacleList = new List<GameObject>();
    [Space(5)]
    [Range(4,37)]
    public int minBridgeLength = 5;
    [Range(4, 37)]
    public int maxBridgeLength = 8;
    [Space(5)]
    public float firstObstacleY = 1; //y position of first obstacle
    [Space(5)]
    public float heightDistanceLastFirst = 1; //difference in y position between first and last obstacle

    [Space(25)]
    Vector2 screenBounds;

    float followSpeed = 4f; //how fast camera show scene
    float obstacleSpeed = 2f;
    public int bridgeLength, obstacleIndex;
    float obstacleWidth, obstacleHeight;
    GameObject lastObstacle, tempObstacle, bridgeEnd;
    bool cameraOnStart, playing, canCreateObstacle;
    Vector2 cameraStartPos, targetPosition, tempPos;
    float obstaclePositionY;

    [SerializeField] int Dado;
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;

        camObject = Camera.main.gameObject;
        cameraStartPos = camObject.transform.position;
        minBridgeLength = 5;
        maxBridgeLength = 8;
        minObstacleSpeed = 1;
        maxObstacleSpeed = 2;

        //get screen bounds to set position for first square obstacle
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        CreateScene();
    }

    void Update()
    {
        if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButton(0))
        {
            if (uIManager.IsButton())
                return;

            //stop obstacle and check if position is same or lower than previous block
            if (playing && canCreateObstacle)
            {
                canCreateObstacle = false;
                lastObstacle.GetComponent<Obstacle>().StopMoving();

                //if stopped obstacle position is over previous then trigger game over
                if (lastObstacle.transform.position.y - (.05f * obstacleHeight) > obstacleList[obstacleIndex - 1].transform.position.y)
                {
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
                    playing = false;
                    GameOver();

                    return;
                }
                else if (lastObstacle.transform.position.y - (.05f * obstacleHeight) < obstacleList[obstacleIndex - 1].transform.position.y && lastObstacle.transform.position.y + (.05f * obstacleHeight) > obstacleList[obstacleIndex - 1].transform.position.y) //perfect stop (player can stop little higher or lower -> 5% of block heigh)
                {
                    lastObstacle.transform.position = new Vector2(lastObstacle.transform.position.x, obstacleList[obstacleIndex - 1].transform.position.y);
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.perfect);
                    ScoreManager.Instance.UpdateScore(2);

                }
                else //block is lower than previous
                {
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.sameColor);

                    ScoreManager.Instance.UpdateScore(1);

                }

                //finished bridge
                if (obstacleIndex == bridgeLength && lastObstacle.transform.position.y + (.05f * obstacleHeight) > bridgeEnd.transform.position.y)  // Con solo conectar los cubos bien al final esta bien
                {
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.perfect);

                    for (int i = 0; i < obstacleList.Count; i++)
                    {
                        obstacleList[i].GetComponent<Renderer>().materials[1].color = goodColor;
                    }

                    ScoreManager.Instance.UpdateScore(4);
                    playing = false;
                    StartCoroutine(NewScene(1.5f));
                }
                else if (obstacleIndex == bridgeLength) //bridge is lower than last obstacle
                {
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
                    playing = false;
                    GameOver();


                }
                else 
                    SpawnObstacle();

              
            }
        }
        else if (uIManager.gameState == GameState.PLAYING) //show first and last part of bridge
        {
            //show first and last obstacle on game start
            if (!cameraOnStart)
            {
                cameraOnStart = true;
                StartCoroutine(ShowStartEnd());
                return;
            }

            if (cameraOnStart && !playing) //move camera when showing scene
            {
                tempPos = Vector2.Lerp(camObject.transform.position, targetPosition, followSpeed * Time.deltaTime);
                camObject.transform.position = new Vector3(tempPos.x, 0, 0); //reset z and y of camera (move only of x axis)
            }
            else if (playing) //move camera during gameplay
            {
                if (lastObstacle.transform.position.x > 0)
                {
                    tempPos = Vector2.Lerp(camObject.transform.position, lastObstacle.transform.position, followSpeed * Time.deltaTime);
                    camObject.transform.position = new Vector3(tempPos.x, 0, 0); //reset z and y of camera (move only of x axis)
                }
            }

        }

        if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonUp(0)) //prepare for next spawned block
        {
            canCreateObstacle = true;
        }

    }

    //create start scene
    public void CreateScene()
    {


        bridgeLength = Random.Range(minBridgeLength, maxBridgeLength + 1);
        Dado = Random.Range(0, 2);
        cameraOnStart = false;
        obstacleIndex = 0;

        heightDistanceLastFirst = Random.Range(1, 6);

        //create first bridge part
        lastObstacle = Instantiate(obstaclePrefab);

        obstacleWidth = lastObstacle.GetComponent<Renderer>().bounds.size.x; //calculate obstacle width
        obstacleHeight = lastObstacle.GetComponent<Renderer>().bounds.size.y; //calculate obstacle height

        lastObstacle.GetComponent<Renderer>().materials[1].color = colorTable[Random.Range(0, colorTable.Length)];
        lastObstacle.transform.position = new Vector2(-screenBounds.x + obstacleWidth / 2, firstObstacleY);

        obstacleIndex++;

        //create last bridge part
        bridgeEnd = Instantiate(obstaclePrefab);
        bridgeEnd.GetComponent<Renderer>().materials[1].color = colorTable[Random.Range(0, colorTable.Length)];
        bridgeEnd.transform.position = new Vector2(lastObstacle.transform.position.x + (bridgeLength * obstacleWidth), firstObstacleY - heightDistanceLastFirst);
        targetPosition = cameraStartPos;

        obstacleList.Add(bridgeEnd);
        obstacleList.Add(lastObstacle);
    }

    //create new obstacle
    void SpawnObstacle()
    {

        obstacleSpeed = Random.Range(minObstacleSpeed, maxObstacleSpeed);
        //create first bridge part
        tempObstacle = Instantiate(obstaclePrefab);

        //random spawn obstacle on top or bottom of screen
        if (Random.Range(0, 2) == 0)
            obstaclePositionY = screenBounds.y + obstacleHeight / 2;
        else
            obstaclePositionY = -screenBounds.y - obstacleHeight / 2;

        tempObstacle.GetComponent<Renderer>().materials[1].color = colorTable[Random.Range(0, colorTable.Length)];
        tempObstacle.transform.position = new Vector2(lastObstacle.transform.position.x + obstacleWidth, obstaclePositionY);
        tempObstacle.GetComponent<Obstacle>().StartMoving(obstacleSpeed);
        lastObstacle = tempObstacle;
        obstacleList.Add(lastObstacle);
        obstacleIndex++;

    }

    //create new scene
    IEnumerator NewScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearScene();
        // Level UP--------------------------------------------

          if (scoreManager.puntuacionActual > 7)
        {
            minBridgeLength = 5;
            maxBridgeLength = 8;
            minObstacleSpeed = 2;
            maxObstacleSpeed = 3;
        }
        if (scoreManager.puntuacionActual > 14)
        {
            minBridgeLength = 5;
            maxBridgeLength = 8;
            minObstacleSpeed = 3;
            maxObstacleSpeed = 4;
        }
        if (scoreManager.puntuacionActual > 28)
        {
            minBridgeLength = 9;
            maxBridgeLength = 15;
            minObstacleSpeed = 4;
            maxObstacleSpeed = 5;
        }
        if (scoreManager.puntuacionActual > 34)
        {
            minBridgeLength = 15;
            maxBridgeLength = 25;
            minObstacleSpeed = 5;
            maxObstacleSpeed = 6;
        }
        if (scoreManager.puntuacionActual > 50)
        {
            minBridgeLength = 25;
            maxBridgeLength = 35;
            minObstacleSpeed = 6;
            maxObstacleSpeed = 7;
        }
        if (scoreManager.puntuacionActual > 70)
        {
            minBridgeLength = 35;
            maxBridgeLength = 40;
            minObstacleSpeed = 7;
            maxObstacleSpeed = 8;
        }
        CreateScene();
        cameraOnStart = false;
        playing = false;
    }

    //muestra el primer y ultimo obstaculo
    IEnumerator ShowStartEnd()
    {
        yield return new WaitForSeconds(.5f);
        targetPosition = bridgeEnd.transform.position;
        yield return new WaitForSeconds(bridgeLength  / 8);
        targetPosition = cameraStartPos;
        yield return new WaitForSeconds(bridgeLength / 5);
        playing = true;
        SpawnObstacle();
        canCreateObstacle = true;
        playing = true;
    }

    //restart game, reset score,...
    public void RestartGame()
    {
        if (uIManager.gameState == GameState.PAUSED)
            Time.timeScale = 1;

        minBridgeLength = 5;
        maxBridgeLength = 8;
        minObstacleSpeed = 1;
        maxObstacleSpeed = 2;   
        ClearScene();
        CreateScene();
        camObject.transform.position = new Vector3(cameraStartPos.x, 0, 0); //reset z and y of camera (move only of x axis)
        uIManager.ShowGameplay();
        scoreManager.ResetCurrentScore();
        cameraOnStart = true;
        playing = false;
        StartCoroutine(ShowStartEnd());


    }

    //clear all blocks from scene
    public void ClearScene()
    {

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject item in obstacles)
        {
            Destroy(item);
        }

        //clear obstacles list
        obstacleList.Clear();

        //reset camera position
        camObject.transform.position = new Vector3(cameraStartPos.x, 0, 0); //reset z and y of camera (move only of x axis)

    }

    //show game over gui
    public void GameOver()
    {
        if (uIManager.gameState == GameState.PLAYING)
        {
            playing = false;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
            uIManager.ShowGameOver();

            for (int i = 0; i < obstacleList.Count; i++)
            {
                obstacleList[i].GetComponent<Renderer>().materials[1].color = wrongColor;
            }

            scoreManager.UpdateScoreGameover();
        }

        minBridgeLength = 5;
        maxBridgeLength = 8;
        minObstacleSpeed = 1;
        maxObstacleSpeed = 2;
    }
}
