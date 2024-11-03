using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool instance;
        [System.Serializable]
        public class Pool
        {
            public string name;
            public GameObject prefab;
            public int size;
            public Transform parent;
        }
        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;
        private void Awake()
        {
            instance = this;

            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, pool.parent);
                    obj.gameObject.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(pool.name, objectPool);
            }
        }
        public GameObject GetObjet(string name)
        {
            GameObject X = null;
            if (poolDictionary.ContainsKey(name))
            {
                int count = poolDictionary[name].Count;
                for (int i = 0; i < count; i++)
                {
                    X = poolDictionary[name].Dequeue();
                    poolDictionary[name].Enqueue(X);
                    if (!X.activeSelf)
                    {
                        X.SetActive(true);
                        return X;
                    }
                }
            }
            if (X != null)
            {
                GameObject obj = Instantiate(X, X.transform.parent);
                poolDictionary[name].Enqueue(obj);
                return obj;
            }
            return null;
        }
        public void ResetObject(string name)
        {
            int cnt = poolDictionary[name].Count;
            for (int i = 0; i < cnt; i++)
            {
                GameObject gameObject = poolDictionary[name].Dequeue();
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
                poolDictionary[name].Enqueue(gameObject);
            }
        }
    }