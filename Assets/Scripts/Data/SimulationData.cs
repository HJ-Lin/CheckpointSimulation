using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TravelerType
{
    public string name;
    public float weight;
    public float speedMultiplier;
    public float processingTimeMultiplier;
}

[Serializable, CreateAssetMenu(fileName = "ScenarioPreset", menuName = "Simulation/Scenario Preset")]
public class ScenarioPreset : ScriptableObject
{
    public string id;
    public string displayName;
    public float arrivalRate; // travelers per minute
    public int securityLanes;
    public float securityProcessingTime; // seconds
    public float securityMinTime;
    public float securityMaxTime;
    public float securityEnhancedProb;
    public int immigrationCounters;
    public float immigrationProcessingTime;
    public float immigrationMinTime;
    public float immigrationMaxTime;
    public float automatedProcessingTime;
    public float automatedMinTime;
    public float automatedMaxTime;
    public float automatedErrorProb;
    public float enhancedScreeningProb;
    public int citizenPercentage;
    public float baseWalkingSpeed;
    public float[] timeMultipliers; // 8 slots: 0-3h, 3-6h, etc.
    public string[] immigrationLaneSettings; // "all", "citizens", "foreigners"
}

[Serializable]
public class TravelerLogEntry
{
    public int id;
    public string type;
    public bool isCitizen;
    public float arrivalTime;
    public float securityStartTime;
    public float securityEndTime;
    public float immigrationStartTime;
    public float immigrationEndTime;
    public float exitTime;
}

public class SimulationMetrics
{
    public int totalProcessed;
    public float totalWaitTime;
    public float totalSecurityWaitTime;
    public int securityProcessedCount;
    public float totalImmigrationWaitTime;
    public int immigrationProcessedCount;
    public int securityQueueLength;
    public int immigrationQueueLength;
    public float throughputPerHour;
    public float lastThroughputUpdate;
    public int processedLastHour;
    public float avgWalkingSpeed;
    public float measuredArrivalRate;
    public int peakSecurityQueue;
    public int peakImmigrationQueue;
    public float totalManHours;
    public int peakTotalProcessed;
    public float peakTotalWaitTime;
    public int peakSecurityProcessedCount;
    public float peakTotalSecurityWaitTime;
    public int peakImmigrationProcessedCount;
    public float peakTotalImmigrationWaitTime;
}