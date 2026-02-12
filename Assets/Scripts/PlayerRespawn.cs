using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public Transform lastCheckpoint; 
    public Image fadeImage;          
    public float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioSource sfxSource; // Το component που θα παίξει τον ήχο
    public AudioClip waterBubbleClip; // Το αρχείο ήχου (.mp3 ή .wav)

    private Rigidbody rb;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider foreign)
    {
        if (foreign.gameObject.CompareTag("Water") && !isRespawning)
        {
            if (sfxSource != null && waterBubbleClip != null)
            {
                sfxSource.PlayOneShot(waterBubbleClip);
            }

            StartCoroutine(RespawnSequence());
        }

        if (foreign.gameObject.CompareTag("Water") && !isRespawning)
        {
            StartCoroutine(RespawnSequence());
        }

        if (foreign.gameObject.CompareTag("Checkpoint"))
        {
            lastCheckpoint = foreign.transform;
        }
    }

    IEnumerator RespawnSequence()
    {
        isRespawning = true;

        // 1. Fade to Black
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (GetComponent<CustomVehicleController>() != null)
            GetComponent<CustomVehicleController>().enabled = false;

        Vector3 spawnPos = lastCheckpoint.position - (lastCheckpoint.forward * 5f);
        spawnPos.y += 1f;

        transform.position = spawnPos;
        transform.rotation = lastCheckpoint.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        if (GetComponent<CustomVehicleController>() != null)
            GetComponent<CustomVehicleController>().enabled = true;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        isRespawning = false;
    }
}