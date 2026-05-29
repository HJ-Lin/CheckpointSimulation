using UnityEngine;
using System.Collections.Generic;

public static class ScenarioPresetsLoader
{
    public static List<ScenarioPreset> GetPresets()
    {
        return new List<ScenarioPreset>
        {
            new ScenarioPreset
            {
                id = "baseline",
                displayName = "Baseline",
                arrivalRate = 4f,
                securityLanes = 3,
                securityProcessingTime = 60f,
                securityMinTime = 30f,
                securityMaxTime = 90f,
                securityEnhancedProb = 0.05f,
                immigrationCounters = 4,
                immigrationProcessingTime = 90f,
                immigrationMinTime = 45f,
                immigrationMaxTime = 135f,
                automatedProcessingTime = 60f,
                automatedMinTime = 30f,
                automatedMaxTime = 90f,
                automatedErrorProb = 0.05f,
                enhancedScreeningProb = 0.1f,
                citizenPercentage = 90,
                baseWalkingSpeed = 80f,
                timeMultipliers = new float[] { 0.2f, 0.9f, 2.0f, 1.7f, 1.4f, 1.6f, 2.0f, 1.0f },
                immigrationLaneSettings = new string[] { "all", "all", "all", "all" }
            },
            new ScenarioPreset
            {
                id = "holiday",
                displayName = "Holiday Rush",
                arrivalRate = 8f,
                securityLanes = 3,
                securityProcessingTime = 60f,
                securityMinTime = 30f,
                securityMaxTime = 100f,
                securityEnhancedProb = 0.1f,
                immigrationCounters = 4,
                immigrationProcessingTime = 90f,
                immigrationMinTime = 45f,
                immigrationMaxTime = 150f,
                automatedProcessingTime = 75f,
                automatedMinTime = 40f,
                automatedMaxTime = 110f,
                automatedErrorProb = 0.1f,
                enhancedScreeningProb = 0.15f,
                citizenPercentage = 40,
                baseWalkingSpeed = 70f,
                timeMultipliers = new float[] { 0.5f, 1.2f, 2.5f, 2.0f, 1.8f, 2.0f, 2.5f, 1.5f },
                immigrationLaneSettings = new string[] { "all", "all", "all", "all" }
            },
            new ScenarioPreset
            {
                id = "optimized",
                displayName = "Optimized",
                arrivalRate = 8f,
                securityLanes = 5,
                securityProcessingTime = 50f,
                securityMinTime = 25f,
                securityMaxTime = 75f,
                securityEnhancedProb = 0.05f,
                immigrationCounters = 6,
                immigrationProcessingTime = 75f,
                immigrationMinTime = 40f,
                immigrationMaxTime = 110f,
                automatedProcessingTime = 50f,
                automatedMinTime = 25f,
                automatedMaxTime = 75f,
                automatedErrorProb = 0.02f,
                enhancedScreeningProb = 0.1f,
                citizenPercentage = 70,
                baseWalkingSpeed = 90f,
                timeMultipliers = new float[] { 0.4f, 1.0f, 2.0f, 1.5f, 1.2f, 1.4f, 1.8f, 0.8f },
                immigrationLaneSettings = new string[] { "citizens", "all", "all", "foreigners" }
            },
            new ScenarioPreset
            {
                id = "wet_weather",
                displayName = "Wet Weather",
                arrivalRate = 4f,
                securityLanes = 3,
                securityProcessingTime = 75f,
                securityMinTime = 40f,
                securityMaxTime = 110f,
                securityEnhancedProb = 0.08f,
                immigrationCounters = 4,
                immigrationProcessingTime = 105f,
                immigrationMinTime = 55f,
                immigrationMaxTime = 155f,
                automatedProcessingTime = 70f,
                automatedMinTime = 35f,
                automatedMaxTime = 105f,
                automatedErrorProb = 0.08f,
                enhancedScreeningProb = 0.12f,
                citizenPercentage = 90,
                baseWalkingSpeed = 55f,
                timeMultipliers = new float[] { 0.2f, 0.9f, 2.0f, 1.7f, 1.4f, 1.6f, 2.0f, 1.0f },
                immigrationLaneSettings = new string[] { "all", "all", "all", "all" }
            }
        };
    }
}