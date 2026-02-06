using UnityEngine;

public class CustomVehicleController : MonoBehaviour
{
    [Header("Wheel Positions")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;
    
    [Header("Wheel Meshes (Visual)")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;
    
    [Header("Suspension Settings")]
    public float suspensionRestDistance = 0.6f;
    public float springStrength = 35000f;
    public float springDamper = 4500f;
    public float maxSuspensionTravel = 0.3f;
    public float wheelRadius = 0.3f;
    public float landingForceMultiplier = 2f; // Extra spring force when landing hard
    
    [Header("Steering Settings")]
    public float maxSteeringAngle = 25f;
    public float steerSpeed = 8f;
    public AnimationCurve steerCurveBySpeed = AnimationCurve.Linear(0, 1, 30, 0.4f);
    
    [Header("Drive Settings")]
    public float motorPower = 18000f;
    public float reversePower = 10000f;
    public float brakePower = 25000f;
    public float maxSpeed = 30f;
    public float maxReverseSpeed = 10f;
    public AnimationCurve powerCurveBySpeed = AnimationCurve.EaseInOut(0, 1, 30, 0.3f);
    
    [Header("Tire Grip Settings")]
    public float forwardGripFactor = 2.5f;
    public float sidewaysGripFactor = 3.5f;
    public float tireMass = 15f;
    
    [Header("Drifting")]
    public KeyCode driftKey = KeyCode.Space;
    public float driftGripReduction = 0.3f;  // Reduces grip to 30% during drift
    public float minSpeedToDrift = 5f;
    public float driftSteerBoost = 1.5f;
    
    [Header("Visual Settings")]
    public float wheelSmoothSpeed = 15f; // Higher = snappier, Lower = more realistic
    public float landingDampBoost = 2f; // Extra dampening when hitting ground
    
    [Header("Center of Mass")]
    public Transform centerOfMass;
    
    [Header("Debug")]
    public bool showDebugRays = true;
    
    private Rigidbody rb;
    private float motorInput;
    private float steerInput;
    private float currentSteerAngle;
    private bool isDrifting;
    private Transform[] allWheels;
    private Vector3[] wheelTargetPositions = new Vector3[4];
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Set center of mass
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }
        else
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
        }
        
        // Store all wheel transforms for easy iteration
        allWheels = new Transform[] 
        { 
            frontLeftWheel, frontRightWheel, 
            rearLeftWheel, rearRightWheel 
        };
        
        // Initialize wheel target positions array
        wheelTargetPositions = new Vector3[4];
        
