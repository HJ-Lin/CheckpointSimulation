using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("Visual Indicators")]
    [SerializeField] private GameObject statusPanel;           // Panel that rotates to face camera
    [SerializeField] private TMP_Text statusText;              // Shows "ACTIVE", "INACTIVE", "OCCUPIED"
    [SerializeField] private GameObject officerImage;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private MeshRenderer indicatorLight;      // Small sphere or cube for color state
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color occupiedColor = Color.red;
    [SerializeField] private Color drainingColor = new Color(1f, 0.5f, 0f); // Orange

    // ----- ADDED FOR INTERACTION -----
    [Header("Interaction")]
    [SerializeField] private Collider clickCollider;  // assign in Inspector (e.g., a BoxCollider)
    [Header("Connection Visuals")]
    [SerializeField] private LineRenderer lineRenderer;

    private SimulationController controller;
    private float processingDuration;
    private bool isDraining = false;

    void Start()
    {
        controller = SimulationController.Instance;
        SetIndicatorRotation();
        // Ensure collider exists
        if (clickCollider == null) clickCollider = GetComponent<Collider>();
        if (clickCollider == null) Debug.LogWarning($"ImmigrationCounter {id} has no collider for clicking.");

        // Ensure a LineRenderer exists
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        ConfigureLineRenderer();

        controller.OnCounterSelected += OnSetSelected;
    }

    private void OnDestroy()
    {
        controller.OnCounterSelected -= OnSetSelected;
    }

    void Update()
    {
        if (controller.isPaused) return;

        // Determine draining state (inactive but still serving hooked lanes)
        isDraining = !active && IsDraining();

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
            UpdateVisualState();
        }
        else if (!occupied && (active || isDraining))
        {
            controller.TryAssignTravelerToImmigrationCounter(this);
        }

        if (active)
        {
            activeTime += Time.deltaTime * controller.simulationSpeed;
            if (occupied) busyTime += Time.deltaTime * controller.simulationSpeed;
        }

        // Update UI every frame for real-time status
        UpdateVisualState();
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

        UpdateVisualState();
    }

    public bool CanServeTraveler(TravelerAgent traveler)
    {
        // If no hooked lanes, this counter can serve any traveler (if restrictions allow)
        if (hookedLanes.Count == 0)
        {
            // Check the lane setting for the traveler's current lane
            string setting = controller.gameConfig.immigrationLaneSettings[traveler.LaneIndex];
            if (setting == "all") return true;
            if (setting == "citizens" && traveler.IsCitizen) return true;
            if (setting == "foreigners" && !traveler.IsCitizen) return true;
            return false;
        }

        // Check lane assignment restrictions for hooked lanes
        foreach (int laneIdx in hookedLanes)
        {
            string setting = controller.gameConfig.immigrationLaneSettings[laneIdx];
            if (setting == "all") return true;
            if (setting == "citizens" && traveler.IsCitizen) return true;
            if (setting == "foreigners" && !traveler.IsCitizen) return true;
        }
        return false;
    }

    private float TriangularRandom(float min, float max, float mode)
    {
        float u = Random.value;
        float f = (mode - min) / (max - min);
        if (u < f) return min + Mathf.Sqrt(u * (max - min) * (mode - min));
        else return max - Mathf.Sqrt((1 - u) * (max - min) * (max - mode));
    }

    private void UpdateVisualState()
    {
        if (statusText != null)
        {
            if (occupied)
                statusText.text = "OCCUPIED";
            else if (active)
                statusText.text = type == CounterType.Manned ? "MANNED" : "AUTO";
            else if (isDraining)
                statusText.text = "DRAINING";
            else
                statusText.text = "INACTIVE";
        }

        if (indicatorLight != null)
        {
            if (occupied)
                indicatorLight.material.color = occupiedColor;
            else if (active)
                indicatorLight.material.color = activeColor;
            else if (isDraining)
                indicatorLight.material.color = drainingColor;
            else
                indicatorLight.material.color = inactiveColor;
        }

        if (officerImage != null)
        {
            officerImage.SetActive(active && type == CounterType.Manned);
        }
    }

    private void SetIndicatorRotation()
    {
        // Set citizen indicator to be parallel to camera (only once during initialization)
        if (statusPanel != null && Camera.main != null)
        {
            statusPanel.transform.rotation = Camera.main.transform.rotation;
        }
    }

    /// <summary>
    /// Initialize the counter with its ID, active state, and hooked lanes.
    /// Called by SimulationController during setup.
    /// </summary>
    public void Initialize(int counterId, bool isActive, List<int> lanes)
    {
        id = counterId;
        active = isActive;
        hookedLanes = new List<int>(lanes);
        // Ensure default type is Manned (will be overwritten by scenario if needed)
        if (!active && type != CounterType.Manned) type = CounterType.Manned;

        // Reset state
        occupied = false;
        currentTraveler = null;
        activeTime = 0f;
        busyTime = 0f;
        processingEndTime = 0f;
        isDraining = false;

        // Update visual indicators to reflect new state
        UpdateVisualState();
        RefreshConnectionLines();
    }

    public void OnClicked()
    {
        // If this counter is already selected, cycle its state (Closed → Manned → Automated → Closed)
        if (controller.SelectedCounter == this)
        {
            CycleState();
        }
        else
        {
            // Otherwise, select this counter
            controller.SelectedCounter = this;
        }
        // Play click sound (optional)
        // AudioManager.Play("ui_click");
        RefreshConnectionLines();
    }

    // ----- NEW METHOD: Cycle through states -----
    public void CycleState()
    {
        if (!active)
        {
            // Closed → Manned (open)
            active = true;
            type = CounterType.Manned;
        }
        else if (type == CounterType.Manned)
        {
            // Manned → Automated
            type = CounterType.Automated;
            // active stays true
        }
        else // type == Automated and active == true
        {
            // Automated → Closed
            active = false;
            type = CounterType.Manned;   // reset for next cycle
        }

        // Update the config value to match the number of active counters
        controller.UpdateImmigrationCountersCount();
        UpdateVisualState();
    }

    public void OnSetSelected(object selectedCounter)
    {
        if (selectedImage != null)
        {
            selectedImage.SetActive(selectedCounter == this);
        }
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null) return;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // Use greenish color for immigration lines (matching HTML)
        lineRenderer.startColor = new Color(0.3f, 0.8f, 0.3f, 0.7f);
        lineRenderer.endColor = new Color(0.3f, 0.8f, 0.3f, 0.7f);
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Redraws lines from this counter to all lane heads it is connected to.
    /// Called whenever hookedLanes or active state changes.
    /// </summary>
    public void RefreshConnectionLines()
    {
        if (lineRenderer == null) return;

        var controller = SimulationController.Instance;
        int connectedCount = 0;
        foreach (int laneIdx in hookedLanes)
        {
            var lane = controller.GetImmigrationLane(laneIdx);
            if (lane != null && lane.laneHead != null)
                connectedCount++;
        }
        lineRenderer.positionCount = connectedCount * 2;
        int pointIndex = 0;
        foreach (int laneIdx in hookedLanes)
        {
            var lane = controller.GetImmigrationLane(laneIdx);
            if (lane != null && lane.laneHead != null)
            {
                Vector3 start = transform.position;
                Vector3 end = lane.laneHead.position;
                lineRenderer.SetPosition(pointIndex++, start);
                lineRenderer.SetPosition(pointIndex++, end);
            }
        }
        bool shouldShow = (active || IsDraining()) && connectedCount > 0;
        lineRenderer.enabled = shouldShow;
    }
}