using UnityEngine;
using TMPro;

/// <summary>
/// A poolable log entry UI component for displaying traveler log information.
/// Implements IPoolable for object pool reuse.
/// </summary>
public class LogEntryUI : MonoBehaviour, IPoolable
{
    [SerializeField]
    private TMP_Text[] textFields;

    void Awake()
    {
        if (textFields.Length < 9)
        {
            Debug.LogWarning("LogEntryUI requires 9 TMP_Text components (ID, Type, Citizen, Arrival, Sec Start, Sec End, Imm Start, Imm End, Exit)");
        }
    }

    /// <summary>
    /// Display a traveler log entry in the UI.
    /// </summary>
    public void DisplayLog(TravelerLogEntry log)
    {
        if (textFields.Length < 9) return;

        textFields[0].text = log.id.ToString();
        textFields[1].text = log.type;
        textFields[2].text = log.isCitizen ? "Yes" : "No";
        textFields[3].text = FormatTime(log.arrivalTime);
        textFields[4].text = FormatTime(log.securityStartTime);
        textFields[5].text = FormatTime(log.securityEndTime);
        textFields[6].text = FormatTime(log.immigrationStartTime);
        textFields[7].text = FormatTime(log.immigrationEndTime);
        textFields[8].text = FormatTime(log.exitTime);
    }

    /// <summary>
    /// Clear all text fields.
    /// </summary>
    public void ClearDisplay()
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            textFields[i].text = "";
        }
    }

    /// <summary>
    /// Called when the entry is taken from the pool.
    /// </summary>
    public void OnSpawnFromPool()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when the entry is returned to the pool.
    /// </summary>
    public void OnReturnToPool()
    {
        ClearDisplay();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Get the GameObject of this poolable object.
    /// </summary>
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /// <summary>
    /// Format time in seconds to HH:MM:SS format.
    /// </summary>
    private string FormatTime(float seconds)
    {
        if (seconds <= 0) return "-";
        int totalSecs = Mathf.FloorToInt(seconds);
        int hours = (totalSecs / 3600) % 24;
        int mins = (totalSecs % 3600) / 60;
        int secs = totalSecs % 60;
        return $"{hours:D2}:{mins:D2}:{secs:D2}";
    }
}
