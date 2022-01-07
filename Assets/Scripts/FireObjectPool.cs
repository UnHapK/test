using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireObjectPool : MonoBehaviour
{
    public static FireObjectPool Instance;
    [SerializeField] private GameObject poolingObjectPrefab;
    Queue<FireCS> poolingObjectQueue = new Queue<FireCS>();

    private void Awake()
    {
        Instance = this;
        Initialize(10);
    }

    private void Initialize(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            poolingObjectQueue.Enqueue(CreateNewObject());
        }
    }

    private FireCS CreateNewObject()
    {
        var newObj = Instantiate(poolingObjectPrefab).GetComponent<FireCS>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public static FireCS GetObject()
    {
        if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObject();
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    public static void ReturnObject(FireCS obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(obj);
    }
}
