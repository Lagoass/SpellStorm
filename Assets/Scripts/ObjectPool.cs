using System.Collections.Generic;
using UnityEngine;


// This is my Object Pool Script
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 10;
    private List<GameObject> pool;
    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = CreateNewObject();
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return CreateNewObject();
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, Vector2.zero, Quaternion.identity);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }
}
