using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top Bar")]
    public TMP_Text simTimeText;
    public TMP_Text securityQueueText;
    public TMP_Text immigrationQueueText;
    public TMP_Text secWaitText;
    public TMP_Text immWaitText;
    public TMP_Text avgWaitText;
    public TMP_Text arrivalFlowText;
    public TMP_Text throughputText;
    public TMP_Text manHoursText;

    [Header("Bottom Bar")]
    public Button playPauseBtn;
    public Button resetBtn;
    public Button speedBtn;
    public Button settingsBtn;
    public Button analysisBtn;
    public Button logsBtn;
    public TMP_Text speedDisplay;
    public Image playPauseIcon;
    public Sprite playIcon;
    public Sprite pauseIcon;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject reportPanel;
    public GameObject logsPanel;

    [Header("Settings Panel Elements")]
    public Slider arrivalRateSlider;
    public TMP_Text arrivalRateValue;
    public TMP_Dropdown distributionDropdown;
    public Slider citizenPercentageSlider;
    public TMP_Text citizenPercentageValue;
    public Slider walkingSpeedSlider;
    public TMP_Text walkingSpeedValue;
    public Slider securityLanesSlider;
    public TMP_Text securityLanesValue;
    public Slider securityTimeSlider;
    public TMP_Text securityTimeValue;
    // Add all other sliders similarly...

    [Header("Report Panel")]
    public TMP_Text reportScenarioName;
    public TMP_Text statTotalTravelers;
    public TMP_Text statAvgThroughput;
    public TMP_Text statAvgSystem;
    public TMP_Text statAvgSecurity;
    public TMP_Text statAvgImmigration;
    public TMP_Text statPeakSec;
    public TMP_Text statPeakImm;
    public GameObject reportPage1;
    public GameObject reportPage2;
    public GameObject reportPage3;
    public Button prevReportBtn;
    public Button nextReportBtn;
    public Button closeReportBtn;

    [Header("Logs Panel")]
    public RectTransform logContent;
    public GameObject logEntryPrefab;
    public Button closeLogsBtn;
    public Button downloadLogsBtn;

    private SimulationController controller;
    private int currentReportPage = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        controller = SimulationController.Instance;

        // Assign button listeners
        playPauseBtn.onClick.AddListener(TogglePlayPause);
        resetBtn.onClick.AddListener(() => controller.ResetSimulation());
        speedBtn.onClick.AddListener(CycleSpeed);
        settingsBtn.onClick.AddListener(() => settingsPanel.SetActive(true));
        analysisBtn.onClick.AddListener(ShowDayReport);
        logsBtn.onClick.AddListener(ShowLogs);

        closeReportBtn.onClick.AddListener(() => reportPanel.SetActive(false));
        closeLogsBtn.onClick.AddListener(() => logsPanel.SetActive(false));
        downloadLogsBtn.onClick.AddListener(DownloadLogsCSV);
        prevReportBtn.onClick.AddListener(() => ChangeReportPage(-1));
        nextReportBtn.onClick.AddListener(() => ChangeReportPage(1));

        // Settings sliders listeners
        arrivalRateSlider.onValueChanged.AddListener(v => { controller.gameConfig.arrivalRate = v; arrivalRateValue.text = v.ToString("F0"); });
        citizenPercentageSlider.onValueChanged.AddListener(v => { controller.gameConfig.citizenPercentage = (int)v; citizenPercentageValue.text = v.ToString("F0"); });
        walkingSpeedSlider.onValueChanged.AddListener(v => { controller.gameConfig.baseWalkingSpeed = v; walkingSpeedValue.text = v.ToString("F0"); });
        securityLanesSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityLanes = (int)v; controller.ResetSimulation(); securityLanesValue.text = v.ToString("F0"); });
        securityTimeSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityProcessingTime = v; securityTimeValue.text = v.ToString("F0"); });
        // Add other sliders...

        distributionDropdown.onValueChanged.AddListener(idx => { controller.gameConfig.arrivalDistribution = idx == 0 ? "poisson" : "fixed"; });

        // Load scenario buttons
        foreach (var scenario in controller.scenarios)
        {
            // Dynamically create buttons or reference existing ones
        }
    }

    void Update()
    {
        if (!controller.isPaused)
        {
            playPauseIcon.sprite = pauseIcon;
        }
        else
        {
            playPauseIcon.sprite = playIcon;
        }
    }

    public void UpdateMetricsUI()
    {
        // Time display
        int totalSecs = Mathf.FloorToInt(controller.simulationTime);
        int hours = (totalSecs / 3600) % 24;
        int mins = (totalSecs % 3600) / 60;
        int secs = totalSecs % 60;
        simTimeText.text = $"{hours:D2}:{mins:D2}:{secs:D2}";

        securityQueueText.text = controller.metrics.securityQueueLength.ToString();
        immigrationQueueText.text = controller.metrics.immigrationQueueLength.ToString();

        float avgSecWait = controller.metrics.securityProcessedCount > 0 ?
            (controller.metrics.totalSecurityWaitTime / controller.metrics.securityProcessedCount / 60f) : 0;
        secWaitText.text = avgSecWait.ToString("F1");

        float avgImmWait = controller.metrics.immigrationProcessedCount > 0 ?
            (controller.metrics.totalImmigrationWaitTime / controller.metrics.immigrationProcessedCount / 60f) : 0;
        immWaitText.text = avgImmWait.ToString("F1");

        float avgWait = controller.metrics.totalProcessed > 0 ?
            (controller.metrics.totalWaitTime / controller.metrics.totalProcessed / 60f) : 0;
        avgWaitText.text = avgWait.ToString("F1");

        arrivalFlowText.text = controller.metrics.measuredArrivalRate.ToString("F1");
        throughputText.text = controller.metrics.throughputPerHour.ToString();
        manHoursText.text = controller.metrics.totalManHours.ToString("F1");
    }

    private void TogglePlayPause()
    {
        controller.isPaused = !controller.isPaused;
    }

    private void CycleSpeed()
    {
        float[] speeds = { 1f, 2f, 5f, 10f, 20f, 50f, 100f, 200f, 500f };
        int currentIdx = Array.IndexOf(speeds, controller.simulationSpeed);
        currentIdx = (currentIdx + 1) % speeds.Length;
        controller.simulationSpeed = speeds[currentIdx];
        speedDisplay.text = $"{controller.simulationSpeed}x";
    }

    public void ShowDayReport()
    {
        controller.isPaused = true;
        currentReportPage = 1;
        UpdateReportUI();
        reportPanel.SetActive(true);
    }

    private void UpdateReportUI()
    {
        reportPage1.SetActive(currentReportPage == 1);
        reportPage2.SetActive(currentReportPage == 2);
        reportPage3.SetActive(currentReportPage == 3);

        prevReportBtn.gameObject.SetActive(currentReportPage > 1);
        nextReportBtn.gameObject.SetActive(currentReportPage < 3);

        if (currentReportPage == 1)
        {
            statTotalTravelers.text = controller.metrics.totalProcessed.ToString();
            float totalHours = controller.simulationTime / 3600f;
            float avgThroughput = totalHours > 0 ? controller.metrics.totalProcessed / totalHours : 0;
            statAvgThroughput.text = $"{avgThroughput:F1}/h";
            statAvgSystem.text = controller.metrics.totalProcessed > 0 ?
                (controller.metrics.totalWaitTime / controller.metrics.totalProcessed / 60f).ToString("F1") + "m" : "0.0m";
            statAvgSecurity.text = controller.metrics.securityProcessedCount > 0 ?
                (controller.metrics.totalSecurityWaitTime / controller.metrics.securityProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statAvgImmigration.text = controller.metrics.immigrationProcessedCount > 0 ?
                (controller.metrics.totalImmigrationWaitTime / controller.metrics.immigrationProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statPeakSec.text = controller.metrics.peakSecurityQueue.ToString();
            statPeakImm.text = controller.metrics.peakImmigrationQueue.ToString();
        }
        // Add page 2 and 3 similarly...
    }

    private void ChangeReportPage(int delta)
    {
        currentReportPage += delta;
        currentReportPage = Mathf.Clamp(currentReportPage, 1, 3);
        UpdateReportUI();
    }

    private void ShowLogs()
    {
        // Clear existing entries
        foreach (Transform child in logContent) Destroy(child.gameObject);

        foreach (var log in controller.travelerLogs)
        {
            GameObject entry = Instantiate(logEntryPrefab, logContent);
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            texts[0].text = log.id.ToString();
            texts[1].text = log.type;
            texts[2].text = log.isCitizen ? "Yes" : "No";
            texts[3].text = FormatTime(log.arrivalTime);
            texts[4].text = FormatTime(log.securityStartTime);
            texts[5].text = FormatTime(log.securityEndTime);
            texts[6].text = FormatTime(log.immigrationStartTime);
            texts[7].text = FormatTime(log.immigrationEndTime);
            texts[8].text = FormatTime(log.exitTime);
        }

        logsPanel.SetActive(true);
    }

    private void DownloadLogsCSV()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ID,Type,Citizen,Arrival,Security Start,Security End,Immigration Start,Immigration End,Exit");
        foreach (var log in controller.travelerLogs)
        {
            sb.AppendLine($"{log.id},{log.type},{(log.isCitizen ? "Yes" : "No")},{FormatTime(log.arrivalTime)},{FormatTime(log.securityStartTime)},{FormatTime(log.securityEndTime)},{FormatTime(log.immigrationStartTime)},{FormatTime(log.immigrationEndTime)},{FormatTime(log.exitTime)}");
        }

        string path = Application.persistentDataPath + $"/checkpoint_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"Logs saved to {path}");
    }

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