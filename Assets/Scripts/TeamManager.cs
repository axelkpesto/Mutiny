using System.Collections.Generic;
using UnityEngine;

public class TeamManager
{
	//INSTATIATION
    private int playerNum = 0;
	private int pirateIndex = 0;
	private Pirate[] pirates;
	private List<Pirate> livingPirates;
	private float totalWorth;
	private float maxWorth;

	//Constructor
	public TeamManager( int playerNum ) {
        this.playerNum = playerNum%2;
		GetPiratesByTag();

		totalWorth = CalculateTotalWorth();
		maxWorth = CalculateTotalWorth();
	}
	private void GetPiratesByTag()
	{
		GameObject[] piratesGO = GameObject.FindGameObjectsWithTag($"Pirate_{GetPlayerTeam()}");
		this.pirates = GameObjectToPirate(piratesGO);
		this.livingPirates = new List<Pirate>(pirates);
	}

	private Pirate[] GameObjectToPirate(GameObject[] GameObjectArray)
	{
		Pirate[] pirateArray = new Pirate[GameObjectArray.Length];
		for (int i = 0; i < GameObjectArray.Length; i++)
		{
			pirateArray[i] = (Pirate)GameObjectArray[i].GetComponent(typeof(Pirate));
		}
		return pirateArray;
	}

	//Pirate Selection
	public void NextPirate()
	{
		GetCurrentPirate().ResetState();
		pirateIndex = IncrementPirate();
	}

	public void PrevPirate()
	{
		GetCurrentPirate().ResetState();
		pirateIndex = DecrementPirate();
	}

	private int IncrementPirate()
	{
		if (livingPirates.Count == 0) { return 0; }
		return (pirateIndex+1)%(livingPirates.Count);
	}

	private int DecrementPirate()
	{
		if(livingPirates.Count==0) {  return 0; }
		return ((pirateIndex >= 1) ? pirateIndex-1 : IncrementPirate());
	}

	public int SetPirateIndex(int newIndex)
	{
		if(livingPirates.Count==0) { pirateIndex = 0; }
		pirateIndex = (newIndex) % (livingPirates.Count);
		return pirateIndex;
	}

	public int GetPirateByIndex(Pirate p)
	{
		return livingPirates.IndexOf(p);
	}

	public Pirate SetCurrentPirate(Pirate p)
	{
		if (GetCurrentPirate() != null) { GetCurrentPirate().ResetState(); };
		int index = GetPirateByIndex(p);
		pirateIndex = ((index >= 0) ? index : pirateIndex);
		return GetCurrentPirate();
	}

	//Pirate Living Status
	public void UpdatePirateStatus()
	{
		for(int i=0; i<livingPirates.Count; i++)
		{
			if (!livingPirates[i].IsAlive())
			{
				livingPirates.RemoveAt(i);
				DecrementPirate();
			}
		}

		totalWorth = CalculateTotalWorth();
	}

	//Total 'Worth' of Pirates
	private float CalculateTotalWorth()
	{
		float currentWorth = 0f;
		for (int i = 0; i < livingPirates.Count; i++)
		{
			currentWorth += livingPirates[i].GetWorth();
		} 
		
		return currentWorth;
	}

	//Getters and ToStrings
	override
	public string ToString()
	{
		return ($"PlayerNum: {playerNum}, PlayerTeam: {GetPlayerTeam()}, PirateIndex: {pirateIndex}, PirateCount: {livingPirates.Count} + \n \nPirate Strings: \n{PirateString()}");
	}

	public string SimpleString()
	{
		return ($"PlayerNum: {playerNum}, PirateIndex: {pirateIndex}");
	}

	public string PirateString()
	{
		string retString = "";
		for (int i = 0; i<pirates.Length; i++)
		{
			retString += pirates[i].ToString() + "\n";
		} return retString;
	}

	public string GetPlayerTeam() { return GetPlayerTeam(this.playerNum); }
	private string GetPlayerTeam(int playerNumber) { return ((playerNumber % 2 == 0) ? "Blue" : "Red"); }
	public bool PirateIsCurrentPirate(Pirate p) { return (p == GetCurrentPirate()); }
	public Pirate GetCurrentPirate() { return ((pirateIndex>=0 && livingPirates.Count>0 && pirateIndex<=livingPirates.Count-1) ? livingPirates[pirateIndex] : null); }
	public Pirate[] GetPirates() { return pirates; }
	public List<Pirate> GetLivingPirates() { return livingPirates; }
	public int GetNumPirates() { return pirates.Length; }
	public int GetNumLivingPirates() { return livingPirates.Count; }
	public float GetTotalWorth() { return totalWorth; }
	public int GetPlayerNum() {  return playerNum; }
	public float GetMaxWorth() { return maxWorth; }
	public string GetWinText() { return GetPlayerTeam() + "Player Wins!"; }
}
