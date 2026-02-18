using UnityEngine;

public class MenuBackgroundTaxis : MonoBehaviour
{
    [Header("Taxi Settings")]
    public GameObject taxiPrefab;
    
    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;       
    public int maxTaxis = 20;                
    public float taxiLifetime = 15f;         
    [Header("Speed Settings")]
    public float minSpeed = 8f;              
    public float maxSpeed = 15f;             
    
    [Header("Height Variation")]
    public float minHeight = 0f;             
    public float maxHeight = 2f;             
    
    [Header("Camera Reference")]
    public Camera backgroundCamera;          
    public float spawnDistance = 0.5f;       
    
    private float spawnTimer = 0f;
    private int currentTaxiCount = 0;
    private float cameraViewWidth;
    private float cameraViewHeight;
    
    void Start()
    {
        if (backgroundCamera != null)
        {                    
            float cameraHeight = backgroundCamera.transform.position.y;
            float calculatedViewHeight = 2f * cameraHeight * Mathf.Tan(backgroundCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float calculatedViewWidth = calculatedViewHeight * backgroundCamera.aspect;
            
            cameraViewWidth = 126f;   
            cameraViewHeight = 71.4f; 
            
            Debug.Log($"Camera at: {backgroundCamera.transform.position}");
            Debug.Log($"Using measured view area: {cameraViewWidth:F1} x {cameraViewHeight:F1} units");
            Debug.Log($"Calculated from FOV: {calculatedViewWidth:F1} x {calculatedViewHeight:F1} units");
        }
        else
        {
            Debug.LogError("Background camera not assigned!");
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
            Debug.LogError("No taxi prefab assigned!");
            return;
        }
        
        currentTaxiCount++;
        
        GameObject taxi = Instantiate(taxiPrefab, Vector3.zero, Quaternion.identity, transform);
        taxi.name = $"Taxi_{Random.Range(1000, 9999)}";
        
        Rigidbody rb = taxi.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        var controller = taxi.GetComponent<CustomVehicleController>();
        if (controller != null) controller.enabled = false;
        
        int routePattern = Random.Range(0, 3); 
        
        switch (routePattern)
        {
            case 0: 
                AddRowRoute(taxi);
                break;
            case 1: 
                AddSShapeRoute(taxi);
                break;
            case 2: 
                AddBackwardsRoute(taxi);
                break;
        }
        
        Destroy(taxi, taxiLifetime);
        StartCoroutine(DecrementCountAfterDelay());
    }
    
    void AddRowRoute(GameObject taxi)
    {
        var mover = taxi.AddComponent<StraightDrivingTaxi>();
        
        int edge = Random.Range(0, 4);
        
        float cameraZOffset = backgroundCamera != null ? backgroundCamera.transform.position.z : -94.8f;
        
        float halfWidth = (cameraViewWidth / 2f);   
        float halfHeight = (cameraViewHeight / 2f); 
        
        float randomHeight = Random.Range(minHeight, maxHeight);
        
        Vector3 spawnPos = Vector3.zero;
        Vector3 direction = Vector3.zero;
        
        switch (edge)
        {
            case 0: // Left Right
                spawnPos = new Vector3(
                    -halfWidth - spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                direction = Vector3.right;
                break;
                
            case 1: // Right Left
                spawnPos = new Vector3(
                    halfWidth + spawnDistance, 
                    randomHeight, 
                    cameraZOffset + Random.Range(-halfHeight + 5f, halfHeight - 5f)
                );
                direction = Vector3.left;
                break;
                
            case 2: // Top Bottom 
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset - halfHeight - spawnDistance
                );
                direction = Vector3.forward; 
                break;
                
            case 3: // Bottom Top 
                spawnPos = new Vector3(
                    Random.Range(-halfWidth + 5f, halfWidth - 5f), 
                    randomHeight, 
                    cameraZOffset + halfHeight + spawnDistance
                );
                direction = Vector3.back; 
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
        // S-shape
        var mover = taxi.AddComponent<SShapeTaxi>();
        
        float cameraZOffset = backgroundCamera != null ? backgroundCamera.transform.position.z : -94.8f;
        float halfHeight = (cameraViewHeight / 2f);
        float randomHeight = Random.Range(minHeight, maxHeight);
        
        // Spawn from bottom 
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

public class SShapeTaxi : MonoBehaviour
{
    [HideInInspector] public float forwardSpeed;
    [HideInInspector] public float waveAmplitude;
    [HideInInspector] public float waveFrequency;
    
    private float timeElapsed = 0f;
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
        
        // Move forward 
        float forwardMovement = forwardSpeed * Time.deltaTime;
        
        // Sideways wave 
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

public class BackwardsDrivingTaxi : MonoBehaviour
{
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public Vector3 faceDirection;
    [HideInInspector] public float speed;
    
    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        
        if (faceDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(faceDirection);
        }
    }
}