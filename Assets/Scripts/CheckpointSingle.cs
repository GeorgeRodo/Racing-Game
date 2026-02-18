using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckPoints trackCheckPoints;
    private MeshRenderer meshRenderer;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private void Start()
    {
        Hide();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CustomVehicleController>(out CustomVehicleController customVehicle) ||
            other.TryGetComponent<VehicleController>(out VehicleController vehicle))
        {
            Debug.Log($"Player passed checkpoint {gameObject.name}");
            trackCheckPoints.PlayerThroughCheckPoint(this);
        }
    }

    public void SetTrackCheckPoints(TrackCheckPoints trackCheckPoints)
    {
        this.trackCheckPoints = trackCheckPoints;
    }
    
    public void Show()
    {
        meshRenderer.enabled = true;
    }
    
    public void Hide()
    {
        meshRenderer.enabled = false;
    }
}