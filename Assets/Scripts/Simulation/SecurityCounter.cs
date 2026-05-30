using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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

    [Header("Visual Indicators")]
    [SerializeField] private GameObject statusPanel;           // Panel that rotates to face camera
    [SerializeField] private TMP_Text statusText;              // Shows "ACTIVE", "INACTIVE", "OCCUPIED"
    [SerializeField] private GameObject officerImage;
    [SerializeField] private MeshRenderer indicatorLight;      // Small sphere or cube for color state
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color occupiedColor = Color.red;
    [SerializeField] private Color drainingColor = new Color(1f, 0.5f, 0f); // Orange

    private SimulationController controller;
    private float processingDuration;
    private bool isDraining = false;

    void Start()
    {
        controller = SimulationController.Instance;
        SetIndicatorRotation();
    }

    void Update()
    {
        if (controller.isPaused) return;

        // Determine draining state (inactive but still serving hooked lanes)
        isDraining = !active && IsDraining();

        if (occupied && controller.simulationTime >= processingEndTime)
        {
            // Finished processing current traveler
            controller.OnSecurityProcessingComplete(this);
            occupied = false;
            currentTraveler = null;
            UpdateVisualState();
        }
        else if (!occupied && (active || isDraining))
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

        // Update UI every frame for real-time status
        UpdateVisualState();
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
        Vector3 newPos = transform.position + Vector3.right * 0.5f;
        newPos.y = 0.17f;
        traveler.SetTarget(newPos);

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

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (statusText != null)
        {
            if (occupied)
                statusText.text = "OCCUPIED";
            else if (active)
                statusText.text = "ACTIVE";
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
            officerImage.SetActive(active);
        }
    }

    private float TriangularRandom(float min, float max, float mode)
    {
        float u = Random.value;
        float f = (mode - min) / (max - min);
        if (u < f) return min + Mathf.Sqrt(u * (max - min) * (mode - min));
        else return max - Mathf.Sqrt((1 - u) * (max - min) * (max - mode));
    }

    // Optional: Called when object is pooled (if using pooling)
    public void OnSpawnFromPool()
    {
        active = true;
        occupied = false;
        currentTraveler = null;
        activeTime = 0f;
        busyTime = 0f;
        processingEndTime = 0f;
        UpdateVisualState();
    }

    public void OnReturnToPool()
    {
        active = true;
        occupied = false;
        currentTraveler = null;
        activeTime = 0f;
        busyTime = 0f;
        processingEndTime = 0f;
        UpdateVisualState();
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

        // Reset state
        occupied = false;
        currentTraveler = null;
        activeTime = 0f;
        busyTime = 0f;
        processingEndTime = 0f;
        isDraining = false;

        // Update visual indicators to reflect new state
        UpdateVisualState();
    }
}
