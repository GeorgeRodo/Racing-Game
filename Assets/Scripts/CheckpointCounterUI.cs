using UnityEngine;
using TMPro;

public class CheckpointCounterUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private TextMeshProUGUI checkpointText;

    private void Start()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        
        UpdateCheckpointText();
        
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        UpdateCheckpointText();
    }

    private void UpdateCheckpointText()
    {
        int currentCheckpoint = trackCheckPoints.GetNextCheckpointIndex();
        int totalCheckpoints = trackCheckPoints.GetTotalCheckpoints();
        
        checkpointText.text = $"{currentCheckpoint}/{totalCheckpoints}";
    }
    
    private void OnDestroy()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint -= TrackCheckPoints_OnPlayerCorrectCheckpoint;
    }
}