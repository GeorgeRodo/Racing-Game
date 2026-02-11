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
    
    [Header("Center of Mass")]
    public Transform centerOfMass;
    
    [Header("Engine Sound")]
    public AudioSource engineSound;
    public float minPitch = 0.5f;
    public float maxPitch = 2f;
    public float pitchSpeedFactor = 30f;  // Speed at which pitch reaches maximum
    public float minVolume = 0.3f;
    public float maxVolume = 1f;
    
    [Header("Physics Correction")]
    public float forwardGripStrength = 30f;  // Prevents sideways sliding
    public float sidewaysDragMultiplier = 3f; // Extra drag on sideways movement
    
    private Rigidbody rb;
    private float motorInput;
    private float steerInput;
    private float currentSpeed;
    private float currentAcceleration = 0f;

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
        
        // Setup engine sound
        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.Play();
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
        
        // Update engine sound based on speed
        UpdateEngineSound(speedMagnitude);
    }

    void UpdateEngineSound(float speed)
    {
        if (engineSound == null) return;
        
        // Calculate pitch based on speed (0 to pitchSpeedFactor)
        float speedRatio = Mathf.Clamp01(speed / pitchSpeedFactor);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        
        // Add slight variation based on acceleration input
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            targetPitch += 0.2f * Mathf.Abs(motorInput);
        }
        
        engineSound.pitch = Mathf.Lerp(engineSound.pitch, targetPitch, Time.deltaTime * 5f);
        
        // Volume increases with speed too
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, speedRatio * 0.5f + Mathf.Abs(motorInput) * 0.5f);
        engineSound.volume = Mathf.Lerp(engineSound.volume, targetVolume, Time.deltaTime * 3f);
    }

    void FixedUpdate()
    {
        // FIX SIDEWAYS DRIVING - Apply forward grip force
        PreventSidewaysSliding();
        
        // Steering with speed-based control
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / minSpeedForSteering);
        float actualSteerAngle = steerInput * maxSteeringAngle * speedFactor;
        
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
            ApplyBraking(brakeTorque * 0.3f);
            
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

    void PreventSidewaysSliding()
    {
        // Get the car's right vector (sideways direction)
        Vector3 sidewaysVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        
        // Calculate how much we're sliding sideways
        float sidewaysSpeed = sidewaysVelocity.magnitude;
        
        // Only apply correction if moving significantly
        if (rb.linearVelocity.magnitude > 1f && sidewaysSpeed > 0.5f)
        {
            // Apply force opposite to sideways motion (grip force)
            Vector3 correctionForce = -sidewaysVelocity * forwardGripStrength;
            rb.AddForce(correctionForce, ForceMode.Acceleration);
            
            // Also increase drag on sideways movement
            Vector3 sidewaysDrag = -sidewaysVelocity * sidewaysDragMultiplier;
            rb.AddForce(sidewaysDrag, ForceMode.Acceleration);
        }
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
}