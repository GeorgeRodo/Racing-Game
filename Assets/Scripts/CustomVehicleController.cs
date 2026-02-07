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
    public float driftTorque = 8000f; // Torque applied when initiating drift
    public float driftAngularDamping = 3f; // Controls rotation during drift (lower = more spin)
    public float normalAngularDamping = 5f; // Normal angular damping when not drifting
    public float driftExitSnapiness = 0.3f; // How quickly car straightens when exiting drift (0-1)
    public float counterSteerAssist = 0.4f; // Helps prevent spin-outs (0-1)
    
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
    private bool wasDrifting;
    private float driftDirection; // -1 for left, 1 for right
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
        
        // Set initial angular damping
        rb.angularDamping = normalAngularDamping;
        
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
        if (!enabled) return;
        
        // Get input
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        
        // Handle drift state
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        bool driftPressed = Input.GetKey(driftKey);
        
        wasDrifting = isDrifting;
        
        // Check if we can/should be drifting
        if (driftPressed && Mathf.Abs(currentSpeed) >= minSpeedToDrift && Mathf.Abs(steerInput) > 0.1f)
        {
            if (!isDrifting)
            {
                // Just started drifting - record direction
                driftDirection = Mathf.Sign(steerInput);
            }
            isDrifting = true;
        }
        else if (!driftPressed)
        {
            isDrifting = false;
        }
        
        // Update angular damping based on drift state
        if (isDrifting)
        {
            rb.angularDamping = driftAngularDamping;
        }
        else
        {
            rb.angularDamping = normalAngularDamping;
        }
        
        // Update steering angle smoothly with speed-based reduction
        float speedFactor = steerCurveBySpeed.Evaluate(Mathf.Abs(currentSpeed));
        float steerBoost = isDrifting ? driftSteerBoost : 1f;
        float targetSteerAngle = steerInput * maxSteeringAngle * speedFactor * steerBoost;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steerSpeed);
    }
    
    void FixedUpdate()
    {
        // Apply drift physics
        ApplyDriftPhysics();
        
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
    
    void ApplyDriftPhysics()
    {
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Only apply drift physics if we're actually drifting
        if (isDrifting && Mathf.Abs(currentSpeed) >= minSpeedToDrift)
        {
            // Apply torque to rotate the car in drift direction
            float torqueAmount = steerInput * driftTorque;
            rb.AddTorque(transform.up * torqueAmount);
            
            // Counter-steer assist: if player is steering opposite to drift, help straighten out
            if (Mathf.Sign(steerInput) != Mathf.Sign(driftDirection) && Mathf.Abs(steerInput) > 0.3f)
            {
                // Apply counter-torque to prevent spin-out
                Vector3 angularVel = rb.angularVelocity;
                float yawVelocity = Vector3.Dot(angularVel, transform.up);
                rb.AddTorque(-transform.up * yawVelocity * counterSteerAssist * 1000f);
            }
        }
        // Exiting drift - help straighten the car
        else if (wasDrifting && !isDrifting)
        {
            // Snap velocity more towards forward direction
            Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            Vector3 sidewaysVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
            
            // Reduce sideways velocity to help straighten out
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                forwardVel + sidewaysVel * (1f - driftExitSnapiness),
                0.3f
            );
            
            // Also dampen angular velocity
            rb.angularVelocity *= 0.7f;
        }
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
        
        // REAR WHEELS GET EXTRA GRIP REDUCTION during drift for better sliding
        if (isDrifting && !IsFrontWheel(wheel))
        {
            gripMultiplier *= 0.7f; // Even less grip on rear wheels
        }
        
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
            
            // Boost power slightly during drift for maintaining speed
            if (isDrifting)
            {
                availablePower *= 1.2f;
            }
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
            // Apply light rolling resistance (less during drift)
            float resistanceMultiplier = isDrifting ? 0.5f : 1f;
            availablePower = -Mathf.Sign(forwardSpeed) * forwardGripFactor * 1000f * resistanceMultiplier;
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
    
    // Public getter for drift angle (useful for UI/effects)
    public float GetDriftAngle()
    {
        if (!isDrifting) return 0f;
        
        // Calculate angle between velocity and forward direction
        Vector3 velocity = rb.linearVelocity;
        if (velocity.magnitude < 0.1f) return 0f;
        
        Vector3 velocityDir = velocity.normalized;
        Vector3 forwardDir = transform.forward;
        
        float angle = Vector3.SignedAngle(forwardDir, velocityDir, Vector3.up);
        return angle;
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
        
        // Draw drift direction indicator when drifting
        if (isDrifting)
        {
            Gizmos.color = Color.cyan;
            Vector3 driftIndicator = transform.position + transform.up * 2f;
            Gizmos.DrawWireSphere(driftIndicator, 0.3f);
            Gizmos.DrawRay(driftIndicator, transform.right * driftDirection * 2f);
        }
    }
}