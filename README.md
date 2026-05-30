# Checkpoint Simulation

A comprehensive Unity-based simulation system for modeling checkpoint operations, including security screening and immigration processing. This project simulates traveler flow through multiple processing lanes with configurable parameters for real-world scenario analysis.

## Overview

The Checkpoint Simulation is designed to model and analyze the performance of border checkpoint operations. It allows users to:

- Simulate traveler arrivals and processing through security and immigration checkpoints
- Configure multiple processing lanes with different staffing levels
- Analyze performance metrics including wait times, throughput, and resource utilization
- Test different scenarios and optimization strategies
- Export detailed logs for further analysis

### Available Actions in the Simulation

**Simulation Control:**
- Play/Pause simulation execution
- Reset simulation to initial state
- Adjust simulation speed (1x, 2x, 5x, 10x, 20x, 50x, 100x, 200x, 500x)
- View real-time metrics and queue status

**Configuration & Analysis:**
- Load preset scenarios (Baseline, Holiday, Optimized, Wet Weather)
- Adjust traveler arrival rates and distributions
- Configure security and immigration processing parameters
- Monitor performance through real-time metrics dashboard
- View detailed day reports with statistics and utilization analysis
- Download traveler logs in CSV format

**Parameter Adjustment:**
- Traveler type distribution (standard-yellow, business-green, family-red)
- Time-of-day multipliers for arrival rates
- Processing times and variability
- Security lane and immigration counter configuration
- Lane-specific settings for immigration processing

---

## Engine Version

**Unity: 6000.4.0f1**

This project requires Unity 6 (6000.4.0f1 or compatible versions) with TextMesh Pro support.

---

## Setup Instructions

### Prerequisites

- Unity 6000.4.0f1 or later
- .NET Framework 4.7.1 compatible environment
- Minimum 4GB RAM
- TextMesh Pro package (included with Unity)

### Installation Steps

