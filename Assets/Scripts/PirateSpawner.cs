using System.Collections.Generic;
using UnityEngine;


public class PirateSpawner : MonoBehaviour
{
	[SerializeField] private int numPiratesA;
	[SerializeField] private int numPiratesB;

	[SerializeField] private GameObject Pirate_Blue_A_Prefab;
	[SerializeField] private GameObject Pirate_Blue_B_Prefab;
	[SerializeField] private GameObject Pirate_Red_A_Prefab;
	[SerializeField] private GameObject Pirate_Red_B_Prefab;

	private Dictionary<int, GameObject> prefabs;

	private List<GameObject> pirates = new List<GameObject>();
	public void Awake()
    {
		BuildAll();
	}

	public void BuildAll()
	{
		BuildPrefabDict();
		SetNumPirates();
	}

	private void BuildPrefabDict()
	{
		prefabs = prefabs ?? new Dictionary<int, GameObject>();
		prefabs.Add(1, Pirate_Blue_A_Prefab);
		prefabs.Add(2, Pirate_Blue_B_Prefab);
		prefabs.Add(-1, Pirate_Red_A_Prefab);
		prefabs.Add(-2, Pirate_Red_B_Prefab);
	}

	private int GetPrefabType(int player, int pirateType)
	{
		int num = (pirateType == 0) ? 1 : 2;
		int multiple = (player == 0) ? 1 : -1;
		return num * multiple;
	}

	private void ConditionalInstatiate(int player, int type, Vector3 pos)
	{
		pirates.Add(Instantiate(prefabs[GetPrefabType(player, type)], pos, gameObject.transform.rotation, transform));
	}

	private void SetNumPirates()
	{
		numPiratesA = ((numPiratesA < 0) ? 3 : numPiratesA);
		numPiratesB = ((numPiratesB < 0) ? 1 : numPiratesB);
	}

	public void CreatePirates()
	{
		CreatePiratesA();
		CreatePiratesB();
	}

	private void CreatePiratesA()
	{
		for (int i = 0; i < numPiratesA*2; i++)
		{
			Vector3 posVector = new Vector3(Random.Range(-30, 30), 2.0f, 0.0f);
			ConditionalInstatiate(i%2, 0, posVector);
		}
	}

	private void CreatePiratesB()
	{
		for (int i = 0; i < numPiratesB*2; i++)
		{
			Vector3 posVector = new Vector3(Random.Range(-30, 30), 2.0f, 0.0f);
			ConditionalInstatiate(i%2, 1, posVector);
		}
	}

	public void DestroyAll()
	{
		for(int i= 0; i < pirates.Count; i++)
		{
			if (pirates[i]!=null)
			{
				Destroy(pirates[i]);
			}
		}

		pirates.Clear();
	}
}
