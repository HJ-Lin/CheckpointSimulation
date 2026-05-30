using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class LaneWaypointHolder : MonoBehaviour
{
    [Header("Lane Geometry")]
    public int laneIndex;
    public Transform laneStart;        // The point where travelers first enter this lane
    public Vector3 queueDirection = Vector3.back; // Direction towards the counter (e.g., -Z)
    public float spacing = 1.2f;       // Distance between queue positions

    [Header("Interaction")]
    [SerializeField] private Collider clickCollider;

    [Header("Lane Head")]
    public Transform laneHead;   // The physical position of the lane's front (where counters connect to)

    [Header("Lane Type HUD Configuration")]
    [SerializeField] private bool enableHUD = true;
    [SerializeField] private bool isImmigrationLane = false; // Set to true for immigration lanes

    // Cache – no need to store actual Transforms
    private List<Vector3> cachedPositions = new List<Vector3>();
    public List<Vector3> CachedPositions => cachedPositions;

    // HUD References
    [SerializeField] private GameObject hudGameObject;
    [SerializeField] private TMP_Text hudTextMeshPro;

    public static Action<LaneWaypointHolder, string> OnImmigrationLaneChanged;

    void Start()
    {
        if (clickCollider == null) clickCollider = GetComponent<Collider>();
        
        if (enableHUD && isImmigrationLane)
        {
            SetIndicatorRotation();
            RefreshHUDText();
        }
    }

    public void RefreshHUDText()
    {
        if (!enableHUD || !isImmigrationLane || hudTextMeshPro == null)
        {
            hudGameObject.SetActive(false);
            return;
        }

        var controller = SimulationController.Instance;
        if (controller == null)
            return;

        string setting = controller.gameConfig.immigrationLaneSettings[laneIndex];
        string displayText = GetDisplayTextForSetting(setting);
        
        hudTextMeshPro.text = displayText;
        hudGameObject.SetActive(true);
    }

    private string GetDisplayTextForSetting(string setting)
    {
        return setting switch
        {
            "all" => "All",
            "citizens" => "Citizens",
            "foreigners" => "Foreigners",
            _ => "Unknown"
        };
    }

    public void OnClicked()
    {
        var controller = SimulationController.Instance;
        var selected = controller.SelectedCounter;

        // If a SecurityCounter is selected, toggle hook for this lane
        if (selected is SecurityCounter secCounter)
        {
            if (secCounter.hookedLanes.Contains(laneIndex))
                secCounter.hookedLanes.Remove(laneIndex);
            else
                secCounter.hookedLanes.Add(laneIndex);
            // Play sound
            secCounter.RefreshConnectionLines();
        }
        // If an ImmigrationCounter is selected, toggle hook for this lane
        else if (selected is ImmigrationCounter immCounter)
        {
            if (immCounter.hookedLanes.Contains(laneIndex))
                immCounter.hookedLanes.Remove(laneIndex);
            else
                immCounter.hookedLanes.Add(laneIndex);
            // Play sound
            immCounter.RefreshConnectionLines();
        }
        else
        {
            // No counter selected → cycle lane citizenship setting (for immigration lanes only)
            if (isImmigrationLane)
            {
                string[] settings = { "all", "citizens", "foreigners" };
                string current = controller.gameConfig.immigrationLaneSettings[laneIndex];
                int nextIdx = (System.Array.IndexOf(settings, current) + 1) % settings.Length;
                controller.gameConfig.immigrationLaneSettings[laneIndex] = settings[nextIdx];
                RefreshHUDText();

                // Update UI dropdown if needed
                OnImmigrationLaneChanged?.Invoke(this, settings[nextIdx]);

                // Play sound
            }
        }
    }

    /// <summary> Returns the world position for a given queue index (0 = front). </summary>
    public Vector3 GetQueuePosition(int index)
    {
        // Expand cache if needed
        while (cachedPositions.Count <= index)
        {
            Vector3 newPos = laneStart.position + queueDirection * (cachedPositions.Count * spacing);
            newPos.y = 0.17f;
            cachedPositions.Add(newPos);
        }
        return cachedPositions[index];
    }

    // Optional: visually debug waypoints in Editor
    private void OnDrawGizmosSelected()
    {
        if (laneStart == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++) // preview up to 20
        {
            Vector3 pos = laneStart.position + queueDirection * (i * spacing);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
    }

    private void SetIndicatorRotation()
    {
        // Set citizen indicator to be parallel to camera (only once during initialization)
        if (hudGameObject != null && Camera.main != null)
        {
            hudGameObject.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
