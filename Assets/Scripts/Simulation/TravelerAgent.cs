using UnityEngine;
using System.Collections.Generic;

public enum TravelerState
{
    Arriving,
    InSecurityQueue,
    AtSecurityCounter,
    InImmigrationQueue,
    AtImmigrationCounter,
    Leaving
}

public class TravelerAgent : MonoBehaviour, IPoolable
{
    private int id;
    public int Id => id;
    private TravelerState state;
    public TravelerState State { get { return state; } set { state = value; } }
    private string type; // "standard", "business", "family"
    public string Type => type; // "standard", "business", "family"
    private bool isCitizen;
    public bool IsCitizen { get { return isCitizen; } set { isCitizen = value; } }
    private float speed;
    public float Speed => speed;
    private float entryTime;
    public float EntryTime => entryTime;
    private float securityQueueJoinTime;
    public float SecurityQueueJoinTime { get { return securityQueueJoinTime; } set { securityQueueJoinTime = value; } }
    private float immigrationQueueJoinTime;
    public float ImmigrationQueueJoinTime { get { return immigrationQueueJoinTime; } set { immigrationQueueJoinTime = value; } }
    private float securityStartTime;
    public float SecurityStartTime { get { return securityStartTime; } set { securityStartTime = value; } }
    private float securityEndTime;
    public float SecurityEndTime { get { return securityEndTime; } set { securityEndTime = value; } }
    private float immigrationStartTime;
    public float ImmigrationStartTime { get { return immigrationStartTime; } set { immigrationStartTime = value; } }
    private float immigrationEndTime;
    public float ImmigrationEndTime { get { return immigrationEndTime; } set { immigrationEndTime = value; } }
    private float exitTime;
    public float ExitTime { get { return exitTime; } set { exitTime = value; } }
    private int laneIndex = -1;
    public int LaneIndex { get { return laneIndex; } set { laneIndex = value; } }
    private int queuePosition = 0;
    private SecurityCounter assignedSecurityCounter;
    public SecurityCounter AssignedSecurityCounter { get { return assignedSecurityCounter; } set { assignedSecurityCounter = value; } }
    private ImmigrationCounter assignedImmigrationCounter;
    public ImmigrationCounter AssignedImmigrationCounter { get { return assignedImmigrationCounter; } set { assignedImmigrationCounter = value; } }
    private bool enhanced = false;
    public bool Enhanced { get { return enhanced; } set { enhanced = value; } }
    private bool automatedError = false;
    public bool AutomatedError { get { return automatedError; } set { automatedError = value; } }

    private Vector3 targetPosition;
    private float typeMultiplier;
    public float TypeMultiplier => typeMultiplier;
    private Color originalMaterialColor;

    public void Initialize(int travelerId, string travelerType, bool citizen, float arrivalTime)
    {
        id = travelerId;
        type = travelerType;
        isCitizen = citizen;
        entryTime = arrivalTime;
        state = TravelerState.Arriving;

        // Set speed based on type
        var config = SimulationController.Instance.gameConfig;
        float baseSpeed = config.baseWalkingSpeed;
        switch (type)
        {
            case "business": speed = baseSpeed * 1.2f; break;
            case "family": speed = baseSpeed * 0.8f; break;
            default: speed = baseSpeed; break;
        }
        // Apply random variation
        speed = TriangularRandom(speed * 0.8f, speed * 1.2f, speed);

        typeMultiplier = GetTypeMultiplier(type);
        targetPosition = transform.position;

        ApplyTypeColor(type);
    }

    private void ApplyTypeColor(string travelerType)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            if (originalMaterialColor == Color.clear)
            {
                originalMaterialColor = renderer.material.color;
            }

            Color newColor = Color.white;
            switch (travelerType)
            {
                case "business": newColor = Color.green; break;
                case "family": newColor = Color.red; break;
                default: newColor = Color.yellow; break;
            }

            renderer.material.color = newColor;
        }
    }

    private float GetTypeMultiplier(string travelerType)
    {
        var types = SimulationController.Instance.gameConfig.travelerTypes;
        if (travelerType == "business") return types["business"].processingTimeMultiplier;
        if (travelerType == "family") return types["family"].processingTimeMultiplier;
        return 1.0f;
    }

    void Update()
    {
        if (SimulationController.Instance.isPaused) return;

        float step = speed * Time.deltaTime * SimulationController.Instance.simulationSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            OnReachTarget();
        }
    }

    public void SetTarget(Vector3 pos)
    {
        targetPosition = pos;
    }

    private void OnReachTarget()
    {
        switch (state)
        {
            case TravelerState.Arriving:
                SimulationController.Instance.OnTravelerArrivedAtSecurity(this);
                break;
            case TravelerState.InSecurityQueue:
                // Already in queue, waiting for counter
                break;
            case TravelerState.AtSecurityCounter:
                // Being processed
                break;
            case TravelerState.InImmigrationQueue:
                break;
            case TravelerState.AtImmigrationCounter:
                break;
            case TravelerState.Leaving:
                SimulationController.Instance.OnTravelerExited(this);
                break;
        }
    }

    private float TriangularRandom(float min, float max, float mode)
    {
        float u = Random.value;
        float f = (mode - min) / (max - min);
        if (u < f)
            return min + Mathf.Sqrt(u * (max - min) * (mode - min));
        else
            return max - Mathf.Sqrt((1 - u) * (max - min) * (max - mode));
    }

    public void SetSecurityQueuePosition(int pos)
    {
        queuePosition = pos;
        // Update target to queue waypoint
        var lane = SimulationController.Instance.GetSecurityLane(laneIndex);
        if (lane != null)
        {
            targetPosition = lane.GetQueuePosition(pos);
        }
    }

    public void SetImmigrationQueuePosition(int pos)
    {
        queuePosition = pos;
        // Update target to queue waypoint
        var lane = SimulationController.Instance.GetImmigrationLane(laneIndex);
        if (lane != null)
        {
            targetPosition = lane.GetQueuePosition(pos);
        }
    }

    /// <summary>
    /// Called when the traveler is spawned from the object pool.
    /// </summary>
    public void OnSpawnFromPool()
    {
        // Object is now active, will be initialized via Initialize() method
    }

    /// <summary>
    /// Called when the traveler is returned to the object pool.
    /// Resets all state for reuse.
    /// </summary>
    public void OnReturnToPool()
    {
        // Reset all traveler data
        id = 0;
        type = "standard";
        isCitizen = false;
        speed = 0;
        entryTime = 0;
        securityQueueJoinTime = 0;
        immigrationQueueJoinTime = 0;
        securityStartTime = 0;
        securityEndTime = 0;
        immigrationStartTime = 0;
        immigrationEndTime = 0;
        exitTime = 0;
        laneIndex = -1;
        queuePosition = 0;
        assignedSecurityCounter = null;
        assignedImmigrationCounter = null;
        enhanced = false;
        automatedError = false;
        targetPosition = Vector3.zero;
        typeMultiplier = 1.0f;
        state = TravelerState.Arriving;

        RevertMaterialColor();
    }

    private void RevertMaterialColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null && originalMaterialColor != Color.clear)
        {
            renderer.material.color = originalMaterialColor;
            originalMaterialColor = Color.clear;
        }
    }

    /// <summary>
    /// Get the GameObject of this poolable object.
    /// </summary>
    public GameObject GetGameObject()
    {
        return gameObject;
    }
}