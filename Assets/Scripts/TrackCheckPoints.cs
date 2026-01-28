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
    
    public void PlayerThroughCheckPoint(CheckpointSingle checkpointSingle)
    {
        int checkpointIndex = checkpointSingleList.IndexOf(checkpointSingle);
        
        if (checkpointIndex == nextCheckpointIndex)
        {
            Debug.Log($"CORRECT checkpoint {checkpointSingle.name}");
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointIndex];
            correctCheckpointSingle.Hide();
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpointSingleList.Count;
            OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Debug.Log($"WRONG checkpoint! Expected {nextCheckpointIndex}, got {checkpointIndex}");
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointIndex];
            correctCheckpointSingle.Show();
        }
    }
    
    // Add these public methods
    public int GetNextCheckpointIndex()
    {
        return nextCheckpointIndex;
    }
    
    public int GetTotalCheckpoints()
    {
        return checkpointSingleList.Count;
    }
}