using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armory : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    private void Awake()
    {
        if (itemPrefabs == null )
        {
            itemPrefabs = new GameObject[0];
        }

    }

    public GameObject GetPrefab(string name)
    {
        return itemPrefabs[GetPrefabIndex(name)];
    }

    private int GetPrefabIndex(string name)
    {
        if(name=="Bomb")
        {
            return 0;
        } else if(name=="Large Bomb")
        {
            return 1;
        } return -1;
    }
}
