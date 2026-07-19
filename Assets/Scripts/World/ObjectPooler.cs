using System.Collections.Generic;
using UnityEngine;

namespace EndlessRunner.World
{
    /// <summary>
    /// A robust, developer-friendly dynamic Object Pooling system.
    /// It automatically spawns and manages pools based on Prefab Instance IDs, avoiding allocations.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        private readonly Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Keep the pooler alive across scene loads if needed
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Retrieves an object of the specified prefab from its pool, or creates one if empty.
        /// </summary>
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();

            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary.Add(key, new Queue<GameObject>());
            }

            GameObject obj = null;
            Queue<GameObject> queue = poolDictionary[key];

            while (queue.Count > 0)
            {
                obj = queue.Dequeue();
                if (obj != null) break; // Ensure we didn't get a destroyed reference
            }

            if (obj == null)
            {
                obj = Instantiate(prefab);
                var poolable = obj.AddComponent<PoolableObject>();
                poolable.PrefabKey = key;
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Returns an object to its pool and deactivates it.
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            var poolable = obj.GetComponent<PoolableObject>();
            if (poolable != null)
            {
                obj.SetActive(false);
                int key = poolable.PrefabKey;
                if (poolDictionary.TryGetValue(key, out Queue<GameObject> queue))
                {
                    if (!queue.Contains(obj))
                    {
                        queue.Enqueue(obj);
                    }
                }
            }
            else
            {
                // Fallback: If it's not a pooled object, destroy it to clean up memory
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// Helper component attached to pooled objects to identify their parent pool.
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        public int PrefabKey { get; set; }
    }
}
