using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TravelerType
{
    public string name;
    public int weight;
    public float speedMultiplier;
    public float processingTimeMultiplier;
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