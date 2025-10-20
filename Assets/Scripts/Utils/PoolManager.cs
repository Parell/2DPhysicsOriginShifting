using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Decel
{
    public static class PoolManager
    {
        public static Dictionary<GameObject, Pool> poolByPrefab = new Dictionary<GameObject, Pool>();
        public static Dictionary<GameObject, Pool> poolByGameObject = new Dictionary<GameObject, Pool>();

        public static GameObject Allocate(GameObject parent, GameObject prefab, Vector3 position, Quaternion rotation, bool persistent = false)
        {
            if (!poolByPrefab.ContainsKey(prefab))
            {
                poolByPrefab.Add(prefab, new Pool(parent, prefab, 5, persistent));
            }

            return poolByPrefab[prefab].Allocate(position, rotation);
        }

        public static GameObject Allocate(GameObject parent, GameObject prefab)
        {
            return Allocate(parent, prefab, Vector3.zero, Quaternion.identity);
        }

        public static void Deallocate(GameObject obj)
        {
            if (!poolByGameObject.ContainsKey(obj))
            {
                Object.Destroy(obj);
                return;
            }
            poolByGameObject[obj].Deallocate(obj);
        }

        public static void ClearAllPools()
        {
            List<GameObject> keysToRemove = new List<GameObject>();

            foreach (var pair in poolByPrefab)
            {
                Pool pool = pair.Value;

                if (!pool.isPersistent)
                {
                    foreach (var gameObject in pool.enabledGameObjects)
                    {
                        Object.Destroy(gameObject);
                    }

                    while (pool.disabledGameObjects.TryDequeue(out var gameObject))
                    {
                        Object.Destroy(gameObject);
                    }

                    pool.enabledGameObjects.Clear();
                    pool.disabledGameObjects = new ConcurrentQueue<GameObject>();
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                poolByPrefab.Remove(key);
                poolByGameObject.Remove(key);
            }
        }

        public class Pool
        {
            public int initalSize;
            public GameObject parent;
            public GameObject pooledPrefab;
            public List<GameObject> enabledGameObjects = new List<GameObject>();
            public ConcurrentQueue<GameObject> disabledGameObjects = new ConcurrentQueue<GameObject>();
            public bool isPersistent;

            public Pool(GameObject poolParent, GameObject pooledPrefab, int initalSize, bool persistent = false)
            {
                this.pooledPrefab = pooledPrefab;
                this.initalSize = initalSize;
                isPersistent = persistent;

                if (!poolParent) { parent = new GameObject(this.pooledPrefab.name); }
                else { parent = poolParent; }

                for (int i = 0; i <= this.initalSize; i++)
                {
                    var gameObject = Object.Instantiate(this.pooledPrefab);
                    disabledGameObjects.Enqueue(gameObject);
                    poolByGameObject.Add(gameObject, this);
                    gameObject.transform.SetParent(parent.transform);
                    gameObject.SetActive(false);
                }
            }

            public void Deallocate(GameObject gameObject)
            {
                gameObject.SendMessage("OnDeallocate", SendMessageOptions.DontRequireReceiver);
                gameObject.SetActive(false);
                enabledGameObjects.Remove(gameObject);
                disabledGameObjects.Enqueue(gameObject);
            }

            public GameObject Allocate(Vector3 position, Quaternion rotation)
            {
                GameObject gameObject;

                if (!disabledGameObjects.TryDequeue(out gameObject)) { gameObject = ExtendPool(); }

                gameObject.transform.SetPositionAndRotation(position, rotation);
                gameObject.SetActive(true);
                enabledGameObjects.Add(gameObject);
                gameObject.SendMessage("OnAllocate", SendMessageOptions.DontRequireReceiver);
                return gameObject;
            }

            private GameObject ExtendPool()
            {
                GameObject gameObject = Object.Instantiate(pooledPrefab);
                enabledGameObjects.Add(gameObject);
                poolByGameObject.Add(gameObject, this);
                gameObject.transform.SetParent(parent.transform);
                return gameObject;
            }
        }

        public interface IPoolable
        {
            public abstract void OnAllocate();
            public abstract void OnDeallocate();
        }
    }
}