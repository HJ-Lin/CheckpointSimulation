using UnityEngine;
using System.Collections.Generic;

public class LaneWaypointHolder : MonoBehaviour
{
    [Header("Lane Geometry")]
    public int laneIndex;
    public Transform laneStart;        // The point where travelers first enter this lane
    public Vector3 queueDirection = Vector3.back; // Direction towards the counter (e.g., -Z)
    public float spacing = 1.2f;       // Distance between queue positions

    // Cache – no need to store actual Transforms
    private List<Vector3> cachedPositions = new List<Vector3>();
    public List<Vector3> CachedPositions => cachedPositions;

    /// <summary> Returns the world position for a given queue index (0 = front). </summary>
    public Vector3 GetQueuePosition(int index)
    {
        // Expand cache if needed
        while (cachedPositions.Count <= index)
        {
            Vector3 newPos = laneStart.position + queueDirection * (cachedPositions.Count * spacing);
            newPos.y = 0.17f;
            cachedPositions.Add(newPos);
        }
        return cachedPositions[index];
    }

    // Optional: visually debug waypoints in Editor
    private void OnDrawGizmosSelected()
    {
        if (laneStart == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++) // preview up to 20
        {
            Vector3 pos = laneStart.position + queueDirection * (i * spacing);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
    }
}