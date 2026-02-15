using UnityEngine;

/// <summary>
/// Spawns taxis that drive straight across the screen from random edges
/// Perfect for animated menu backgrounds with top-down camera
/// </summary>
public class MenuBackgroundTaxis : MonoBehaviour
{
    [Header("Taxi Settings")]
    public GameObject taxiPrefab;
    
    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;        // How often taxis appear (lower = more chaos!)
    public int maxTaxis = 20;                 // Maximum taxis on screen at once
    public float taxiLifetime = 15f;          // How long before taxi despawns
    
    [Header("Speed Settings")]
    public float minSpeed = 8f;               // Minimum taxi speed
    public float maxSpeed = 15f;              // Maximum taxi speed
    
    [Header("Height Variation")]
    public float minHeight = 0f;              // Minimum Y position
    public float maxHeight = 2f;              // Maximum Y position (for stacked effect)
    
    [Header("Camera Reference")]
    public Camera backgroundCamera;           // Top-down camera
    public float spawnDistance = 0.5f;        // How far outside camera view to spawn (very tight!)
    
    private float spawnTimer = 0f;
    private int currentTaxiCount = 0;
    private float cameraViewWidth;
    private float cameraViewHeight;
    
    void Start()
    {
        // Calculate camera view bounds based on actual camera setup
        if (backgroundCamera != null)
        {
            // Camera is at (0, 50, -94.8) looking down
            // Your visible area block: Size (126, 7.7, 71.4) at position (391, -3.9, -87)
            // This means the camera sees approximately:
            // Width (X): ±63 units from center (126/2)
            // Height (Z): ±35.7 units from camera Z position (71.4/2)
            
            // But let's also calculate from camera FOV for accuracy
            float cameraHeight = backgroundCamera.transform.position.y;
            float calculatedViewHeight = 2f * cameraHeight * Mathf.Tan(backgroundCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float calculatedViewWidth = calculatedViewHeight * backgroundCamera.aspect;
            
            // Use your actual measured area (more accurate!)
            cameraViewWidth = 126f;   // Your block width
            cameraViewHeight = 71.4f; // Your block depth (Z dimension)
            
            Debug.Log($"📷 Camera at: {backgroundCamera.transform.position}");
            Debug.Log($"📏 Using measured view area: {cameraViewWidth:F1} x {cameraViewHeight:F1} units");
            Debug.Log($"📐 Calculated from FOV: {calculatedViewWidth:F1} x {calculatedViewHeight:F1} units");
        }
        else
        {
            Debug.LogError("❌ Background camera not assigned!");
            cameraViewWidth = 126f;
            cameraViewHeight = 71.4f;
        }
    }
    
    void Update()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval && currentTaxiCount < maxTaxis)
        {
            SpawnTaxi();
            spawnTimer = 0f;
        }
    }
    
    void SpawnTaxi()
    {
        if (taxiPrefab == null)
        {
            Debug.LogError("❌ No taxi prefab assigned!");
            return;
        }
        
        currentTaxiCount++;
        
        // Create taxi
        GameObject taxi = Instantiate(taxiPrefab, Vector3.zero, Quaternion.identity, transform);
        taxi.name = $"Taxi_{Random.Range(1000, 9999)}";
        
        // Disable physics (we control movement manually)
        Rigidbody rb = taxi.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        // Disable vehicle controller
        var controller = taxi.GetComponent<CustomVehicleController>();
        if (controller != null) controller.enabled = false;
        
        // Choose a predetermined route pattern
        int routePattern = Random.Range(0, 3); // 0=Row, 1=S-Shape, 2=Backwards
        
        switch (routePattern)
        {
            case 0: // Row - straight line across
                AddRowRoute(taxi);
                break;
            case 1: // S-Shape - weaving pattern
                AddSShapeRoute(taxi);
                break;
            case 2: // Backwards - driving in reverse
                AddBackwardsRoute(taxi);
                break;
        }
        
        // Auto-destroy after lifetime
        Destroy(taxi, taxiLifetime);
        StartCoroutine(DecrementCountAfterDelay());
    }
    
    void AddRowRoute(GameObject taxi)
    {
        // Straight line across the screen
        var mover = taxi.AddComponent<StraightDrivingTaxi>();
        
        // Choose random edge
        int edge = Random.Range(0, 4);
        
        // Camera is at Z=-94.8, visible area is centered there
        float cameraZOffset = backgroundCamera != null ? backgroundCamera.transform.position.z : -94.8f;
        
        // Spawn bounds based on actual visible area
        float halfWidth = (cameraViewWidth / 2f);   // ±63 units in X
        float halfHeight = (cameraViewHeight / 2f);  // ±35.7 units in Z from camera position
        
        float randomHeight = Random.Range(minHeight, maxHeight);
        
        Vector3 spawnPos = Vector3.zero;
        Vector3 direction = Vector3.zero;
        
        switch (edge)
        {
            case 0: // Left → Right
                spawnPos = new Vector3(
                    -halfWidth - spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                direction = Vector3.right;
                break;
                
            case 1: // Right → Left
                spawnPos = new Vector3(
                    halfWidth + spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                direction = Vector3.left;
                break;
                
            case 2: // Top → Bottom (far Z)
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset - halfHeight - spawnDistance
                );
                direction = Vector3.forward; // Toward camera (positive Z)
                break;
                
            case 3: // Bottom → Top (near Z)
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset + halfHeight + spawnDistance
                );
                direction = Vector3.back; // Away from camera (negative Z)
                break;
        }
        
        mover.transform.position = spawnPos;
        mover.direction = direction;
        mover.speed = Random.Range(minSpeed, maxSpeed);
        mover.rotationOffset = 0f;
        
        taxi.name += "_Row";
    }
    
    void AddSShapeRoute(GameObject taxi)
    {
        // S-shaped weaving pattern
        var mover = taxi.AddComponent<SShapeTaxi>();
        
        float cameraZOffset = backgroundCamera != null ? backgroundCamera.transform.position.z : -94.8f;
        float halfHeight = (cameraViewHeight / 2f);
        float randomHeight = Random.Range(minHeight, maxHeight);
        
        // Spawn from bottom (near camera), move away
        mover.transform.position = new Vector3(
            Random.Range(-10f, 10f), 
            randomHeight, 
            cameraZOffset + halfHeight + spawnDistance
        );
        mover.forwardSpeed = Random.Range(minSpeed * 0.7f, maxSpeed * 0.7f);
        mover.waveAmplitude = Random.Range(8f, 15f);
        mover.waveFrequency = Random.Range(1f, 2f);
        
        taxi.name += "_SShape";
    }
    
    void AddBackwardsRoute(GameObject taxi)
    {
        // Driving backwards
        var mover = taxi.AddComponent<BackwardsDrivingTaxi>();
        
        int edge = Random.Range(0, 4);
        float cameraZOffset = backgroundCamera != null ? backgroundCamera.transform.position.z : -94.8f;
        
        float halfWidth = (cameraViewWidth / 2f);
        float halfHeight = (cameraViewHeight / 2f);
        float randomHeight = Random.Range(minHeight, maxHeight);
        
        Vector3 spawnPos = Vector3.zero;
        Vector3 moveDirection = Vector3.zero;
        Vector3 faceDirection = Vector3.zero;
        
        switch (edge)
        {
            case 0: // Spawn left, move right, face left
                spawnPos = new Vector3(
                    -halfWidth - spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                moveDirection = Vector3.right;
                faceDirection = Vector3.left;
                break;
                
            case 1: // Spawn right, move left, face right
                spawnPos = new Vector3(
                    halfWidth + spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                moveDirection = Vector3.left;
                faceDirection = Vector3.right;
                break;
                
            case 2: // Spawn far, move near, face away
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset - halfHeight - spawnDistance
                );
                moveDirection = Vector3.forward;
                faceDirection = Vector3.back;
                break;
                
            case 3: // Spawn near, move far, face toward
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset + halfHeight + spawnDistance
                );
                moveDirection = Vector3.back;
                faceDirection = Vector3.forward;
                break;
        }
        
        mover.transform.position = spawnPos;
        mover.moveDirection = moveDirection;
        mover.faceDirection = faceDirection;
        mover.speed = Random.Range(minSpeed * 0.8f, maxSpeed * 0.8f);
        
        taxi.name += "_Backwards";
    }
    
    System.Collections.IEnumerator DecrementCountAfterDelay()
    {
        yield return new WaitForSeconds(taxiLifetime);
        currentTaxiCount--;
    }
}

