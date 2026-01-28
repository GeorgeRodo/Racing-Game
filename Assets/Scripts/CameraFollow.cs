using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -8);
    
    [Header("Smoothing")]
    public float positionSmoothing = 5f;
    public float rotationSmoothing = 3f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public float returnSpeed = 5f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    
    [Header("Speed-Based Zoom")]
    public float minDistance = 8f;      
    public float maxDistance = 14f;
    public float speedForMaxZoom = 25f;
    public float zoomSmoothing = 4f;
    
    [Header("Dynamic Camera")]
    public float maxSpeedFOV = 75f;
    public float normalFOV = 60f;
    
    private Camera cam;
    private Rigidbody targetRb;
    private Vector3 velocity = Vector3.zero;
    
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 0f;
    private Vector3 currentOffset;
    private float currentDistance;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }
        currentOffset = offset;
        currentDistance = offset.magnitude;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float speed = targetRb != null ? targetRb.linearVelocity.magnitude : 0f;
        
        float speedFactor = Mathf.Clamp01(speed / speedForMaxZoom);
        float targetDistance = Mathf.Lerp(minDistance, maxDistance, speedFactor);
        
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothing * Time.deltaTime);
        
        Vector3 offsetDirection = offset.normalized;
        Vector3 baseOffset = offsetDirection * currentDistance;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            currentHorizontalAngle += mouseX;
            currentVerticalAngle -= mouseY;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
            currentOffset = rotation * baseOffset;
        }
        else
        {
            currentHorizontalAngle = Mathf.Lerp(currentHorizontalAngle, 0f, returnSpeed * Time.deltaTime);
            currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, 0f, returnSpeed * Time.deltaTime);
            
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
            currentOffset = rotation * baseOffset;
        }

        Vector3 desiredPosition = target.position + target.TransformDirection(currentOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / positionSmoothing);
        
        Vector3 lookDirection = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        
        // Dynamic FOV only - NO SHAKE
        if (targetRb != null && cam != null)
        {
            float targetFOV = Mathf.Lerp(normalFOV, maxSpeedFOV, speed / 25f);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
        }
    }
}