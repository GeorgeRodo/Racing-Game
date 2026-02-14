using UnityEngine;
using TMPro;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;

    [Header("Audio Settings")]
    public AudioSource raceMusic; 

    [Header("Vehicle Reference")]
    public CustomVehicleController vehicleController; // Το script του αυτοκινήτου

    [Header("Track Reference")]
    public TrackCheckPoints trackCheckPoints;

    private bool raceStarted = false;
    private float raceTime = 0f;

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        // Απενεργοποίηση κίνησης στην αρχή
        if (vehicleController != null)
            vehicleController.enabled = false;

        // Βεβαιώσου ότι η μουσική δεν παίζει από πριν
        if (raceMusic != null)
            raceMusic.Stop();

        StartCoroutine(CountdownSequence());
    }

    void Update()
    {
        if (raceStarted && trackCheckPoints != null && !trackCheckPoints.IsRaceFinished())
        {
            raceTime += Time.deltaTime;
        }
    }

    IEnumerator CountdownSequence()
    {
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.color = Color.white;
                countdownText.fontSize = 120;
            }
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.green;
        }

        StartRace();

        yield return new WaitForSeconds(1.5f);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    void StartRace()
    {
        raceStarted = true;

        // Έναρξη μουσικής
        if (raceMusic != null)
            raceMusic.Play();

        // Ενεργοποίηση κίνησης
        if (vehicleController != null)
            vehicleController.enabled = true;
    }

    public bool IsRaceStarted() { return raceStarted; }
    public float GetRaceTime() { return raceTime; }
}