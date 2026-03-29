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
    public AudioSource ambientAudioSource;
    public AudioSource phoneAudioSource;
    [Range(0f, 1f)] public float muffledVolumeLevel = 0.2f;

    private float originalAmbientVol;
    private float originalPhoneVol;
    private bool isBookOpen = false;
    private int currentPageIndex = 0;

    void Start()
    {
        if (ambientAudioSource != null) originalAmbientVol = ambientAudioSource.volume;
        if (phoneAudioSource != null) originalPhoneVol = phoneAudioSource.volume;
        
        // Ensure the game starts with the UI closed and the desk item visible
        rulebookCanvas.SetActive(false);
        if (deskRulebookIcon != null) deskRulebookIcon.SetActive(true);
    }

    // ---> THE CLEANLY REWRITTEN TOGGLE FUNCTION <---
    public void ToggleRulebook()
    {
        isBookOpen = !isBookOpen;
        
        // 1. Toggle UI and Desk Icon Visiblity
        rulebookCanvas.SetActive(isBookOpen);
        if (deskRulebookIcon != null) deskRulebookIcon.SetActive(!isBookOpen);
        
        // 2. Handle Opening the Book
        if (isBookOpen)
        {
            currentPageIndex = 0;
            UpdatePageVisibility();
            
            // Play Open Sound
            if (uiFoleySource != null && bookOpenClip != null) uiFoleySource.PlayOneShot(bookOpenClip);
            
            // Muffle Background Audio
            if (ambientAudioSource != null) ambientAudioSource.volume = originalAmbientVol * muffledVolumeLevel;
            if (phoneAudioSource != null) phoneAudioSource.volume = originalPhoneVol * muffledVolumeLevel;
        }
        // 3. Handle Closing the Book
        else 
        {
            // Play Close Sound
            if (uiFoleySource != null && bookCloseClip != null) uiFoleySource.PlayOneShot(bookCloseClip);
            
            // Restore Background Audio
            if (ambientAudioSource != null) ambientAudioSource.volume = originalAmbientVol;
            if (phoneAudioSource != null) phoneAudioSource.volume = originalPhoneVol;
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