using UnityEngine;

public class TrackCheckpointsUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;

    private void Start()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        trackCheckPoints.OnPlayerWrongCheckpoint += TrackCheckPoints_OnPlayerWrongCheckpoint;
        Hide();
    }

    private void TrackCheckPoints_OnPlayerWrongCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("Player hit wrong checkpoint!");
        Show();
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("Player hit correct checkpoint!");
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}