        // Validate setup
        foreach (var wheel in allWheels)
        {
            if (wheel == null)
            {
                Debug.LogError("Missing wheel transform assignment!");
            }
        }
    }
    
    void Update()
    {
        // Get input
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        
        // Handle drift state
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        bool driftPressed = Input.GetKey(driftKey);
        
        if (driftPressed && Mathf.Abs(currentSpeed) >= minSpeedToDrift && Mathf.Abs(steerInput) > 0.1f)
        {
            isDrifting = true;
        }
        else if (!driftPressed)
        {
            isDrifting = false;
        }
        
        // Update steering angle smoothly with speed-based reduction
        float speedFactor = steerCurveBySpeed.Evaluate(Mathf.Abs(currentSpeed));
        float steerBoost = isDrifting ? driftSteerBoost : 1f;
        float targetSteerAngle = steerInput * maxSteeringAngle * speedFactor * steerBoost;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steerSpeed);
    }
    
    void FixedUpdate()
    {
        // Apply physics forces at each wheel
        foreach (Transform wheel in allWheels)
        {
            if (wheel != null)
            {
                ApplyWheelForces(wheel);
            }
        }
        
        // Enforce speed limits
        EnforceSpeedLimits();
        
        // Update visual wheel meshes
        UpdateWheelVisuals();
    }
    
    void ApplyWheelForces(Transform wheel)
    {
        RaycastHit hit;
        
        // Cast ray from wheel position downward
        if (Physics.Raycast(wheel.position, -wheel.up, out hit, suspensionRestDistance + maxSuspensionTravel))
        {
            // Calculate all three force components
            Vector3 suspensionForce = CalculateSuspensionForce(wheel, hit);
            Vector3 steeringForce = CalculateSteeringForce(wheel);
            Vector3 driveForce = CalculateDriveForce(wheel);
            
            // Apply forces at wheel position
            rb.AddForceAtPosition(suspensionForce, wheel.position);
            rb.AddForceAtPosition(steeringForce, wheel.position);
            rb.AddForceAtPosition(driveForce, wheel.position);
            
            // Debug visualization
            if (showDebugRays)
            {
                Debug.DrawRay(wheel.position, suspensionForce.normalized * 0.5f, Color.green);
                Debug.DrawRay(wheel.position, steeringForce.normalized * 0.5f, Color.red);
                Debug.DrawRay(wheel.position, driveForce.normalized * 0.5f, Color.blue);
            }
        }
    }
    
    Vector3 CalculateSuspensionForce(Transform wheel, RaycastHit hit)
    {
        // How much is the spring compressed?
        float offset = suspensionRestDistance - hit.distance;
        
        // Spring force (Hooke's Law: F = -kx)
        float springForce = offset * springStrength;
        
        // Damper force (opposes velocity)
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float verticalVelocity = Vector3.Dot(wheel.up, wheelVelocity);
        float damperForce = verticalVelocity * springDamper;
        
        // LANDING BOOST: If hitting ground hard (high downward velocity), add extra spring force
        if (verticalVelocity < -5f && offset > 0.05f) // Moving down fast and spring is compressed
        {
            springForce *= landingForceMultiplier;
        }
        
        // Combined suspension force pointing upward
        float totalForce = springForce - damperForce;
        return wheel.up * totalForce;
    }
    
    Vector3 CalculateSteeringForce(Transform wheel)
    {
        // Get current velocity at this wheel position
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        
        // Determine steering direction based on wheel rotation
        Vector3 steeringDir;
        if (IsFrontWheel(wheel))
        {
            // Front wheels steer - rotate the right vector by steer angle
            steeringDir = Quaternion.AngleAxis(currentSteerAngle, wheel.up) * wheel.right;
        }
        else
        {
            // Rear wheels don't steer
            steeringDir = wheel.right;
        }
        
        // Calculate sideways slip velocity
        float steeringVelocity = Vector3.Dot(steeringDir, wheelVelocity);
        
        // Apply grip reduction during drift
        float gripMultiplier = isDrifting ? driftGripReduction : 1f;
        
        // Calculate desired velocity change (we want to eliminate slip)
        float desiredVelocityChange = -steeringVelocity * sidewaysGripFactor * gripMultiplier;
        
        // F = ma, so calculate required acceleration
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;
        
        return steeringDir * (tireMass * desiredAcceleration);
    }
    
    Vector3 CalculateDriveForce(Transform wheel)
    {
        // Only rear wheels drive
        if (IsFrontWheel(wheel))
        {
            return Vector3.zero;
        }
        
        // Get forward velocity at wheel
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float forwardSpeed = Vector3.Dot(wheel.forward, wheelVelocity);
        
        float availablePower = 0f;
        
        // Forward drive
        if (motorInput > 0)
        {
            // Scale power by speed curve
            float powerMultiplier = powerCurveBySpeed.Evaluate(Mathf.Abs(forwardSpeed));
            availablePower = motorInput * motorPower * powerMultiplier;
        }
        // Reverse drive
        else if (motorInput < 0)
        {
            // If moving forward, apply brakes instead
            if (forwardSpeed > 1f)
            {
                availablePower = -Mathf.Sign(forwardSpeed) * brakePower;
            }
            else
            {
                // Actually reverse
                float powerMultiplier = powerCurveBySpeed.Evaluate(Mathf.Abs(forwardSpeed));
                availablePower = motorInput * reversePower * powerMultiplier;
            }
        }
        // Braking (no input)
        else
        {
            // Apply light rolling resistance
            availablePower = -Mathf.Sign(forwardSpeed) * forwardGripFactor * 1000f;
        }
        
        return wheel.forward * availablePower;
    }
    
    void EnforceSpeedLimits()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Forward speed limit
        if (motorInput > 0 && forwardSpeed > maxSpeed)
        {
            Vector3 excessVelocity = transform.forward * (forwardSpeed - maxSpeed);
            rb.linearVelocity -= excessVelocity;
        }
        // Reverse speed limit
        else if (motorInput < 0 && forwardSpeed < -maxReverseSpeed)
        {
            Vector3 excessVelocity = transform.forward * (forwardSpeed + maxReverseSpeed);
            rb.linearVelocity -= excessVelocity;
        }
    }
    
    void UpdateWheelVisuals()
    {
        UpdateSingleWheelVisual(frontLeftWheel, frontLeftMesh, 0);
        UpdateSingleWheelVisual(frontRightWheel, frontRightMesh, 1);
        UpdateSingleWheelVisual(rearLeftWheel, rearLeftMesh, 2);
        UpdateSingleWheelVisual(rearRightWheel, rearRightMesh, 3);
    }
    
    void UpdateSingleWheelVisual(Transform wheelTransform, Transform wheelMesh, int wheelIndex)
    {
        if (wheelTransform == null || wheelMesh == null) return;
        
        RaycastHit hit;
        Vector3 targetPosition;
        bool isGrounded = false;
        
        if (Physics.Raycast(wheelTransform.position, -wheelTransform.up, out hit, suspensionRestDistance + maxSuspensionTravel))
        {
            isGrounded = true;
            // Wheel is grounded - position at contact point plus wheel radius
            targetPosition = hit.point + wheelTransform.up * wheelRadius;
        }
        else
        {
            // Wheel is in the air - extend to max suspension travel
            targetPosition = wheelTransform.position - wheelTransform.up * (suspensionRestDistance + maxSuspensionTravel - wheelRadius);
        }
        
        // CRITICAL FIX: If wheels drifted too far during air time, snap them back
        float distanceToTarget = Vector3.Distance(wheelMesh.position, targetPosition);
        if (isGrounded && distanceToTarget > suspensionRestDistance * 1.5f)
        {
            // Snap to extended position
            wheelMesh.position = wheelTransform.position - wheelTransform.up * (suspensionRestDistance + maxSuspensionTravel - wheelRadius);
        }
        
        // Determine smooth speed
        float currentSmoothSpeed = wheelSmoothSpeed;
        if (isGrounded)
        {
            Vector3 toTarget = targetPosition - wheelMesh.position;
            if (Vector3.Dot(toTarget, wheelTransform.up) > 0) // Moving upward (compressing)
            {
                currentSmoothSpeed *= landingDampBoost;
            }
        }
        
        // Smoothly interpolate to target position
        wheelMesh.position = Vector3.Lerp(
            wheelMesh.position, 
            targetPosition, 
            Time.fixedDeltaTime * currentSmoothSpeed
        );
        
        // Rotate wheel based on speed
        Vector3 wheelVelocity = rb.GetPointVelocity(wheelTransform.position);
        float forwardSpeed = Vector3.Dot(wheelTransform.forward, wheelVelocity);
        float rotationSpeed = (forwardSpeed / (2 * Mathf.PI * wheelRadius)) * 360f * Time.fixedDeltaTime;
        wheelMesh.Rotate(Vector3.right, rotationSpeed, Space.Self);
        
        // Apply steering rotation to front wheels
        if (IsFrontWheel(wheelTransform))
        {
            // Smoothly rotate steering
            Quaternion targetRotation = Quaternion.Euler(
                wheelMesh.localRotation.eulerAngles.x, 
                currentSteerAngle, 
                0
            );
            wheelMesh.localRotation = Quaternion.Slerp(
                wheelMesh.localRotation, 
                targetRotation, 
                Time.fixedDeltaTime * steerSpeed
            );
        }
    }
    
    bool IsFrontWheel(Transform wheel)
    {
        // Front wheels have positive Z in local space
        return wheel.localPosition.z > 0;
    }
    
    // Public getter for drift state
    public bool IsDrifting()
    {
        return isDrifting;
    }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || allWheels == null) return;
        
        // Draw suspension travel visualization
        foreach (Transform wheel in allWheels)
        {
            if (wheel != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(wheel.position, 0.1f);
                Gizmos.DrawLine(wheel.position, wheel.position - wheel.up * (suspensionRestDistance + maxSuspensionTravel));
            }
        }
    }
}