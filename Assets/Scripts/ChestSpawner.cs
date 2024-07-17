using System.Collections.Generic;
using UnityEngine;


public class ChestSpawner : MonoBehaviour
{

	[SerializeField] private GameObject chestPrefab;
	private List<GameObject> chests = new List<GameObject>();


	public void Awake()
	{
		if(chestPrefab == null)
		{
			chestPrefab = (GameObject) Resources.Load("");
		}
	}

	public void SpawnChest()
	{
		chests.Add(Instantiate(chestPrefab, GetNewStartPos(), Quaternion.identity));	
	}

	public Vector3 GetNewStartPos()
	{
		return new Vector3(Random.Range(-50, 05), 20, 0);
	}

	public void DestroyAll()
	{
		for (int i = 0; i < chests.Count; i++)
		{
			if (chests[i] != null)
			{
				Destroy(chests[i]);
			}
		}

		chests.Clear();
	}
}
