using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    [Header("Configuration")]
    public GameConfig gameConfig;
    public List<ScenarioPreset> scenarios;

    [Header("Scene References")]
    public Transform securityArea;
    public Transform immigrationArea;
    public Transform travelersParent;
    public GameObject travelerPrefab;
    public SecurityCounter[] securityCounters;
    public ImmigrationCounter[] immigrationCounters;
    public LaneWaypointHolder[] securityLanes;
    public LaneWaypointHolder[] immigrationLanes;
    public Transform spawnPoint;
    public Transform exitPoint;

    [Header("Simulation State")]
    public bool isPaused = true;
    public float simulationSpeed = 1f;
    public float simulationTime = 0f; // in seconds
    public int nextTravelerId = 0;
    public float nextSpawnTime = 0f;
    public List<TravelerAgent> activeTravelers = new List<TravelerAgent>();
    public List<TravelerLogEntry> travelerLogs = new List<TravelerLogEntry>();
    public SimulationMetrics metrics = new SimulationMetrics();

    [Header("Object Pooling")]
    [SerializeField] private int travelerPoolInitialSize = 50;

    public Action<object> OnCounterSelected;
    private object selectedCounter; // Can be SecurityCounter or ImmigrationCounter
    public object SelectedCounter
    {
        get => selectedCounter;
        set
        {
            selectedCounter = value;
            // Optionally highlight the selected counter (you'd need a visual feedback method)
            OnCounterSelected?.Invoke(selectedCounter);
        }
    }

    // Internal queues for each lane
    private List<TravelerAgent>[] securityQueues = new List<TravelerAgent>[4];
    private List<TravelerAgent>[] immigrationQueues = new List<TravelerAgent>[4];
    private List<float> arrivalTimes = new List<float>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < 4; i++)
        {
            securityQueues[i] = new List<TravelerAgent>();
            immigrationQueues[i] = new List<TravelerAgent>();
        }
    }

    void Start()
    {
        // Initialize the traveler object pool
        ObjectPoolManager.Instance.InitializePool("Travelers", travelerPrefab, travelerPoolInitialSize, travelersParent);

        nextSpawnTime = CalculateNextArrivalTime(0f);
        ApplyScenario(scenarios[0]);

        UIManager.Instance.Init();
    }

    public void ApplyScenario(ScenarioPreset scenario)
    {
        gameConfig.displayName = scenario.displayName;
        gameConfig.arrivalRate = scenario.arrivalRate;
        gameConfig.securityLanes = scenario.securityLanes;
        gameConfig.securityProcessingTime = scenario.securityProcessingTime;
        gameConfig.securityMinTime = scenario.securityMinTime;
        gameConfig.securityMaxTime = scenario.securityMaxTime;
        gameConfig.securityEnhancedProb = scenario.securityEnhancedProb;
        gameConfig.immigrationCounters = scenario.immigrationCounters;
        gameConfig.immigrationProcessingTime = scenario.immigrationProcessingTime;
        gameConfig.immigrationMinTime = scenario.immigrationMinTime;
        gameConfig.immigrationMaxTime = scenario.immigrationMaxTime;
        gameConfig.automatedProcessingTime = scenario.automatedProcessingTime;
        gameConfig.automatedMinTime = scenario.automatedMinTime;
        gameConfig.automatedMaxTime = scenario.automatedMaxTime;
        gameConfig.automatedErrorProb = scenario.automatedErrorProb;
        gameConfig.enhancedScreeningProb = scenario.enhancedScreeningProb;
        gameConfig.citizenPercentage = scenario.citizenPercentage;
        gameConfig.baseWalkingSpeed = scenario.baseWalkingSpeed;
        gameConfig.timeMultipliers = scenario.timeMultipliers;
        gameConfig.immigrationLaneSettings = scenario.immigrationLaneSettings;
        gameConfig.InitializeDefaults();

        ResetSimulation();
    }

    void Update()
    {
        if (isPaused) return;

        float delta = Time.deltaTime * simulationSpeed;
        simulationTime += delta;

        // Update man-hours
        int activeStaff = securityCounters.Count(c => c.active) + immigrationCounters.Count(c => c.active);
        metrics.totalManHours += (activeStaff * delta) / 3600f;

        if (simulationTime >= 86400f) // 24 hours
        {
            simulationTime = 86400f;
            isPaused = true;
            UIManager.Instance.ShowDayReport();
        }
        else
        {
            SpawnTravelers();
            ProcessQueues();
            UpdateMetrics();
        }
    }

    private void InitializeCounters()
    {
        for (int i = 0; i < securityCounters.Length; i++)
        {
            List<int> lanes = new List<int>();
            if (i < 4) lanes.Add(i);
            
            bool isActive = i < gameConfig.securityLanes;
            securityCounters[i].Initialize(i, isActive, lanes);
        }

        for (int i = 0; i < immigrationCounters.Length; i++)
        {
            List<int> lanes = new List<int>();
            if (i < 4) lanes.Add(i);

            bool isActive = i < gameConfig.immigrationCounters;
            immigrationCounters[i].Initialize(i, isActive, lanes);
        }
    }

    private float CalculateNextArrivalTime(float currentTime)
    {
        float multiplier = GetTimeMultiplier(currentTime);
        float ratePerMin = gameConfig.arrivalRate * multiplier;
        if (ratePerMin <= 0) return currentTime + 60f;

        if (gameConfig.arrivalDistribution == "poisson")
        {
            float lambda = ratePerMin / 60f;
            float interArrival = -Mathf.Log(1f - UnityEngine.Random.value) / lambda;
            return currentTime + interArrival;
        }
        else
        {
            return currentTime + (60f / ratePerMin);
        }
    }

    public float GetTimeMultiplier(float timeSec)
    {
        int hour = (int)(timeSec / 3600f) % 24;
        int index = Mathf.FloorToInt(hour / 3f);
        return gameConfig.timeMultipliers[index];
    }

    private void SpawnTravelers()
    {
        if (nextSpawnTime == 0) nextSpawnTime = CalculateNextArrivalTime(simulationTime);

        while (simulationTime >= nextSpawnTime)
        {
            // Find best security lane to join
            int bestLane = GetBestSecurityLane();
            if (bestLane != -1)
            {
                // Get traveler from object pool instead of instantiating
                IPoolable poolable = ObjectPoolManager.Instance.GetFromPool("Travelers");
                if (poolable != null)
                {
                    TravelerAgent traveler = poolable as TravelerAgent;
                    GameObject travelerObj = traveler.GetGameObject();
                    travelerObj.transform.position = spawnPoint.position;

                    // Determine traveler type
                    string travelerType = GetRandomTravelerType();
                    bool isCitizen = UnityEngine.Random.value < (gameConfig.citizenPercentage / 100f);

                    traveler.Initialize(nextTravelerId++, travelerType, isCitizen, nextSpawnTime);
                    traveler.LaneIndex = bestLane;
                    activeTravelers.Add(traveler);
                    arrivalTimes.Add(nextSpawnTime);

                    // Move to lane entry point
                    Vector3 entryPos = securityLanes[bestLane].GetQueuePosition(0);
                    entryPos.y = spawnPoint.position.y;
                    traveler.SetTarget(entryPos);
                }
            }

            nextSpawnTime = CalculateNextArrivalTime(nextSpawnTime);
        }

        // Clean old arrival times (last 60 seconds)
        arrivalTimes.RemoveAll(t => simulationTime - t > 60f);
    }

    private string GetRandomTravelerType()
    {
        float totalWeight = gameConfig.travelerTypes.Sum(t => t.Value.weight);
        float r = UnityEngine.Random.value * totalWeight;
        float accum = 0;
        foreach (var kvp in gameConfig.travelerTypes)
        {
            accum += kvp.Value.weight;
            if (r <= accum) return kvp.Key;
        }
        return "standard";
    }

    private int GetBestSecurityLane()
    {
        // Find lanes served by at least one active counter
        var servedLanes = new HashSet<int>();
        foreach (var counter in securityCounters.Where(c => c.active))
        {
            if (counter.hookedLanes.Count > 0)
                foreach (int l in counter.hookedLanes) servedLanes.Add(l);
            else
                servedLanes.Add(GetClosestLaneToCounter(counter));
        }

        if (servedLanes.Count == 0) return -1;

        // Choose shortest queue among served lanes
        int bestLane = servedLanes.First();
        int minQueue = securityQueues[bestLane].Count;
        foreach (int lane in servedLanes)
        {
            if (securityQueues[lane].Count < minQueue)
            {
                minQueue = securityQueues[lane].Count;
                bestLane = lane;
            }
        }
        return bestLane;
    }

    private int GetClosestLaneToCounter(SecurityCounter counter)
    {
        float minDist = float.MaxValue;
        int closest = 0;
        for (int i = 0; i < 4; i++)
        {
            float dist = Vector3.Distance(counter.transform.position, securityLanes[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        return closest;
    }

    public void OnTravelerArrivedAtSecurity(TravelerAgent traveler)
    {
        traveler.State = TravelerState.InSecurityQueue;
        traveler.SecurityQueueJoinTime = simulationTime;
        securityQueues[traveler.LaneIndex].Add(traveler);
        int queuePos = securityQueues[traveler.LaneIndex].Count - 1;
        traveler.SetSecurityQueuePosition(queuePos);
    }

    public void TryAssignTravelerToSecurityCounter(SecurityCounter counter)
    {
        // First check hooked lanes
        foreach (int laneIdx in counter.hookedLanes)
        {
            if (securityQueues[laneIdx].Count > 0)
            {
                TravelerAgent traveler = securityQueues[laneIdx][0];
                if (!counter.active && IsSecurityLaneServed(laneIdx)) continue;
                securityQueues[laneIdx].RemoveAt(0);
                UpdateSecurityQueuePositions(laneIdx);
                counter.AssignTraveler(traveler);
                return;
            }
        }

        // If active, check nearest lane
        if (counter.active)
        {
            int bestLane = GetClosestLaneToCounter(counter);
            if (securityQueues[bestLane].Count > 0)
            {
                TravelerAgent traveler = securityQueues[bestLane][0];
                securityQueues[bestLane].RemoveAt(0);
                UpdateSecurityQueuePositions(bestLane);
                counter.AssignTraveler(traveler);
                return;
            }
        }
    }

    public void OnSecurityProcessingComplete(SecurityCounter counter)
    {
        TravelerAgent traveler = counter.currentTraveler;
        traveler.SecurityEndTime = simulationTime;

        // Calculate wait time
        float wait = traveler.SecurityEndTime - traveler.EntryTime;
        metrics.totalSecurityWaitTime += wait;
        metrics.securityProcessedCount++;

        if (GetTimeMultiplier(simulationTime) >= 2.0f)
        {
            metrics.peakTotalSecurityWaitTime += wait;
            metrics.peakSecurityProcessedCount++;
        }

        // Move to immigration - find best lane
        int bestLane = GetBestImmigrationLane(traveler);
        if (bestLane != -1)
        {
            traveler.LaneIndex = bestLane;
            traveler.State = TravelerState.InImmigrationQueue;
            traveler.ImmigrationQueueJoinTime = simulationTime;
            immigrationQueues[bestLane].Add(traveler);
            int queuePos = immigrationQueues[bestLane].Count - 1;
            traveler.SetImmigrationQueuePosition(queuePos);
        }
        else
        {
            // No immigration lanes available - hold at security (shouldn't happen)
            traveler.State = TravelerState.Leaving;
            traveler.SetTarget(exitPoint.position);
        }
    }

    private int GetBestImmigrationLane(TravelerAgent traveler)
    {
        var eligibleLanes = new HashSet<int>();
        foreach (var counter in immigrationCounters.Where(c => c.active))
        {
            foreach (int lane in counter.hookedLanes)
            {
                string setting = gameConfig.immigrationLaneSettings[lane];
                if (setting == "all") eligibleLanes.Add(lane);
                else if (setting == "citizens" && traveler.IsCitizen) eligibleLanes.Add(lane);
                else if (setting == "foreigners" && !traveler.IsCitizen) eligibleLanes.Add(lane);
            }
        }

        if (eligibleLanes.Count == 0) return -1;

        int bestLane = eligibleLanes.First();
        int minQueue = immigrationQueues[bestLane].Count;
        foreach (int lane in eligibleLanes)
        {
            if (immigrationQueues[lane].Count < minQueue)
            {
                minQueue = immigrationQueues[lane].Count;
                bestLane = lane;
            }
        }
        return bestLane;
    }

    public void TryAssignTravelerToImmigrationCounter(ImmigrationCounter counter)
    {
        // Check hooked lanes
        foreach (int laneIdx in counter.hookedLanes)
        {
            if (immigrationQueues[laneIdx].Count > 0)
            {
                TravelerAgent traveler = immigrationQueues[laneIdx][0];
                if (!counter.active && IsImmigrationLaneServed(laneIdx)) continue;
                if (counter.type == CounterType.Automated && !traveler.IsCitizen) continue;
                if (!counter.CanServeTraveler(traveler)) continue;

                immigrationQueues[laneIdx].RemoveAt(0);
                UpdateImmigrationQueuePositions(laneIdx);
                counter.AssignTraveler(traveler);
                return;
            }
        }

        // Active counters can pull from any eligible lane
        if (counter.active)
        {
            int bestLane = -1;
            float minDist = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                if (immigrationQueues[i].Count > 0)
                {
                    TravelerAgent traveler = immigrationQueues[i][0];
                    if (counter.type == CounterType.Automated && !traveler.IsCitizen) continue;
                    if (!counter.CanServeTraveler(traveler)) continue;

                    float dist = Vector3.Distance(counter.transform.position, immigrationLanes[i].transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestLane = i;
                    }
                }
            }

            if (bestLane != -1)
            {
                TravelerAgent traveler = immigrationQueues[bestLane][0];
                immigrationQueues[bestLane].RemoveAt(0);
                UpdateImmigrationQueuePositions(bestLane);
                counter.AssignTraveler(traveler);
                return;
            }
        }
    }

    public void OnImmigrationProcessingComplete(ImmigrationCounter counter)
    {
        TravelerAgent traveler = counter.currentTraveler;
        traveler.ImmigrationEndTime = simulationTime;
        traveler.ExitTime = simulationTime;
        traveler.State = TravelerState.Leaving;
        traveler.SetTarget(exitPoint.position);

        // Log entry
        TravelerLogEntry log = new TravelerLogEntry
        {
            id = traveler.Id,
            type = traveler.Type,
            isCitizen = traveler.IsCitizen,
            arrivalTime = traveler.EntryTime,
            securityStartTime = traveler.SecurityStartTime,
            securityEndTime = traveler.SecurityEndTime,
            immigrationStartTime = traveler.ImmigrationStartTime,
            immigrationEndTime = traveler.ImmigrationEndTime,
            exitTime = traveler.ExitTime
        };
        travelerLogs.Add(log);

        // Update metrics
        float immWait = traveler.ImmigrationEndTime - traveler.SecurityEndTime;
        metrics.totalImmigrationWaitTime += immWait;
        metrics.immigrationProcessedCount++;

        float totalTime = traveler.ExitTime - traveler.EntryTime;
        metrics.totalProcessed++;
        metrics.totalWaitTime += totalTime;
        metrics.processedLastHour++;

        if (GetTimeMultiplier(simulationTime) >= 2.0f)
        {
            metrics.peakTotalImmigrationWaitTime += immWait;
            metrics.peakImmigrationProcessedCount++;
            metrics.peakTotalProcessed++;
            metrics.peakTotalWaitTime += totalTime;
        }
    }

    public void OnTravelerExited(TravelerAgent traveler)
    {
        activeTravelers.Remove(traveler);
        
        // Return traveler to the object pool instead of destroying
        IPoolable poolable = traveler as IPoolable;
        if (poolable != null)
        {
            ObjectPoolManager.Instance.ReturnToPool("Travelers", poolable);
        }
    }

    private void UpdateSecurityQueuePositions(int laneIdx)
    {
        for (int i = 0; i < securityQueues[laneIdx].Count; i++)
        {
            securityQueues[laneIdx][i].SetSecurityQueuePosition(i);
        }
    }

    private void UpdateImmigrationQueuePositions(int laneIdx)
    {
        for (int i = 0; i < immigrationQueues[laneIdx].Count; i++)
        {
            immigrationQueues[laneIdx][i].SetImmigrationQueuePosition(i);
        }
    }

    private void ProcessQueues()
    {
        // Queues are processed by counters' Update methods via TryAssign calls
    }

    private void UpdateMetrics()
    {
        metrics.securityQueueLength = securityQueues.Sum(q => q.Count);
        metrics.immigrationQueueLength = immigrationQueues.Sum(q => q.Count);
        metrics.peakSecurityQueue = Mathf.Max(metrics.peakSecurityQueue, metrics.securityQueueLength);
        metrics.peakImmigrationQueue = Mathf.Max(metrics.peakImmigrationQueue, metrics.immigrationQueueLength);

        metrics.measuredArrivalRate = arrivalTimes.Count;

        if (activeTravelers.Count > 0)
            metrics.avgWalkingSpeed = activeTravelers.Average(t => t.Speed);

        if (simulationTime - metrics.lastThroughputUpdate >= 3600f)
        {
            metrics.throughputPerHour = metrics.processedLastHour;
            metrics.processedLastHour = 0;
            metrics.lastThroughputUpdate = simulationTime;
        }

        UIManager.Instance.UpdateMetricsUI();
    }

    public bool IsSecurityLaneServed(int laneIdx)
    {
        return securityCounters.Any(c => c.active && (c.hookedLanes.Count == 0 || c.hookedLanes.Contains(laneIdx)));
    }

    public bool IsImmigrationLaneServed(int laneIdx)
    {
        return immigrationCounters.Any(c => c.active && (c.hookedLanes.Count == 0 || c.hookedLanes.Contains(laneIdx)));
    }

    public int GetSecurityQueueLength(int laneIdx) => securityQueues[laneIdx].Count;
    public int GetImmigrationQueueLength(int laneIdx) => immigrationQueues[laneIdx].Count;
    public LaneWaypointHolder GetSecurityLane(int idx) => securityLanes[idx];
    public LaneWaypointHolder GetImmigrationLane(int idx) => immigrationLanes[idx];

    public void UpdateSecurityLanesCount()
    {
        int activeCount = 0;
        foreach (var c in securityCounters) if (c.active) activeCount++;
        gameConfig.securityLanes = activeCount;
        // Update UI sliders if needed
    }

    public void UpdateImmigrationCountersCount()
    {
        int activeCount = 0;
        foreach (var c in immigrationCounters) if (c.active) activeCount++;
        gameConfig.immigrationCounters = activeCount;
        // Update UI sliders if needed
    }

    public void ResetSimulation()
    {
        // Return all travelers to the pool instead of destroying
        foreach (var traveler in activeTravelers)
        {
            IPoolable poolable = traveler as IPoolable;
            if (poolable != null)
            {
                ObjectPoolManager.Instance.ReturnToPool("Travelers", poolable);
            }
        }
        activeTravelers.Clear();

        // Clear queues
        for (int i = 0; i < 4; i++)
        {
            securityQueues[i].Clear();
            immigrationQueues[i].Clear();
        }
        arrivalTimes.Clear();
        travelerLogs.Clear();

        simulationTime = 0f;
        nextTravelerId = 0;
        nextSpawnTime = CalculateNextArrivalTime(0f);

        // Reset metrics
        metrics = new SimulationMetrics();
        metrics.lastThroughputUpdate = 0f;

        InitializeCounters();

        foreach (var c in securityCounters) c.RefreshConnectionLines();
        foreach (var c in immigrationCounters) c.RefreshConnectionLines();
    }
}