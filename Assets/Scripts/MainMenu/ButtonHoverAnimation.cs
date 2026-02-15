using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to each button for hover/click animations
/// Makes buttons scale up on hover and bounce on click
/// </summary>
public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.1f;           // Scale when hovering
    public float hoverDuration = 0.2f;        // How fast to scale
    
    [Header("Click Settings")]
    public float clickScale = 0.95f;          // Scale when clicking
    public float clickDuration = 0.1f;        // How fast to click
    
    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleVelocity;
    private bool isHovering = false;
    private bool isClicking = false;
    
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }
    
    void Update()
    {
        // Smooth scale transition
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            targetScale, 
            Time.deltaTime / (isClicking ? clickDuration : hoverDuration) * 10f
        );
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        if (!isClicking)
        {
            targetScale = originalScale * hoverScale;
        }
        
        // Play hover sound
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        if (!isClicking)
        {
            targetScale = originalScale;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isClicking = true;
        targetScale = originalScale * clickScale;
        
        // Play click sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isClicking = false;
        
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
        }
        else
        {
            targetScale = originalScale;
        }
    }
}