/// <summary>
/// Taxi drives in a straight line (Row pattern)
/// </summary>
public class StraightDrivingTaxi : MonoBehaviour
{
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float speed;
    [HideInInspector] public float rotationOffset;
    
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        
        if (direction != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(direction);
            Quaternion offsetRotation = Quaternion.Euler(0f, rotationOffset, 0f);
            transform.rotation = baseRotation * offsetRotation;
        }
    }
}

/// <summary>
/// Taxi drives in an S-shape pattern (weaving side to side)
/// </summary>
public class SShapeTaxi : MonoBehaviour
{
    [HideInInspector] public float forwardSpeed;
    [HideInInspector] public float waveAmplitude;
    [HideInInspector] public float waveFrequency;
    
    private float timeElapsed = 0f;
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
        
        // Move forward (in Z direction)
        float forwardMovement = forwardSpeed * Time.deltaTime;
        
        // Sideways wave (in X direction)
        float sidewaysOffset = Mathf.Sin(timeElapsed * waveFrequency) * waveAmplitude;
        float previousSidewaysOffset = Mathf.Sin((timeElapsed - Time.deltaTime) * waveFrequency) * waveAmplitude;
        float sidewaysMovement = sidewaysOffset - previousSidewaysOffset;
        
        // Apply movement
        transform.position += new Vector3(sidewaysMovement, 0f, forwardMovement);
        
        // Face direction of movement
        Vector3 movementDirection = new Vector3(sidewaysMovement, 0f, forwardMovement).normalized;
        if (movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
    }
}

/// <summary>
/// Taxi drives backwards (faces one direction, moves the opposite)
/// </summary>
public class BackwardsDrivingTaxi : MonoBehaviour
{
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public Vector3 faceDirection;
    [HideInInspector] public float speed;
    
    void Update()
    {
        // Move in one direction
        transform.position += moveDirection * speed * Time.deltaTime;
        
        // Face the opposite direction
        if (faceDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(faceDirection);
        }
    }
}