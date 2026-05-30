using UnityEngine;

/// <summary>
/// Interface for objects that can be pooled.
/// Implement this interface on any object that needs to be managed by ObjectPoolManager.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when the object is taken from the pool and activated.
    /// Initialize or reset the object to a ready state.
    /// </summary>
    void OnSpawnFromPool();

    /// <summary>
    /// Called when the object is returned to the pool and deactivated.
    /// Clean up any references, stop coroutines, and prepare for reuse.
    /// </summary>
    void OnReturnToPool();

    /// <summary>
    /// Get the GameObject of this poolable object.
    /// </summary>
    GameObject GetGameObject();
}
