using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	
	[SerializeField] private PlayerInputManager inputManager;
	[SerializeField] private ChestSpawner chestSpawner;
	[SerializeField] private PirateSpawner pirateSpawner;
	[SerializeField] private GameObject gameButtonsParent;
	[SerializeField] private GameObject pregameButtonsParent;
	[SerializeField] private GameObject infoPageParent;
	[SerializeField] private GameObject redSlider;
	[SerializeField] private GameObject blueSlider;

	private GameObject[] gameButtons;
	private Button[] items;
	private TextMeshProUGUI winTextBox;

	private Pirate currentPirateCheck;
	private int turnCounter;
	private bool gameStarted = false;
	private string winner;
	private TeamManager winningTeam;


	public void Start()
	{
		winner = "";
		winTextBox = pregameButtonsParent.transform.Find("WinnerText").GetComponentInChildren<TextMeshProUGUI>();
		gameButtons = GetGameButtons();
		items = GetItems();
		gameButtonsParent.SetActive(false);
		infoPageParent.SetActive(false);
	}

	public void Update()
	{
		if (!CheckCurrentPirate())
		{
			SetCurrentPirate();
			UpdatePirateButtons();
			UpdateItemButtonStats();
		}

		MoveButtonStatus(!inputManager.moveUsed);
		UpdateItemButtonStats();
		UpdateSliders();
		IsGameOver();
		GameState();
	}

	public bool InitLevel()
	{
		turnCounter = 0;
		winningTeam = null;
		pregameButtonsParent.SetActive(false);
		infoPageParent.SetActive(false);
		pirateSpawner.CreatePirates();
		inputManager.BuildInputManager();
		currentPirateCheck = null;
		gameStarted = true;
		inputManager.Startup();
		UpdatePirateButtons();
		winner = "";
		return gameStarted;
	}


	private GameObject[] GetGameButtons()
	{
		GameObject[] childObjectArray = new GameObject[gameButtonsParent.transform.childCount];
		for (int i = 0; i < gameButtonsParent.transform.childCount; i++)
		{
			childObjectArray[i] = gameButtonsParent.transform.GetChild(i).gameObject;
		}

		return childObjectArray;
	}

	private Button[] GetItems()
	{
		return new Button[] { gameButtons[2].GetComponent<Button>(), gameButtons[3].GetComponent<Button>(), gameButtons[4].GetComponent<Button>(), gameButtons[5].GetComponent<Button>() };
	}

	private void UpdateWinText()
	{
		if(winTextBox != null) 
		{
			winTextBox.text = winner;
			if(!winTextBox.text.Equals(""))
			{
				winTextBox.color = TextToColor();
			}
		}
	}

	private Color TextToColor()
	{
		if(winner != null)
		{
			return (winningTeam.GetPlayerNum() == 0) ? Color.blue : Color.red;
		} return Color.white;
	}
	public void printData()
	{
		Debug.Log(GetCurrentTeamManager().ToString());
	}

	private bool CheckCurrentPirate()
	{
		return currentPirateCheck == inputManager.GetCurrentPirateGlobal();
	}

	private void SetCurrentPirate()
	{
		currentPirateCheck = inputManager.GetCurrentPirateGlobal();
	}

	private bool UpdatePirateButtons()
	{
		return SetPirateButtons(inputManager.GetCurrentPirateGlobal() != null && inputManager.IsCurrentPirateOnCurrentTeam() && gameStarted && winningTeam==null);
	}

	private bool SetPirateButtons(bool b)
	{
		for (int i = 0; i < gameButtons.Length; i++)
		{
			if (gameButtons[i] != null && gameButtons[i].name.ToLower().Contains("button"))
			{
				gameButtons[i].GetComponent<Button>().enabled = b;
				gameButtons[i].GetComponent<Button>().interactable = b;
			}

		}
		gameButtonsParent.SetActive(b);
		return b;
	}

	private void UpdateItemButtonStats()
	{
		int currentIndex = 0;
		if (inputManager.GetCurrentPirateGlobal() != null)
		{
			foreach (var item in inputManager.GetCurrentPirateGlobal().GetItems())
			{
				if (items[currentIndex] != null && items[currentIndex].enabled)
				{
					items[currentIndex].GetComponentInChildren<TMP_Text>().text = item.Value.ToString();
				}
				currentIndex++;
			}

			if (currentIndex < items.Length - 1)
			{
				for (int i = currentIndex; i < items.Length; i++)
				{
					items[i].GetComponentInChildren<TMP_Text>().text = 0.ToString();
					items[i].enabled = false;
					items[i].interactable = false;
				}
			}
		}
	}

	private TeamManager GetCurrentTeamManager() { return inputManager.GetCurrentPlayer(); }

	private void MoveButtonStatus(bool b)
	{
		if (gameButtons != null)
		{
			gameButtons[0].GetComponent<Button>().enabled = b;
			gameButtons[0].GetComponent<Button>().interactable = b;
		}
	}

	public void OnMoveButtonClick()
	{
		inputManager.SetSelectionState(2);
	}

	public void OnSkipButtonClick()
	{
		NextTurn();
	}

	public void OnItemButtonClick(int i)
	{
		inputManager.SetSelectionState(3);
		inputManager.SetItem(GetNameFromInt(i));
	}


	private string GetNameFromInt(int i)
	{
		if(i == 1)
		{
			return "Bomb";
		} else if(i==2)
		{
			return "Large Bomb";

		} return "";
	}

	private void GameState()
	{
		if (inputManager.itemUsed)
		{
			NextTurn();
		}
	}
	private void NextTurn()
	{
		inputManager.NextTurn();
		turnCounter++;
		if(turnCounter%2 == 0 && Random.Range(0,100)>40)
		{
			Debug.Log("NEXT TURN!!");
			chestSpawner.SpawnChest();
		}
	}

	public void LoadInfoPage(bool b)
	{
		pregameButtonsParent.SetActive(!b);
		infoPageParent.SetActive(b);
	}


	public void QuitGame()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#endif
		Application.Quit();
	}

	public bool Restart()
	{
		pirateSpawner.DestroyAll();
		chestSpawner.DestroyAll();
		return InitLevel();
	}

	private bool IsGameOver()
	{
		bool gameOver = false;
		if (gameStarted)
		{
			foreach (TeamManager manager in inputManager.GetTeamManagers())
			{
				if (manager.GetTotalWorth() == 0)
				{
					winningTeam = (manager.GetPlayerNum() == 0) ? inputManager.GetTeamManager(1) : inputManager.GetTeamManager(0);
					winner = winningTeam.GetWinText();
					gameStarted = false;
					gameOver = true;
				}
			}

			if (gameOver)
			{
				gameButtonsParent.SetActive(false);
				pregameButtonsParent.SetActive(true);
				UpdateWinText();
			}
		}


		return gameOver;
	}

	private Slider GetSlider(TeamManager t)
	{
		if (t != null)
		{
			return ((t.GetPlayerTeam() == "Blue") ? blueSlider : redSlider).GetComponent<Slider>();
		}
		return null;
	}

	private void UpdateSliders()
	{
		if(gameButtonsParent.activeSelf)
		{
			for(int i = 0; i<inputManager.GetTeamManagers().Length; i++)
			{
				if (inputManager.GetTeamManagers()[i]!=null && GetSlider(inputManager.GetTeamManagers()[i])!=null)
				{
					GetSlider(inputManager.GetTeamManagers()[i]).value = inputManager.GetTeamManagers()[i].GetTotalWorth() / inputManager.GetTeamManagers()[i].GetMaxWorth();
				}
			}
		}
	}

	public void StartGame()
	{
		bool b =  (winner=="") ? InitLevel() : Restart();
	}
}
