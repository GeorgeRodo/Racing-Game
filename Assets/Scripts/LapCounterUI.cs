using UnityEngine;
using TMPro;

public class LapCounterUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private TextMeshProUGUI lapText;

    private void Start()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint += UpdateLapDisplay;
        trackCheckPoints.OnLapCompleted += UpdateLapDisplay;
        
        UpdateLapDisplay(null, System.EventArgs.Empty);
        
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