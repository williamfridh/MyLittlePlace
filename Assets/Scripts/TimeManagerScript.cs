using UnityEngine;

public class TimeManagerScript : MonoBehaviour
{

    public static TimeManagerScript Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("How many real-world seconds per in-game minutes")]
    public float secondsPerMinute = 1.0f;

    private float totalInGameSeconds = 0f;

    // Constants for cleaner time calcaulations
    private const int SECONDS_PER_MINUTE = 60;
    private const int MINUTES_PER_HOUR = 60;
    private const int HOURS_PER_DAY = 24;
    private const int SECONDS_PER_HOUR = SECONDS_PER_MINUTE * MINUTES_PER_HOUR;
    private const int SECONDS_PER_DAY = SECONDS_PER_HOUR * HOURS_PER_DAY;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        float gameSecondsPassed = Time.deltaTime * (SECONDS_PER_MINUTE / secondsPerMinute);
        totalInGameSeconds += gameSecondsPassed;
        Debug.Log("In-game time: " + GetFormattedTime() + " (Day " + GetCurrentDay() + ")" + " - Day/Night Ratio: " + GetTimeNightRatio().ToString("F2"));
    }

    public int GetCurrentDay()
    {
        return (int)(totalInGameSeconds / SECONDS_PER_DAY) + 1; // +1 to start from Day 1
    }

    public int GetCurrentHour()
    {
        return (int)((totalInGameSeconds % SECONDS_PER_DAY) / SECONDS_PER_HOUR);
    }

    public int GetCurrentMinute()
    {
        return (int)((totalInGameSeconds % SECONDS_PER_HOUR) / SECONDS_PER_MINUTE);
    }

    public float GetTimeNightRatio()
    {
        return (totalInGameSeconds % SECONDS_PER_DAY) / SECONDS_PER_DAY;
    }

    public string GetFormattedTime()
    {
        int hour = GetCurrentHour();
        int minute = GetCurrentMinute();
        return string.Format("{0:00}:{1:00}", hour, minute);
    }
}
