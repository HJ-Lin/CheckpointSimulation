using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
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
    public Button exitBtn;
    public TMP_Text speedDisplay;
    public Image playPauseIcon;
    public Sprite playIcon;
    public Sprite pauseIcon;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject reportPanel;
    public GameObject logsPanel;
    public GameObject exitPanel;

    // ---------- Settings Panel Elements ----------
    [Header("Scenario Buttons")]
    public Toggle baselineScenarioBtn;
    public Toggle holidayScenarioBtn;
    public Toggle optimizedScenarioBtn;
    public Toggle wetWeatherScenarioBtn;

    [Header("Traveler Types")]
    public Slider standardTypeSlider;
    public TMP_Text standardTypeValue;
    public Slider businessTypeSlider;
    public TMP_Text businessTypeValue;
    public Slider familyTypeSlider;
    public TMP_Text familyTypeValue;

    [Header("Time Multipliers")]
    public Slider[] timeMultiplierSliders = new Slider[8];
    public TMP_Text[] timeMultiplierValues = new TMP_Text[8];

    [Header("Arrival Settings")]
    public Slider arrivalRateSlider;
    public TMP_Text arrivalRateValue;
    public TMP_Dropdown distributionDropdown;
    public Slider citizenPercentageSlider;
    public TMP_Text citizenPercentageValue;
    public Slider walkingSpeedSlider;
    public TMP_Text walkingSpeedValue;

    [Header("Security Configuration")]
    public Slider securityLanesSlider;
    public TMP_Text securityLanesValue;
    public Slider securityTimeSlider;
    public TMP_Text securityTimeValue;
    public Slider securityMinSlider;
    public TMP_Text securityMinValue;
    public Slider securityMaxSlider;
    public TMP_Text securityMaxValue;
    public Slider securityEnhancedProbSlider;
    public TMP_Text securityEnhancedProbValue;

    [Header("Immigration Configuration")]
    public Slider immigrationCountersSlider;
    public TMP_Text immigrationCountersValue;
    public TMP_Dropdown[] immigrationLaneDropdowns = new TMP_Dropdown[4];
    public Slider immigrationTimeSlider;
    public TMP_Text immigrationTimeValue;
    public Slider immigrationMinSlider;
    public TMP_Text immigrationMinValue;
    public Slider immigrationMaxSlider;
    public TMP_Text immigrationMaxValue;
    public Slider automatedTimeSlider;
    public TMP_Text automatedTimeValue;
    public Slider automatedMinSlider;
    public TMP_Text automatedMinValue;
    public Slider automatedMaxSlider;
    public TMP_Text automatedMaxValue;
    public Slider automatedErrorSlider;
    public TMP_Text automatedErrorValue;
    public Slider enhancedProbSlider;
    public TMP_Text enhancedProbValue;

    // ---------- Report Panel ----------
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

    // Report page 2 elements
    public TMP_Text statPeakThroughput;
    public TMP_Text statPeakAvgSystem;
    public TMP_Text statPeakAvgSec;
    public TMP_Text statPeakAvgImm;
    public TMP_Text statManHours;

    // Report page 3 elements
    public RectTransform utilizationTableContent;
    public GameObject utilizationEntryPrefab; // Prefab with 3 TMP_Texts: Counter, HoursOpen, Utilization%

    // ---------- Logs Panel ----------
    [Header("Logs Panel")]
    public RectTransform logContent;
    public GameObject logEntryPrefab;
    public Button closeLogsBtn;
    public Button downloadLogsBtn;
    [SerializeField] private int logEntryPoolSize = 50;

    // ---------- Exit Panel ----------
    [Header("Exit Panel")]
    public Button confirmExitBtn;

    private SimulationController controller;
    private int currentReportPage = 1;
    private List<LogEntryUI> activeLogEntries = new List<LogEntryUI>();
    private float[] speeds = { 1f, 2f, 5f, 10f, 20f, 50f, 100f, 200f, 500f };
    private int currentSpeedIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Init()
    {
        controller = SimulationController.Instance;

        // Initialize log entry object pool
        ObjectPoolManager.Instance.InitializePool("LogEntries", logEntryPrefab, logEntryPoolSize, logContent);

        // Bottom bar listeners
        playPauseBtn.onClick.AddListener(TogglePlayPause);
        resetBtn.onClick.AddListener(() => controller.ResetSimulation());
        speedBtn.onClick.AddListener(CycleSpeed);
        settingsBtn.onClick.AddListener(() => settingsPanel.SetActive(true));
        analysisBtn.onClick.AddListener(ShowDayReport);
        logsBtn.onClick.AddListener(ShowLogs);
        exitBtn.onClick.AddListener(ShowExitPanel);

        // Report panel listeners
        closeReportBtn.onClick.AddListener(() => reportPanel.SetActive(false));
        prevReportBtn.onClick.AddListener(() => ChangeReportPage(-1));
        nextReportBtn.onClick.AddListener(() => ChangeReportPage(1));

        // Logs panel listeners
        closeLogsBtn.onClick.AddListener(() => logsPanel.SetActive(false));
        downloadLogsBtn.onClick.AddListener(DownloadLogsCSV);

        // Exit panel listeners
        confirmExitBtn.onClick.AddListener(ExitApplication);

        // Scenario buttons
        baselineScenarioBtn.onValueChanged.AddListener((value) => ApplyScenario("baseline", value));
        holidayScenarioBtn.onValueChanged.AddListener((value) => ApplyScenario("holiday", value));
        optimizedScenarioBtn.onValueChanged.AddListener((value) => ApplyScenario("optimized", value));
        wetWeatherScenarioBtn.onValueChanged.AddListener((value) => ApplyScenario("wet_weather", value));

        // Traveler type sliders
        standardTypeSlider.onValueChanged.AddListener(v => BalanceTravelerTypes("standard", Mathf.RoundToInt(v)));
        businessTypeSlider.onValueChanged.AddListener(v => BalanceTravelerTypes("business", Mathf.RoundToInt(v)));
        familyTypeSlider.onValueChanged.AddListener(v => BalanceTravelerTypes("family", Mathf.RoundToInt(v)));

        // Time multiplier sliders
        for (int i = 0; i < timeMultiplierSliders.Length; i++)
        {
            int idx = i;
            timeMultiplierSliders[i].onValueChanged.AddListener(v => UpdateTimeMultiplier(idx, v));
        }

        // Arrival settings
        arrivalRateSlider.onValueChanged.AddListener(v => { controller.gameConfig.arrivalRate = v; arrivalRateValue.text = v.ToString("F0"); });
        distributionDropdown.onValueChanged.AddListener(idx => controller.gameConfig.arrivalDistribution = idx == 0 ? "poisson" : "fixed");
        citizenPercentageSlider.onValueChanged.AddListener(v => { controller.gameConfig.citizenPercentage = Mathf.RoundToInt(v); citizenPercentageValue.text = v.ToString("F0"); });
        walkingSpeedSlider.onValueChanged.AddListener(v => { controller.gameConfig.baseWalkingSpeed = v; walkingSpeedValue.text = v.ToString("F0"); });

        // Security
        securityLanesSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityLanes = Mathf.RoundToInt(v); securityLanesValue.text = v.ToString("F0"); controller.ResetSimulation(); });
        securityTimeSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityProcessingTime = v; securityTimeValue.text = v.ToString("F0"); });
        securityMinSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityMinTime = v; securityMinValue.text = v.ToString("F0"); });
        securityMaxSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityMaxTime = v; securityMaxValue.text = v.ToString("F0"); });
        securityEnhancedProbSlider.onValueChanged.AddListener(v => { controller.gameConfig.securityEnhancedProb = v / 100f; securityEnhancedProbValue.text = v.ToString("F0"); });

        // Immigration
        immigrationCountersSlider.onValueChanged.AddListener(v => { controller.gameConfig.immigrationCounters = Mathf.RoundToInt(v); immigrationCountersValue.text = v.ToString("F0"); controller.ResetSimulation(); });
        for (int i = 0; i < immigrationLaneDropdowns.Length; i++)
        {
            int idx = i;
            immigrationLaneDropdowns[i].onValueChanged.AddListener(val => UpdateImmigrationLaneSetting(idx, val));
        }
        immigrationTimeSlider.onValueChanged.AddListener(v => { controller.gameConfig.immigrationProcessingTime = v; immigrationTimeValue.text = v.ToString("F0"); });
        immigrationMinSlider.onValueChanged.AddListener(v => { controller.gameConfig.immigrationMinTime = v; immigrationMinValue.text = v.ToString("F0"); });
        immigrationMaxSlider.onValueChanged.AddListener(v => { controller.gameConfig.immigrationMaxTime = v; immigrationMaxValue.text = v.ToString("F0"); });
        automatedTimeSlider.onValueChanged.AddListener(v => { controller.gameConfig.automatedProcessingTime = v; automatedTimeValue.text = v.ToString("F0"); });
        automatedMinSlider.onValueChanged.AddListener(v => { controller.gameConfig.automatedMinTime = v; automatedMinValue.text = v.ToString("F0"); });
        automatedMaxSlider.onValueChanged.AddListener(v => { controller.gameConfig.automatedMaxTime = v; automatedMaxValue.text = v.ToString("F0"); });
        automatedErrorSlider.onValueChanged.AddListener(v => { controller.gameConfig.automatedErrorProb = v / 100f; automatedErrorValue.text = v.ToString("F0"); });
        enhancedProbSlider.onValueChanged.AddListener(v => { controller.gameConfig.enhancedScreeningProb = v / 100f; enhancedProbValue.text = v.ToString("F0"); });

        // Initialize UI from current config
        UpdateUIFromConfig();

        LaneWaypointHolder.OnImmigrationLaneChanged += OnImmigrationLaneChanged;
    }

    private void OnDestroy()
    {
        LaneWaypointHolder.OnImmigrationLaneChanged -= OnImmigrationLaneChanged;
    }

    void Update()
    {
        // Update play/pause icon
        playPauseIcon.sprite = controller.isPaused ? playIcon : pauseIcon;
    }

    public void UpdateMetricsUI()
    {
        // Time display (24h format)
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
        currentSpeedIndex = (currentSpeedIndex + 1) % speeds.Length;
        controller.simulationSpeed = speeds[currentSpeedIndex];
        speedDisplay.text = $"{controller.simulationSpeed}x";
    }

    private void ApplyScenario(string scenarioId, bool isOn)
    {
        if (!isOn)
            return;
        var scenario = controller.scenarios.Find(s => s.id == scenarioId);
        if (scenario != null)
        {
            controller.ApplyScenario(scenario);
            UpdateUIFromConfig();
        }
    }

    private void BalanceTravelerTypes(string changedKey, int newWeight)
    {
        var types = controller.gameConfig.travelerTypes;
        if (!types.ContainsKey(changedKey)) return;

        int oldWeight = types[changedKey].weight;
        int diff = newWeight - oldWeight;
        if (diff == 0) return;

        types[changedKey].weight = newWeight;

        List<string> otherKeys = new List<string>();
        foreach (var k in types.Keys)
            if (k != changedKey) otherKeys.Add(k);

        if (diff > 0) // increased this type -> decrease others proportionally
        {
            int totalOthers = 0;
            foreach (var k in otherKeys) totalOthers += types[k].weight;
            if (totalOthers > 0)
            {
                foreach (var k in otherKeys)
                {
                    float share = (float)types[k].weight / totalOthers;
                    int reduction = Mathf.RoundToInt(diff * share);
                    types[k].weight = Mathf.Max(0, types[k].weight - reduction);
                }
            }
        }
        else // decreased this type -> increase others proportionally to reach 100
        {
            int totalOthers = 0;
            foreach (var k in otherKeys) totalOthers += types[k].weight;
            int toDistribute = -diff; // positive
            if (totalOthers > 0)
            {
                foreach (var k in otherKeys)
                {
                    float share = (float)types[k].weight / totalOthers;
                    int add = Mathf.RoundToInt(toDistribute * share);
                    types[k].weight += add;
                }
            }
            else
            {
                // distribute equally if all others zero
                int addEach = toDistribute / otherKeys.Count;
                foreach (var k in otherKeys)
                    types[k].weight += addEach;
            }
        }

        // Ensure sum is exactly 100
        int sum = 0;
        foreach (var k in types.Keys) sum += types[k].weight;
        if (sum != 100)
        {
            int correction = 100 - sum;
            types[otherKeys[0]].weight += correction;
        }

        // Update UI
        standardTypeSlider.value = types["standard"].weight;
        standardTypeValue.text = types["standard"].weight.ToString();
        businessTypeSlider.value = types["business"].weight;
        businessTypeValue.text = types["business"].weight.ToString();
        familyTypeSlider.value = types["family"].weight;
        familyTypeValue.text = types["family"].weight.ToString();
    }

    private void UpdateTimeMultiplier(int index, float value)
    {
        controller.gameConfig.timeMultipliers[index] = value;
        if (timeMultiplierValues[index] != null)
            timeMultiplierValues[index].text = value.ToString("F1");
    }

    private void UpdateImmigrationLaneSetting(int laneIndex, int dropdownValue)
    {
        string setting = dropdownValue == 0 ? "all" : (dropdownValue == 1 ? "citizens" : "foreigners");
        controller.gameConfig.immigrationLaneSettings[laneIndex] = setting;
    }

    private void UpdateUIFromConfig()
    {
        var cfg = controller.gameConfig;

        // Traveler types
        standardTypeSlider.value = cfg.travelerTypes["standard"].weight;
        standardTypeValue.text = cfg.travelerTypes["standard"].weight.ToString();
        businessTypeSlider.value = cfg.travelerTypes["business"].weight;
        businessTypeValue.text = cfg.travelerTypes["business"].weight.ToString();
        familyTypeSlider.value = cfg.travelerTypes["family"].weight;
        familyTypeValue.text = cfg.travelerTypes["family"].weight.ToString();

        // Time multipliers
        for (int i = 0; i < timeMultiplierSliders.Length; i++)
        {
            if (timeMultiplierSliders[i] != null)
            {
                timeMultiplierSliders[i].value = cfg.timeMultipliers[i];
                if (timeMultiplierValues[i] != null)
                    timeMultiplierValues[i].text = cfg.timeMultipliers[i].ToString("F1");
            }
        }

        // Arrival
        arrivalRateSlider.value = cfg.arrivalRate;
        arrivalRateValue.text = cfg.arrivalRate.ToString("F0");
        distributionDropdown.value = cfg.arrivalDistribution == "poisson" ? 0 : 1;
        citizenPercentageSlider.value = cfg.citizenPercentage;
        citizenPercentageValue.text = cfg.citizenPercentage.ToString();
        walkingSpeedSlider.value = cfg.baseWalkingSpeed;
        walkingSpeedValue.text = cfg.baseWalkingSpeed.ToString("F0");

        // Security
        securityLanesSlider.value = cfg.securityLanes;
        securityLanesValue.text = cfg.securityLanes.ToString();
        securityTimeSlider.value = cfg.securityProcessingTime;
        securityTimeValue.text = cfg.securityProcessingTime.ToString("F0");
        securityMinSlider.value = cfg.securityMinTime;
        securityMinValue.text = cfg.securityMinTime.ToString("F0");
        securityMaxSlider.value = cfg.securityMaxTime;
        securityMaxValue.text = cfg.securityMaxTime.ToString("F0");
        securityEnhancedProbSlider.value = cfg.securityEnhancedProb * 100f;
        securityEnhancedProbValue.text = (cfg.securityEnhancedProb * 100f).ToString("F0");

        // Immigration
        immigrationCountersSlider.value = cfg.immigrationCounters;
        immigrationCountersValue.text = cfg.immigrationCounters.ToString();
        for (int i = 0; i < immigrationLaneDropdowns.Length; i++)
        {
            string setting = cfg.immigrationLaneSettings[i];
            int val = setting == "all" ? 0 : (setting == "citizens" ? 1 : 2);
            immigrationLaneDropdowns[i].value = val;
        }
        immigrationTimeSlider.value = cfg.immigrationProcessingTime;
        immigrationTimeValue.text = cfg.immigrationProcessingTime.ToString("F0");
        immigrationMinSlider.value = cfg.immigrationMinTime;
        immigrationMinValue.text = cfg.immigrationMinTime.ToString("F0");
        immigrationMaxSlider.value = cfg.immigrationMaxTime;
        immigrationMaxValue.text = cfg.immigrationMaxTime.ToString("F0");
        automatedTimeSlider.value = cfg.automatedProcessingTime;
        automatedTimeValue.text = cfg.automatedProcessingTime.ToString("F0");
        automatedMinSlider.value = cfg.automatedMinTime;
        automatedMinValue.text = cfg.automatedMinTime.ToString("F0");
        automatedMaxSlider.value = cfg.automatedMaxTime;
        automatedMaxValue.text = cfg.automatedMaxTime.ToString("F0");
        automatedErrorSlider.value = cfg.automatedErrorProb * 100f;
        automatedErrorValue.text = (cfg.automatedErrorProb * 100f).ToString("F0");
        enhancedProbSlider.value = cfg.enhancedScreeningProb * 100f;
        enhancedProbValue.text = (cfg.enhancedScreeningProb * 100f).ToString("F0");
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

        var metrics = controller.metrics;
        var cfg = controller.gameConfig;

        // Page 1: Overview
        if (currentReportPage == 1)
        {
            reportScenarioName.text = $"Scenario: {cfg.displayName}";
            statTotalTravelers.text = metrics.totalProcessed.ToString();
            float totalHours = controller.simulationTime / 3600f;
            float avgThroughput = totalHours > 0 ? metrics.totalProcessed / totalHours : 0;
            statAvgThroughput.text = $"{avgThroughput:F1}/h";
            statAvgSystem.text = metrics.totalProcessed > 0 ?
                (metrics.totalWaitTime / metrics.totalProcessed / 60f).ToString("F1") + "m" : "0.0m";
            statAvgSecurity.text = metrics.securityProcessedCount > 0 ?
                (metrics.totalSecurityWaitTime / metrics.securityProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statAvgImmigration.text = metrics.immigrationProcessedCount > 0 ?
                (metrics.totalImmigrationWaitTime / metrics.immigrationProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statPeakSec.text = metrics.peakSecurityQueue.ToString();
            statPeakImm.text = metrics.peakImmigrationQueue.ToString();
        }
        // Page 2: Peak Performance
        else if (currentReportPage == 2)
        {
            int peakHourCount = 0;
            for (int h = 0; h < 24; h++)
                if (controller.GetTimeMultiplier(h * 3600f) >= 2.0f) peakHourCount++;
            float peakThroughput = peakHourCount > 0 ? metrics.peakTotalProcessed / (float)peakHourCount : 0;
            statPeakThroughput.text = $"{peakThroughput:F1}/h";
            statPeakAvgSystem.text = metrics.peakTotalProcessed > 0 ?
                (metrics.peakTotalWaitTime / metrics.peakTotalProcessed / 60f).ToString("F1") + "m" : "0.0m";
            statPeakAvgSec.text = metrics.peakSecurityProcessedCount > 0 ?
                (metrics.peakTotalSecurityWaitTime / metrics.peakSecurityProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statPeakAvgImm.text = metrics.peakImmigrationProcessedCount > 0 ?
                (metrics.peakTotalImmigrationWaitTime / metrics.peakImmigrationProcessedCount / 60f).ToString("F1") + "m" : "0.0m";
            statManHours.text = metrics.totalManHours.ToString("F1");
        }
        // Page 3: Utilization
        else if (currentReportPage == 3)
        {
            // Clear old entries
            foreach (Transform child in utilizationTableContent)
                Destroy(child.gameObject);

            // Add security counters
            foreach (var c in controller.securityCounters)
            {
                GameObject entry = Instantiate(utilizationEntryPrefab, utilizationTableContent);
                var texts = entry.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"Sec {c.id + 1}";
                    texts[1].text = (c.activeTime / 3600f).ToString("F2") + "h";
                    float util = c.activeTime > 0 ? (c.busyTime / c.activeTime) * 100f : 0f;
                    texts[2].text = util.ToString("F1") + "%";
                    texts[2].color = GetUtilizationColor(util);
                }
            }
            // Add immigration counters
            foreach (var c in controller.immigrationCounters)
            {
                GameObject entry = Instantiate(utilizationEntryPrefab, utilizationTableContent);
                var texts = entry.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"Imm {c.id + 1} ({c.type})";
                    texts[1].text = (c.activeTime / 3600f).ToString("F2") + "h";
                    float util = c.activeTime > 0 ? (c.busyTime / c.activeTime) * 100f : 0f;
                    texts[2].text = util.ToString("F1") + "%";
                    texts[2].color = GetUtilizationColor(util);
                }
            }
        }
    }

    private Color GetUtilizationColor(float percent)
    {
        if (percent < 30f) return Color.green;
        if (percent < 60f) return Color.yellow;
        if (percent < 85f) return new Color(1f, 0.5f, 0f); // orange
        return Color.red;
    }

    private void ChangeReportPage(int delta)
    {
        currentReportPage += delta;
        currentReportPage = Mathf.Clamp(currentReportPage, 1, 3);
        UpdateReportUI();
    }

    private void ShowLogs()
    {
        // Return all previously active log entries to the pool
        foreach (var entry in activeLogEntries)
        {
            if (entry != null)
            {
                ObjectPoolManager.Instance.ReturnToPool("LogEntries", entry);
            }
        }
        activeLogEntries.Clear();

        // Create new log entries from pool
        foreach (var log in controller.travelerLogs)
        {
            IPoolable poolable = ObjectPoolManager.Instance.GetFromPool("LogEntries");
            if (poolable != null)
            {
                LogEntryUI logEntry = poolable as LogEntryUI;
                if (logEntry != null)
                {
                    logEntry.DisplayLog(log);
                    activeLogEntries.Add(logEntry);
                    logEntry.transform.SetAsLastSibling();
                }
            }
        }

        logsPanel.SetActive(true);
    }

    private void DownloadLogsCSV()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ID,Type,Citizen,Arrival,Security Start,Security End,Immigration Start,Immigration End,Exit");
        foreach (var log in controller.travelerLogs)
        {
            sb.AppendLine($"{log.id},{log.type},{(log.isCitizen ? "Yes" : "No")}," +
                $"{FormatTime(log.arrivalTime)},{FormatTime(log.securityStartTime)},{FormatTime(log.securityEndTime)}," +
                $"{FormatTime(log.immigrationStartTime)},{FormatTime(log.immigrationEndTime)},{FormatTime(log.exitTime)}");
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

    private void OnImmigrationLaneChanged(LaneWaypointHolder lane, string type)
    {
        if (lane == null || lane.laneIndex < 0 || lane.laneIndex >= immigrationLaneDropdowns.Length)
            return;

        int dropdownValue = type == "all" ? 0 : (type == "citizens" ? 1 : 2);
        immigrationLaneDropdowns[lane.laneIndex].value = dropdownValue;
    }

    private void ShowExitPanel()
    {
        exitPanel.SetActive(true);
    }

    private void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }
}