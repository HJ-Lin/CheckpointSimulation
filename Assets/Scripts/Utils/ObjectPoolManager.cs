using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A scalable, generic object pool manager that can be used for any poolable objects.
/// Uses a Stack for efficient allocation and deallocation.
/// Can manage multiple pools of different object types.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    // Dictionary to hold multiple pools for different object types
    private Dictionary<string, Stack<IPoolable>> pools = new Dictionary<string, Stack<IPoolable>>();
    private Dictionary<string, GameObject> pooledPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool logPoolActivity = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize a new object pool.
    /// </summary>
    /// <param name="poolKey">Unique identifier for this pool</param>
    /// <param name="prefab">The prefab to instantiate for pooling</param>
    /// <param name="initialSize">Initial number of objects to create</param>
    /// <param name="parent">Transform to parent pooled objects under</param>
    public void InitializePool(string poolKey, GameObject prefab, int initialSize = 0, Transform parent = null)
    {
        if (pools.ContainsKey(poolKey))
        {
            Debug.LogWarning($"Pool '{poolKey}' already exists. Skipping initialization.");
            return;
        }

        if (initialSize <= 0)
            initialSize = initialPoolSize;

        pools[poolKey] = new Stack<IPoolable>(initialSize);
        pooledPrefabs[poolKey] = prefab;

        // Create a container for this pool's objects
        Transform container;
        if (parent != null)
        {
            container = parent;
        }
        else
        {
            GameObject containerObj = new GameObject($"Pool_{poolKey}");
            containerObj.transform.SetParent(transform);
            container = containerObj.transform;
        }
        poolContainers[poolKey] = container;

        // Pre-populate the pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateAndReturnObject(poolKey);
        }

        if (logPoolActivity)
            Debug.Log($"Initialized pool '{poolKey}' with {initialSize} objects.");
    }

    /// <summary>
    /// Get an object from the pool. If none available, creates a new one.
    /// </summary>
    /// <param name="poolKey">The pool identifier</param>
    /// <returns>An IPoolable object ready to use</returns>
    public IPoolable GetFromPool(string poolKey)
    {
        if (!pools.ContainsKey(poolKey))
        {
            Debug.LogError($"Pool '{poolKey}' does not exist. Call InitializePool first.");
            return null;
        }

        IPoolable poolable;

        if (pools[poolKey].Count > 0)
        {
            poolable = pools[poolKey].Pop();
        }
        else
        {
            // Create a new object if pool is empty
            poolable = CreateNewPoolObject(poolKey);
            if (logPoolActivity)
                Debug.Log($"Pool '{poolKey}' was empty. Created new object.");
        }

        GameObject obj = poolable.GetGameObject();
        obj.SetActive(true);
        poolable.OnSpawnFromPool();

        if (logPoolActivity)
            Debug.Log($"Retrieved object from pool '{poolKey}'. Pool count: {pools[poolKey].Count}");

        return poolable;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    /// <param name="poolKey">The pool identifier</param>
    /// <param name="poolable">The object to return</param>
    public void ReturnToPool(string poolKey, IPoolable poolable)
    {
        if (!pools.ContainsKey(poolKey))
        {
            Debug.LogError($"Pool '{poolKey}' does not exist.");
            return;
        }

        if (poolable == null)
        {
            Debug.LogError("Cannot return null object to pool.");
            return;
        }

        GameObject obj = poolable.GetGameObject();
        poolable.OnReturnToPool();
        obj.SetActive(false);

        pools[poolKey].Push(poolable);

        if (logPoolActivity)
            Debug.Log($"Returned object to pool '{poolKey}'. Pool count: {pools[poolKey].Count}");
    }

    /// <summary>
    /// Get the current count of available objects in a pool.
    /// </summary>
    public int GetPoolCount(string poolKey)
    {
        if (!pools.ContainsKey(poolKey))
            return 0;
        return pools[poolKey].Count;
    }

    /// <summary>
    /// Clear all objects from a specific pool and destroy them.
    /// </summary>
    public void ClearPool(string poolKey)
    {
        if (!pools.ContainsKey(poolKey))
            return;

        while (pools[poolKey].Count > 0)
        {
            IPoolable poolable = pools[poolKey].Pop();
            Destroy(poolable.GetGameObject());
        }

        if (logPoolActivity)
            Debug.Log($"Cleared pool '{poolKey}'.");
    }

    /// <summary>
    /// Clear all pools and destroy all pooled objects.
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var key in new List<string>(pools.Keys))
        {
            ClearPool(key);
        }
        pools.Clear();
        pooledPrefabs.Clear();

        foreach (var container in poolContainers.Values)
        {
            Destroy(container.gameObject);
        }
        poolContainers.Clear();

        if (logPoolActivity)
            Debug.Log("Cleared all pools.");
    }

    // Private helper methods
    private IPoolable CreateNewPoolObject(string poolKey)
    {
        GameObject prefab = pooledPrefabs[poolKey];
        Transform container = poolContainers[poolKey];

        GameObject obj = Instantiate(prefab, container);
        IPoolable poolable = obj.GetComponent<IPoolable>();

        if (poolable == null)
        {
            Debug.LogError($"Prefab for pool '{poolKey}' does not have a component implementing IPoolable.");
            Destroy(obj);
            return null;
        }

        obj.SetActive(false);
        return poolable;
    }

    private void CreateAndReturnObject(string poolKey)
    {
        IPoolable poolable = CreateNewPoolObject(poolKey);
        if (poolable != null)
        {
            pools[poolKey].Push(poolable);
        }
    }
}
