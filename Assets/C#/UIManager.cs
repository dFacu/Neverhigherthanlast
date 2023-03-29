using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour {

	[Header("GUI Components")]
	public GameObject mainMenuGui;
	public GameObject pauseGui, gameplayGui, gameOverGui, helpGui;

	public GameState gameState;

	bool clicked;

	bool beginner;
	public ScoreManager finalScore;
	

	// Use this for initialization
	public void Start ()
	{
		mainMenuGui.SetActive(true);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(false);
        helpGui.SetActive(false);

        gameState = GameState.MENU;

        if (finalScore.puntuacionAlta == 0)
        {
            beginner = true;
        }
        else
        {
            beginner = false;
        }
        if (beginner == true)
        {
            ShowHelp();
        }
    }

    public void Update()
    {

    }
    //show main menu
    public void ShowMainMenu()
	{
		ScoreManager.Instance.ResetCurrentScore();
		mainMenuGui.SetActive(true);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(false);
        helpGui.SetActive(false);

        if (gameState == GameState.PAUSED)
			Time.timeScale = 1;

		gameState = GameState.MENU;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
		GameManager.Instance.ClearScene();
	}

	public void ShowHelp()
	{
        mainMenuGui.SetActive(false);
        pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
        gameOverGui.SetActive(false);
        helpGui.SetActive(true);
		beginner = false;
	}

    // Entras en  Pausa
    public void ShowPauseMenu()
	{
		if (gameState == GameState.PAUSED)
			return;

		pauseGui.SetActive(true);
		Time.timeScale = 0;
		gameState = GameState.PAUSED;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	// Salis del pausa
	public void HidePauseMenu()
	{
		pauseGui.SetActive(false);
		Time.timeScale = 1;
		gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	//show gameplay gui
	public void ShowGameplay()
	{
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(true);
		gameOverGui.SetActive(false);
        helpGui.SetActive(false);

        gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	//show game over gui
	public void ShowGameOver()
	{
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(true);
        helpGui.SetActive(false);

        gameState = GameState.GAMEOVER;
	}



	public void Salir()
	{
		Application.Quit();
	}

	public void SalirHelp()
	{
        mainMenuGui.SetActive(true);
        pauseGui.SetActive(false);
        gameplayGui.SetActive(false);
        gameOverGui.SetActive(false);
        helpGui.SetActive(false);
        gameState = GameState.MENU;

    }
    public bool IsButton()
    {
        bool temp = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult item in results)
        {
            temp |= item.gameObject.GetComponent<Button>() != null;
        }

        return temp;
    }
}
