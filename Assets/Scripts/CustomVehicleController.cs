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
    public float landingForceMultiplier = 2f;

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

    [Header("Physics")]
    public float angularDamping = 5f;
    public float maxAngularVelocity = 3.5f;

    [Header("Stability")]
    public float sleepVelocityThreshold = 0.1f;
    public float sleepAngularThreshold = 0.1f;

    [Header("Boost System")]
    public float boostMaxSpeedMultiplier = 1.5f;  // Max speed becomes 45 instead of 30
    public float boostPowerMultiplier = 1.3f;     // Extra acceleration during boost
    public float boostInitialForce = 8f;          // Instant speed kick when boost starts

    [Header("Center of Mass")]
    public Transform centerOfMass;

    [Header("Debug")]
    public bool showDebugRays = true;

    private Rigidbody rb;
    private Transform[] allWheels;
    private float motorInput;
    private float steerInput;
    private float currentSteerAngle;
    private bool isAsleep = false;
    
    // Boost state
    private bool isBoosting = false;
    private float boostTimer = 0f;
    private float boostDuration = 0f;
    
    // Speed display
    private int lastSpeedTier = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularDamping = angularDamping;

        rb.centerOfMass = centerOfMass != null
            ? centerOfMass.localPosition
            : new Vector3(0, -0.5f, 0);

        allWheels = new Transform[]
        {
            frontLeftWheel,
            frontRightWheel,
            rearLeftWheel,
            rearRightWheel
        };
    }

    void Update()
    {
        CaptureInput();
        UpdateSteering();
        UpdateBoost();
        DisplaySpeed();
    }

    void FixedUpdate()
    {
        CheckSleepState();
        
        if (!isAsleep)
        {
            ApplyWheelForces();
        }
        
        EnforceSpeedLimits();
        ClampAngularVelocity();
        UpdateWheelVisuals();
    }

    void CaptureInput()
    {
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void CheckSleepState()
    {
        float speed = rb.linearVelocity.magnitude;
        float angularSpeed = rb.angularVelocity.magnitude;
        bool hasInput = Mathf.Abs(motorInput) > 0.01f || Mathf.Abs(steerInput) > 0.01f;

        if (speed < sleepVelocityThreshold && angularSpeed < sleepAngularThreshold && !hasInput)
        {
            if (!isAsleep)
            {
                isAsleep = true;
                // Fully stop the vehicle
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            isAsleep = false;
        }
    }

    void UpdateSteering()
    {
        float speed = rb.linearVelocity.magnitude;
        float speedFactor = steerCurveBySpeed.Evaluate(speed);
        float targetSteer = steerInput * maxSteeringAngle * speedFactor;
        
        currentSteerAngle = Mathf.Lerp(
            currentSteerAngle,
            targetSteer,
            Time.deltaTime * steerSpeed
        );
    }

    void UpdateBoost()
    {
        if (isBoosting)
        {
            boostTimer += Time.deltaTime;
            
            // Check if boost duration is over
            if (boostTimer >= boostDuration)
            {
                isBoosting = false;
                boostTimer = 0f;
            }
        }
    }

    void DisplaySpeed()
    {
        // Get forward speed
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Round to nearest 10
        int currentSpeedTier = Mathf.FloorToInt(Mathf.Abs(forwardSpeed) / 10f) * 10;
        
        // Only log when we cross into a new tier
        if (currentSpeedTier != lastSpeedTier && currentSpeedTier > 0)
        {
            string boostIndicator = isBoosting ? " [BOOST]" : "";
            Debug.Log($"Speed: {currentSpeedTier}+{boostIndicator}");
            lastSpeedTier = currentSpeedTier;
        }
        // Reset if we drop below 10
        else if (currentSpeedTier == 0 && lastSpeedTier != 0)
        {
            lastSpeedTier = 0;
        }
    }

    // Public method for SpeedBoost to call
    public void ActivateBoost(float duration)
    {
        // If already boosting, extend the duration
        if (isBoosting)
        {
            boostDuration = Mathf.Max(boostDuration, duration);
            boostTimer = 0f;
        }
        else
        {
            isBoosting = true;
            boostDuration = duration;
            boostTimer = 0f;
            
            // Give instant speed kick
            rb.AddForce(transform.forward * boostInitialForce, ForceMode.VelocityChange);
        }
    }

    // Public getter for boost state
    public bool IsBoosting()
    {
        return isBoosting;
    }

    // Public getter for boost remaining time (for UI)
    public float GetBoostTimeRemaining()
    {
        return isBoosting ? (boostDuration - boostTimer) : 0f;
    }

    void ApplyWheelForces()
    {
        foreach (Transform wheel in allWheels)
        {
            ProcessWheel(wheel);
        }
    }

    void ProcessWheel(Transform wheel)
    {
        if (!Physics.Raycast(
            wheel.position,
            -wheel.up,
            out RaycastHit hit,
            suspensionRestDistance + maxSuspensionTravel))
        {
            return;
        }

        Vector3 suspensionForce = CalculateSuspensionForce(wheel, hit);
        Vector3 steeringForce = CalculateSteeringForce(wheel);
        Vector3 driveForce = CalculateDriveForce(wheel);

        rb.AddForceAtPosition(suspensionForce, wheel.position);
        rb.AddForceAtPosition(steeringForce, wheel.position);
        rb.AddForceAtPosition(driveForce, wheel.position);

        if (showDebugRays)
        {
            Debug.DrawRay(wheel.position, -wheel.up * hit.distance, Color.green);
        }
    }

    Vector3 CalculateSuspensionForce(Transform wheel, RaycastHit hit)
    {
        float offset = suspensionRestDistance - hit.distance;
        float springForce = offset * springStrength;

        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float verticalVelocity = Vector3.Dot(wheel.up, wheelVelocity);
        float damperForce = -verticalVelocity * springDamper;

        // Apply extra force on hard landings
        if (verticalVelocity < -5f && offset > 0.05f)
        {
            springForce *= landingForceMultiplier;
        }

        return wheel.up * (springForce + damperForce);
    }

    Vector3 CalculateSteeringForce(Transform wheel)
    {
        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);

        Vector3 steeringDirection = IsFrontWheel(wheel)
            ? Quaternion.AngleAxis(currentSteerAngle, wheel.up) * wheel.right
            : wheel.right;

        float sidewaysSlip = Vector3.Dot(steeringDirection, wheelVelocity);
        float desiredAcceleration = (-sidewaysSlip * sidewaysGripFactor) / Time.fixedDeltaTime;

        return steeringDirection * desiredAcceleration * tireMass;
    }

    Vector3 CalculateDriveForce(Transform wheel)
    {
        // Only rear wheels drive
        if (IsFrontWheel(wheel))
        {
            return Vector3.zero;
        }

        Vector3 wheelVelocity = rb.GetPointVelocity(wheel.position);
        float forwardSpeed = Vector3.Dot(wheel.forward, wheelVelocity);
        float drivePower = 0f;

        if (motorInput > 0f)
        {
            // Accelerate forward
            float speedCurveFactor = powerCurveBySpeed.Evaluate(Mathf.Abs(forwardSpeed));
            drivePower = motorInput * motorPower * speedCurveFactor;
            
            // Apply boost power multiplier
            if (isBoosting)
            {
                drivePower *= boostPowerMultiplier;
            }
        }
        else if (motorInput < 0f)
        {
            // Brake or reverse
            if (forwardSpeed > 1f)
            {
                // Apply brakes
                drivePower = -Mathf.Sign(forwardSpeed) * brakePower;
            }
            else
            {
                // Reverse
                drivePower = motorInput * reversePower;
            }
        }
        else
        {
            // No input - apply rolling resistance
            drivePower = -Mathf.Sign(forwardSpeed) * forwardGripFactor * 1000f;
        }

        return wheel.forward * drivePower;
    }

    void EnforceSpeedLimits()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Calculate effective max speed (boosted or normal)
        float effectiveMaxSpeed = isBoosting ? (maxSpeed * boostMaxSpeedMultiplier) : maxSpeed;

        if (forwardSpeed > effectiveMaxSpeed)
        {
            rb.linearVelocity -= transform.forward * (forwardSpeed - effectiveMaxSpeed);
        }
        else if (forwardSpeed < -maxReverseSpeed)
        {
            rb.linearVelocity -= transform.forward * (forwardSpeed + maxReverseSpeed);
        }
    }

    void ClampAngularVelocity()
    {
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularVelocity);
    }

    void UpdateWheelVisuals()
    {
        UpdateWheelMesh(frontLeftWheel, frontLeftMesh);
        UpdateWheelMesh(frontRightWheel, frontRightMesh);
        UpdateWheelMesh(rearLeftWheel, rearLeftMesh);
        UpdateWheelMesh(rearRightWheel, rearRightMesh);
    }

    void UpdateWheelMesh(Transform wheel, Transform mesh)
    {
        if (wheel == null || mesh == null)
        {
            return;
        }

        // Update wheel position based on suspension
        if (Physics.Raycast(
            wheel.position,
            -wheel.up,
            out RaycastHit hit,
            suspensionRestDistance + maxSuspensionTravel))
        {
            mesh.position = hit.point + wheel.up * wheelRadius;
        }

        // Update steering rotation for front wheels
        if (IsFrontWheel(wheel))
        {
            Quaternion targetRotation = Quaternion.Euler(
                mesh.localRotation.eulerAngles.x,
                currentSteerAngle,
                mesh.localRotation.eulerAngles.z
            );

            mesh.localRotation = Quaternion.Slerp(
                mesh.localRotation,
                targetRotation,
                Time.fixedDeltaTime * steerSpeed
            );
        }
    }

    bool IsFrontWheel(Transform wheel)
    {
        return wheel.localPosition.z > 0;
    }
}