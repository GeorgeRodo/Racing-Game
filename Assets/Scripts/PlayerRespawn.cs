using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform lastCheckpoint; 
    public float respawnDelay = 0.5f;

    [Header("UI Fade Settings")]
    public Image fadeImage; 
    public float fadeSpeed = 2f;

    [Header("Audio Settings")]
    public AudioSource sfxSource; 
    public AudioClip waterBubbleClip; 

    private Rigidbody rb;
    private CustomVehicleController vehicleController;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vehicleController = GetComponent<CustomVehicleController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") && !isRespawning)
        {
            if (sfxSource != null && waterBubbleClip != null)
                sfxSource.PlayOneShot(waterBubbleClip);

            StartCoroutine(RespawnSequence());
        }

        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpoint = other.transform;
        }
    }

    IEnumerator RespawnSequence()
    {
        isRespawning = true;

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetFadeAlpha(alpha);
            yield return null;
        }

        if (vehicleController != null) vehicleController.enabled = false;

        Vector3 spawnPosition = lastCheckpoint.position - (lastCheckpoint.forward * 5f) + Vector3.up;
        transform.position = spawnPosition;
        transform.rotation = lastCheckpoint.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(respawnDelay);

        if (vehicleController != null) vehicleController.enabled = true;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetFadeAlpha(alpha);
            yield return null;
        }

        isRespawning = false;
    }

    private void SetFadeAlpha(float a)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }
}