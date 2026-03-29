using UnityEngine;

public class RulebookController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rulebookCanvas;
    
    [Tooltip("Drag the GameObject representing the closed book on your desk here.")]
    public GameObject deskRulebookIcon; 
    
    [Tooltip("Drag all of your rulebook page UI Panels here IN ORDER.")]
    public GameObject[] pages;
    
    [Header("Tactile UI Sounds")]
    public AudioSource uiFoleySource;
    public AudioClip bookOpenClip;
    public AudioClip bookCloseClip; // <--- The missing close clip variable!
    public AudioClip pageFlipClip;

    [Header("Audio Muffling Sabotage")]
    [Tooltip("Drag ANY audio source here that you want to muffle when reading.")]
    public AudioSource[] sourcesToMuffle; 
    [Range(0f, 1f)] public float muffledVolumeLevel = 0.2f;

    private float[] originalVolumes;
    private bool isBookOpen = false;
    private int currentPageIndex = 0;

    void Start()
    {
        // Save the original volume of every audio source in the list
        originalVolumes = new float[sourcesToMuffle.Length];
        for (int i = 0; i < sourcesToMuffle.Length; i++)
        {
            if (sourcesToMuffle[i] != null) originalVolumes[i] = sourcesToMuffle[i].volume;
        }
        
        rulebookCanvas.SetActive(false);
        if (deskRulebookIcon != null) deskRulebookIcon.SetActive(true);
    }

    // ---> THE CLEANLY REWRITTEN TOGGLE FUNCTION <---
    public void ToggleRulebook()
    {
        isBookOpen = !isBookOpen;
        
        rulebookCanvas.SetActive(isBookOpen);
        if (deskRulebookIcon != null) deskRulebookIcon.SetActive(!isBookOpen);
        
        if (isBookOpen)
        {
            currentPageIndex = 0;
            UpdatePageVisibility();
            if (uiFoleySource != null && bookOpenClip != null) uiFoleySource.PlayOneShot(bookOpenClip);
            
            // MUFFLE EVERYTHING!
            for (int i = 0; i < sourcesToMuffle.Length; i++)
            {
                if (sourcesToMuffle[i] != null) sourcesToMuffle[i].volume = originalVolumes[i] * muffledVolumeLevel;
            }
        }
        else 
        {
            if (uiFoleySource != null && bookCloseClip != null) uiFoleySource.PlayOneShot(bookCloseClip);
            
            // RESTORE EVERYTHING!
            for (int i = 0; i < sourcesToMuffle.Length; i++)
            {
                if (sourcesToMuffle[i] != null) sourcesToMuffle[i].volume = originalVolumes[i];
            }
        }
    }

    // Hook this to a "Next" button on your UI
    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
            if (uiFoleySource != null && pageFlipClip != null) uiFoleySource.PlayOneShot(pageFlipClip);
        }
    }

    // Hook this to a "Previous" button on your UI
    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
            if (uiFoleySource != null && pageFlipClip != null) uiFoleySource.PlayOneShot(pageFlipClip);
        }
    }

    private void UpdatePageVisibility()
    {
        // Loop through all pages. Turn them off, unless it matches our current index.
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
    }
}