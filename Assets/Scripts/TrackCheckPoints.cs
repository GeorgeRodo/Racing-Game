using UnityEngine;
using System.Collections.Generic;
using System;

public class TrackCheckPoints : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;
    
    private List<CheckpointSingle> checkpointSingleList = new List<CheckpointSingle>();
    private int nextCheckpointIndex = 0;
    
    private void Awake()
    {
        foreach (Transform checkpointSingleTransform in transform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            
            if (checkpointSingle != null)
            {
                checkpointSingle.SetTrackCheckPoints(this);
                checkpointSingleList.Add(checkpointSingle);
            }
        }
    }

    private void Start()
    {
        // Show the first two checkpoints at the start
        UpdateVisibleCheckpoints();
    }
    
    public void PlayerThroughCheckPoint(CheckpointSingle checkpointSingle)
    {
        int checkpointIndex = checkpointSingleList.IndexOf(checkpointSingle);
        
        if (checkpointIndex == nextCheckpointIndex)
        {
            Debug.Log($"CORRECT checkpoint {checkpointSingle.name}");
            
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpointSingleList.Count;
            
            // Update which checkpoints are visible
            UpdateVisibleCheckpoints();

            OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Debug.Log($"WRONG checkpoint! Expected {nextCheckpointIndex}, got {checkpointIndex}");
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateVisibleCheckpoints()
    {
        // Hide all checkpoints first
        foreach (var checkpoint in checkpointSingleList)
        {
            checkpoint.Hide();
        }

        // Show next checkpoint
        checkpointSingleList[nextCheckpointIndex].Show();

        // Show checkpoint after next (wrapping around if needed)
        int nextPlusOneIndex = (nextCheckpointIndex + 1) % checkpointSingleList.Count;
        checkpointSingleList[nextPlusOneIndex].Show();
    }
    
    public int GetNextCheckpointIndex()
    {
        return nextCheckpointIndex;
    }
    
    public int GetTotalCheckpoints()
    {
        return checkpointSingleList.Count;
    }
}