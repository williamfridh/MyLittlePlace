using UnityEngine;

public class TimeManagerScript : MonoBehaviour
{

    public static TimeManagerScript Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("How many real-world seconds per in-game minutes")]
    public float inGameSecondsPerRealSecond = 60.0f;

    [Header("UI Elements")]
    public RectTransform clockHand; // Assign in Inspector: the hand of the clock to rotate

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

    void Start()
    {
        // Initilize time to 12:00
        totalInGameSeconds = SECONDS_PER_HOUR * 12; // Start at 12:00
    }

    void Update()
    {
        // Convert real seconds to in-game seconds
        // (e.g., 1 real second = 60 in-game seconds)
        float gameSecondsPassed = Time.deltaTime * inGameSecondsPerRealSecond;
        totalInGameSeconds += gameSecondsPassed;
        //Debug.Log("In-game time: " + GetFormattedTime() + " (Day " + GetCurrentDay() + ")" + " - Day/Night Ratio: " + GetTimeNightRatio().ToString("F2"));
        // Rotate clock hand
        if (clockHand != null)
        {
            float rotationAngle = (totalInGameSeconds % SECONDS_PER_DAY) / SECONDS_PER_DAY * 360f - 135f; // -135 to start at 12:00 position
            clockHand.localRotation = Quaternion.Euler(0, 0, -rotationAngle); // Rotate counter-clockwise
        }
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
        // Calculate the ratio of the current time to the full day (0.0 at 00:00, 0.5 at 12:00, 1.0 at 24:00)
        return (totalInGameSeconds % SECONDS_PER_DAY) / SECONDS_PER_DAY;
    }

    public string GetFormattedTime()
    {
        int hour = GetCurrentHour();
        int minute = GetCurrentMinute();
        return string.Format("{0:00}:{1:00}", hour, minute);
    }
}
