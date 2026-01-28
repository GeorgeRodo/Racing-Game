using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;
    
    [Header("Wheel Meshes (Visual)")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;
    
    [Header("Vehicle Settings")]
    public float maxMotorTorque = 500f;
    public float reverseMotorTorque = 200f;
    public float maxSpeed = 30f;          
    public float maxReverseSpeed = 10f;
    public float accelerationCurve = 0.15f;
    public float maxSteeringAngle = 25f;     
    public float brakeTorque = 5000f;
    public float decelerationForce = 1500f;
    
    [Header("Steering")]
    public float minSpeedForSteering = 2f;
    
    [Header("Drifting")]
    public KeyCode driftKey = KeyCode.Space;
    public float driftForce = 15f;               // Force applied sideways during drift
    public float minSpeedToDrift = 5f;           // Minimum speed required to drift
    public float driftControlMultiplier = 1.8f;  // Extra control during drift
    public float driftDrag = 0.98f;              // Slight speed reduction during drift
    
    [Header("Center of Mass")]
    public Transform centerOfMass;
    
    private Rigidbody rb;
    private float motorInput;
    private float steerInput;
    private float currentSpeed;
    private float currentAcceleration = 0f;
    
    // Drift state
    private bool isDrifting = false;
    private int driftDirection = 0;  // -1 for left, 1 for right, 0 for none

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }
        else
        {
            rb.centerOfMass = new Vector3(0, -0.8f, 0);  // Lower for stability
        }
    }

    void Update()
    {
        motorInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        
        // Calculate speed in the forward direction
        currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedMagnitude = rb.linearVelocity.magnitude;
        
        // Gradually build up acceleration
        if (motorInput != 0)
        {
            currentAcceleration = Mathf.MoveTowards(currentAcceleration, 1f, accelerationCurve * Time.deltaTime);
        }
        else
        {
            currentAcceleration = Mathf.MoveTowards(currentAcceleration, 0f, 2f * Time.deltaTime);
        }
        
        // Handle drift input
        HandleDriftInput(speedMagnitude);
    }

    void HandleDriftInput(float speed)
    {
        bool driftPressed = Input.GetKey(driftKey);
        
        // START DRIFTING - lock in the direction when space is pressed
        if (driftPressed && !isDrifting && speed >= minSpeedToDrift && Mathf.Abs(steerInput) > 0.1f)
        {
            isDrifting = true;
            // Lock drift direction based on current steering
            driftDirection = steerInput > 0 ? 1 : -1;
        }
        
        // STOP DRIFTING - release space
        if (!driftPressed && isDrifting)
        {
            isDrifting = false;
            driftDirection = 0;
        }
    }

    void FixedUpdate()
    {
        // MARIO KART STYLE DRIFT PHYSICS
        if (isDrifting)
        {
            // Apply sideways drift force in the locked direction
            Vector3 driftForceVector = transform.right * driftDirection * driftForce;
            rb.AddForce(driftForceVector, ForceMode.Acceleration);
            
            // Slight speed reduction during drift
            rb.linearVelocity *= driftDrag;
        }
        
        // Steering - boosted control during drift
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / minSpeedForSteering);
        float steerMultiplier = isDrifting ? driftControlMultiplier : 1f;
        float actualSteerAngle = steerInput * maxSteeringAngle * speedFactor * steerMultiplier;
        
        frontLeftWheel.steerAngle = actualSteerAngle;
        frontRightWheel.steerAngle = actualSteerAngle;
        
        // FORWARD motion
        if (motorInput > 0)
        {
            // Only apply torque if under max speed
            if (currentSpeed < maxSpeed)
            {
                float torque = motorInput * maxMotorTorque * currentAcceleration;
                
                // Reduce torque as we approach max speed (smooth limit)
                float speedRatio = currentSpeed / maxSpeed;
                if (speedRatio > 0.8f)
                {
                    torque *= 1f - ((speedRatio - 0.8f) / 0.2f);
                }
                
                rearLeftWheel.motorTorque = torque;
                rearRightWheel.motorTorque = torque;
            }
            else
            {
                // At max speed, no torque
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
            }
            ApplyBraking(0);
        }
        // REVERSE motion
        else if (motorInput < 0)
        {
            // If moving forward, brake first
            if (currentSpeed > 1f)
            {
                ApplyBraking(brakeTorque);
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
            }
            // If stopped or moving backward, apply reverse
            else if (currentSpeed > -maxReverseSpeed)
            {
                float torque = motorInput * reverseMotorTorque * currentAcceleration;
                rearLeftWheel.motorTorque = torque;
                rearRightWheel.motorTorque = torque;
                ApplyBraking(0);
            }
            else
            {
                // At max reverse speed
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
                ApplyBraking(0);
            }
        }
        // NO INPUT - coast/brake
        else
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
            
            // Additional deceleration
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.AddForce(-rb.linearVelocity.normalized * decelerationForce);
            }
        }
        
        // HARD SPEED LIMIT - enforce max speed
        float totalSpeed = rb.linearVelocity.magnitude;
        if (motorInput > 0 && totalSpeed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        else if (motorInput < 0 && totalSpeed > maxReverseSpeed && currentSpeed < 0)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxReverseSpeed;
        }
        
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(rearLeftWheel, rearLeftTransform);
        UpdateWheelPose(rearRightWheel, rearRightTransform);
    }

    void ApplyBraking(float brake)
    {
        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
    }

    void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        if (transform == null) return;
        
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        
        transform.position = pos;
        transform.rotation = rot;
    }
    
    // Public getter for UI or effects
    public bool IsDrifting()
    {
        return isDrifting;
    }
}