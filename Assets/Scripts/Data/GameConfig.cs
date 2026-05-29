using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Simulation/Game Config")]
public class GameConfig : ScriptableObject
{
    public float arrivalRate = 4f;
    public string arrivalDistribution = "poisson"; // "poisson" or "fixed"
    public int securityLanes = 3;
    public float securityProcessingTime = 60f;
    public float securityMinTime = 30f;
    public float securityMaxTime = 90f;
    public float securityEnhancedProb = 0.05f;
    public int immigrationCounters = 4;
    public float immigrationProcessingTime = 90f;
    public float immigrationMinTime = 45f;
    public float immigrationMaxTime = 135f;
    public float automatedProcessingTime = 60f;
    public float automatedMinTime = 30f;
    public float automatedMaxTime = 90f;
    public float automatedErrorProb = 0.05f;
    public float enhancedScreeningProb = 0.1f;
    public int citizenPercentage = 90;
    public float baseWalkingSpeed = 80f;
    public float[] timeMultipliers = new float[] { 0.2f, 0.9f, 2.0f, 1.7f, 1.4f, 1.6f, 2.0f, 1.0f };
    public string[] immigrationLaneSettings = new string[] { "all", "all", "all", "all" };
    public Dictionary<string, TravelerType> travelerTypes = new Dictionary<string, TravelerType>();

    public void InitializeDefaults()
    {
        travelerTypes["standard"] = new TravelerType { name = "standard", weight = 60, speedMultiplier = 1f, processingTimeMultiplier = 1f };
        travelerTypes["business"] = new TravelerType { name = "business", weight = 20, speedMultiplier = 1.2f, processingTimeMultiplier = 0.7f };
        travelerTypes["family"] = new TravelerType { name = "family", weight = 20, speedMultiplier = 0.8f, processingTimeMultiplier = 1.6f };
    }
}