1. **Clone or Extract the Repository**
   ```bash
   cd "C:\Users\[username]\OneDrive\Documents\Checkpoint Simulation"
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open Project"
   - Navigate to the Checkpoint Simulation folder
   - Wait for Unity to load and compile the project

3. **Verify Installation**
   - Check the Console window for any compilation errors
- Ensure the main scene loads without errors
   - All script files should be in `Assets/Scripts/`

### Project Structure

```
Checkpoint Simulation/
├── Assets/
│   ├── Scripts/
│   │   ├── Simulation/       # Core simulation logic
│   │   │   ├── SimulationController.cs
│   │   │   ├── TravelerAgent.cs
│   │   │   ├── SecurityCounter.cs
│   │   │   ├── ImmigrationCounter.cs
│   │   │   ├── LaneWaypointHolder.cs
│   │   │   └── ImmigrationCounter.cs
│   │ ├── Data/        # Configuration and data structures
│   │   │   ├── GameConfig.cs
│   │   │   ├── SimulationData.cs
│   │   │   ├── ScenarioPreset.cs
│   │   │   └── [other data classes]
│   │   ├── UI/          # User interface management
│   │   │   ├── UIManager.cs
│   │   │   ├── LogEntryUI.cs
│   │   │   └── UtilizationEntryUI.cs
│ │   └── Utils/      # Utility classes
│   │       ├── InteractionManager.cs
│   │       ├── ObjectPoolManager.cs
│   │       ├── CameraController.cs
│   │       └── [other utilities]
│   └── [Scenes, Prefabs, Resources, etc.]
├── ProjectSettings/
├── Assembly-CSharp.csproj
└── README.md
```

---

## Run Instructions

### Starting the Simulation (Unity Build)

1. **Open the Executable file**
   - In the build folder, run "Checkpoint Simulation.exe"

### Starting the Simulation (Unity Editor)

1. **Open the Main Scene**
   - In Unity, navigate to the Scenes folder
   - Double-click the main simulation scene to open it

2. **Enter Play Mode**
   - Click the Play button in the Unity toolbar
   - Or press Ctrl+P (Windows)

3. **Using the Simulation Interface**
   - **Play/Pause Button**: Control simulation execution
   - **Speed Button**: Cycle through simulation speeds
   - **Reset Button**: Return simulation to initial state
   - **Settings Button**: Access parameter configuration panel
   - **Analysis Button**: View performance reports
   - **Logs Button**: Examine individual traveler logs
   - **Exit Button**: Close the application

### Real-Time Monitoring

The top bar displays live metrics:
- **Simulation Time**: Current time in the 24-hour simulation (HH:MM:SS format)
- **Security Queue**: Current number of travelers waiting for security screening
- **Immigration Queue**: Current number of travelers waiting for immigration processing
- **Sec Wait**: Average security processing wait time (minutes)
- **Imm Wait**: Average immigration processing wait time (minutes)
- **Avg Wait**: Overall average wait time across both processes (minutes)
- **Arrival Flow**: Number of travelers arriving per minute
- **Throughput**: Number of travelers processed per hour
- **Man-Hours**: Total staff hours consumed

---

## How to Modify Simulation Parameters

### Via UI (Recommended)

1. **Open Settings Panel**
   - Click "Settings" button in the bottom bar
   - Or press the Settings button during simulation

2. **Configure Parameters**

   **Scenario Selection:**
   - Choose from preset scenarios: Baseline, Holiday, Optimized, Wet Weather
   - Each preset adjusts multiple parameters automatically

   **Traveler Composition:**
- **Standard/Business/Family Type Weights**: Use sliders to adjust traveler type distribution (must sum to 100%)
   - Types have different speed and processing time multipliers

   **Arrival Settings:**
   - **Arrival Rate**: Travelers per minute (range: 0-20+)
   - **Distribution Type**: Poisson (stochastic) or Fixed (deterministic)
   - **Citizen Percentage**: Percentage of citizens vs. foreign nationals (0-100%)
   - **Walking Speed**: Base movement speed for travelers

   **Time Multipliers:**
   - 8 sliders representing 3-hour periods (0-3h, 3-6h, 6-9h, etc.)
   - Values control arrival rate variation throughout the day

   **Security Configuration:**
   - **Security Lanes**: Number of active security screening lanes (1-4)
   - **Processing Time**: Average security screening duration (seconds)
   - **Min/Max Time**: Processing time variation range
   - **Enhanced Screening Probability**: Chance of additional screening

   **Immigration Configuration:**
   - **Immigration Counters**: Number of active immigration counters (1-4)
   - **Counter Type**: Manual or Automated processing
   - **Processing Time**: Average immigration processing duration
   - **Min/Max Time**: Processing time variation range
   - **Lane Restrictions**: Set per-lane eligibility (All, Citizens Only, Foreigners Only)
   - **Automated Error Probability**: Chance of automated processing failure
   - **Enhanced Screening Probability**: Additional screening likelihood

## Performance Metrics & Analysis

### Real-Time Metrics

- **Queue Lengths**: Current security and immigration queue sizes
- **Wait Times**: Average processing times for each stage
- **Throughput**: Number of travelers processed per hour
- **Resource Utilization**: Staff efficiency and utilization percentages

### Day Report (Analysis)

**Page 1 - Overview:**
- Total travelers processed
- Average hourly throughput
- Average system wait time
- Peak queue sizes

**Page 2 - Peak Performance:**
- Peak hour throughput and wait times
- Total man-hours consumed
- Peak period analysis

**Page 3 - Utilization:**
- Per-counter utilization percentages
- Counter activity hours
- Color-coded efficiency ratings:
  - Green: < 30% utilization
  - Yellow: 30-60% utilization
  - Orange: 60-85% utilization
  - Red: > 85% utilization

### Data Export

- **Download Logs**: Export traveler-level data as CSV file
- Includes arrival times, processing times, wait times, citizen status, traveler type
- Saved to persistent data directory with timestamp

---
