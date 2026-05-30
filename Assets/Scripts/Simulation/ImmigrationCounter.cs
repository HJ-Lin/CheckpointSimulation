using UnityEngine;
using System.Collections.Generic;

public enum CounterType { Manned, Automated }

public class ImmigrationCounter : MonoBehaviour
{
    public int id;
    public CounterType type = CounterType.Manned;
    public bool active = true;
    public List<int> hookedLanes = new List<int>();
    public bool occupied { get; private set; }
    public TravelerAgent currentTraveler { get; private set; }
    public float processingEndTime;
    public float activeTime;
    public float busyTime;

    private SimulationController controller;

    void Start()
    {
        controller = SimulationController.Instance;
    }

    void Update()
    {
        if (controller.isPaused) return;

        if (occupied && controller.simulationTime >= processingEndTime)
        {
            if (type == CounterType.Automated && Random.value < controller.gameConfig.automatedErrorProb)
            {
                // Error - reprocess
                currentTraveler.AutomatedError = true;
                var config = controller.gameConfig;
                float mean = config.automatedProcessingTime;
                float min = config.automatedMinTime;
                float max = config.automatedMaxTime;
                float extraTime = TriangularRandom(min, max, mean) * currentTraveler.TypeMultiplier;
                processingEndTime = controller.simulationTime + extraTime;
                return;
            }

            controller.OnImmigrationProcessingComplete(this);
            occupied = false;
            currentTraveler = null;
        }
        else if (!occupied && (active || IsDraining()))
        {
            controller.TryAssignTravelerToImmigrationCounter(this);
        }

        if (active)
        {
            activeTime += Time.deltaTime * controller.simulationSpeed;
            if (occupied) busyTime += Time.deltaTime * controller.simulationSpeed;
        }
    }

    private bool IsDraining()
    {
        if (active) return false;
        foreach (int laneIdx in hookedLanes)
        {
            if (controller.GetImmigrationQueueLength(laneIdx) > 0 && !controller.IsImmigrationLaneServed(laneIdx))
                return true;
        }
        return false;
    }

    public void AssignTraveler(TravelerAgent traveler)
    {
        occupied = true;
        currentTraveler = traveler;
        traveler.AssignedImmigrationCounter = this;
        traveler.State = TravelerState.AtImmigrationCounter;
        traveler.ImmigrationStartTime = controller.simulationTime;
        Vector3 newPos = transform.position + Vector3.right * 0.5f;
        newPos.y = 0.17f;
        traveler.SetTarget(newPos);

        var config = controller.gameConfig;
        float processingTime;

        if (type == CounterType.Automated)
        {
            float mean = config.automatedProcessingTime;
            float min = config.automatedMinTime;
            float max = config.automatedMaxTime;
            processingTime = TriangularRandom(min, max, mean) * traveler.TypeMultiplier;
        }
        else
        {
            float mean = config.immigrationProcessingTime;
            float min = config.immigrationMinTime;
            float max = config.immigrationMaxTime;
            processingTime = TriangularRandom(min, max, mean) * traveler.TypeMultiplier;

            if (Random.value < config.enhancedScreeningProb)
            {
                traveler.Enhanced = true;
                processingTime += 60f;
            }
        }

        processingEndTime = controller.simulationTime + processingTime;
    }

    public bool CanServeTraveler(TravelerAgent traveler)
    {
        // Check lane assignment restrictions
        foreach (int laneIdx in hookedLanes)
        {
            string setting = controller.gameConfig.immigrationLaneSettings[laneIdx];
            if (setting == "all") return true;
            if (setting == "citizens" && traveler.IsCitizen) return true;
            if (setting == "foreigners" && !traveler.IsCitizen) return true;
        }
        // Also check if this counter is the closest for any lane
        return false;
    }

    private float TriangularRandom(float min, float max, float mode)
    {
        float u = Random.value;
        float f = (mode - min) / (max - min);
        if (u < f) return min + Mathf.Sqrt(u * (max - min) * (mode - min));
        else return max - Mathf.Sqrt((1 - u) * (max - min) * (max - mode));
    }
}