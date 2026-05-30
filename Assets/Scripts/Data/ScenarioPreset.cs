using System;
using UnityEngine;

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
