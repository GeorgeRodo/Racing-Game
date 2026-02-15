using UnityEngine;
using TMPro;

public class LapCounterUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private TextMeshProUGUI lapText;

    private void Start()
    {
        // Subscribe to events
        trackCheckPoints.OnPlayerCorrectCheckpoint += UpdateLapDisplay;
        trackCheckPoints.OnLapCompleted += UpdateLapDisplay;
        
        // Update immediately
        UpdateLapDisplay(null, System.EventArgs.Empty);
        
        // Note: RaceManager will handle showing/hiding this UI during countdown
    }

    private void UpdateLapDisplay(object sender, System.EventArgs e)
    {
        int currentLap = trackCheckPoints.GetCurrentLap();
        int totalLaps = trackCheckPoints.GetTotalLaps();
        
        lapText.text = $"Lap: {currentLap}/{totalLaps}";
    }
    
    private void OnDestroy()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint -= UpdateLapDisplay;
        trackCheckPoints.OnLapCompleted -= UpdateLapDisplay;
    }
}