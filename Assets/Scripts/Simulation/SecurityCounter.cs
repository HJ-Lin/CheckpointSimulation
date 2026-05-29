using UnityEngine;
using System.Collections.Generic;

public class SecurityCounter : MonoBehaviour
{
    public int id;
    public bool active = true;
    public List<int> hookedLanes = new List<int>();
    public bool occupied { get; private set; }
    public TravelerAgent currentTraveler { get; private set; }
    public float processingEndTime;
    public float activeTime; // seconds counter has been active
    public float busyTime;   // seconds spent processing

    private SimulationController controller;
    private float processingDuration;

    void Start()
    {
        controller = SimulationController.Instance;
    }

    void Update()
    {
        if (controller.isPaused) return;

        if (occupied && controller.simulationTime >= processingEndTime)
        {
            // Finished processing current traveler
            controller.OnSecurityProcessingComplete(this);
            occupied = false;
            currentTraveler = null;
        }
        else if (!occupied && (active || IsDraining()))
        {
            // Try to pull traveler from queue
            controller.TryAssignTravelerToSecurityCounter(this);
        }

        // Track utilization
        if (active)
        {
            activeTime += Time.deltaTime * controller.simulationSpeed;
            if (occupied) busyTime += Time.deltaTime * controller.simulationSpeed;
        }
    }

    private bool IsDraining()
    {
        if (active) return false;
        // Check if any hooked lane has queued travelers that aren't served by active counters
        foreach (int laneIdx in hookedLanes)
        {
            if (controller.GetSecurityQueueLength(laneIdx) > 0 && !controller.IsSecurityLaneServed(laneIdx))
                return true;
        }
        return false;
    }

    public void AssignTraveler(TravelerAgent traveler)
    {
        occupied = true;
        currentTraveler = traveler;
        traveler.AssignedSecurityCounter = this;
        traveler.State = TravelerState.AtSecurityCounter;
        traveler.SecurityStartTime = controller.simulationTime;
        traveler.SetTarget(transform.position + Vector3.right * 0.5f);

        // Calculate processing time
        var config = controller.gameConfig;
        float mean = config.securityProcessingTime;
        float min = config.securityMinTime;
        float max = config.securityMaxTime;
        float baseTime = TriangularRandom(min, max, mean) * traveler.TypeMultiplier;

        if (Random.value < config.securityEnhancedProb)
        {
            traveler.Enhanced = true;
            baseTime += 45f; // extra screening time
        }

        processingDuration = baseTime;
        processingEndTime = controller.simulationTime + baseTime;
    }

    private float TriangularRandom(float min, float max, float mode)
    {
        float u = Random.value;
        float f = (mode - min) / (max - min);
        if (u < f) return min + Mathf.Sqrt(u * (max - min) * (mode - min));
        else return max - Mathf.Sqrt((1 - u) * (max - min) * (max - mode));
    }
}