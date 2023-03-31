using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; set; }
    public TextMeshProUGUI currentScore, highScore, actualGameOver, juegoPuntuaciónAlta;

    public int CurrentScore, HighScore;
    // Start is called before the first frame update

    bool counting;

    Vector3 pos;

    

    [SerializeField]private TextMeshProUGUI addPoints;
    public GameObject container;
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    //init and load highscore
    void Start()
    {
        if (!PlayerPrefs.HasKey("Puntuación más alta"))
            PlayerPrefs.SetInt("Puntuación más alta", 0);

        HighScore = PlayerPrefs.GetInt("Puntuación más alta");
        pos = container.transform.position;


        UpdateHighScore();
        ResetCurrentScore();
        container.SetActive(false);

    }

    //save and update highscore
    void UpdateHighScore()
    {
        if (CurrentScore > HighScore)
            HighScore = CurrentScore;

        highScore.text = HighScore.ToString();
        PlayerPrefs.SetInt("Puntuación más alta", HighScore);
    }

    //update currentscore
    public void UpdateScore(int value)
    {
        CurrentScore += value;
        currentScore.text = CurrentScore.ToString();
        addPoints.text = "+" + value.ToString();

        StartCoroutine(sp());
    }

    //reset current score
    public void ResetCurrentScore()
    {
        CurrentScore = 0;
        UpdateScore(0);
    }

    //update gameover scores
    public void UpdateScoreGameover()
    {
        UpdateHighScore();

        actualGameOver.text = CurrentScore.ToString();
        juegoPuntuaciónAlta.text = HighScore.ToString();
    }

    IEnumerator sp()
    {
        if (container.transform.position.y >= 700f)
        {
            container.transform.position = pos;
        }
        
        if(CurrentScore == 0)
        {
            container.SetActive(false);
        }
        else
        {
            container.SetActive(true);

        }

        yield return new WaitForSeconds(0.5f);
        container.SetActive(false);
        container.transform.position = pos;
    }

    private void Update()
    {

        container.transform.Translate(Vector3.up * 40f * Time.deltaTime);
    }
}
