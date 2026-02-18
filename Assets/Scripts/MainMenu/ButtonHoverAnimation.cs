using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.1f;           
    public float hoverDuration = 0.2f;        
    
    [Header("Click Settings")]
    public float clickScale = 0.95f;         
    public float clickDuration = 0.1f;      
    
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