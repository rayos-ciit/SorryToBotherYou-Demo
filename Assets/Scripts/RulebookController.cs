using UnityEngine;

public class RulebookController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rulebookCanvas;
    
    [Tooltip("Drag all of your rulebook page UI Panels here IN ORDER.")]
    public GameObject[] pages;

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
        
        rulebookCanvas.SetActive(false);
    }

    public void ToggleRulebook()
    {
        isBookOpen = !isBookOpen;
        rulebookCanvas.SetActive(isBookOpen);
        
        if (isBookOpen)
        {
            // Always open to the first page
            currentPageIndex = 0;
            UpdatePageVisibility();
            
            if (ambientAudioSource != null) ambientAudioSource.volume = originalAmbientVol * muffledVolumeLevel;
            if (phoneAudioSource != null) phoneAudioSource.volume = originalPhoneVol * muffledVolumeLevel;
        }
        else
        {
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
        }
    }

    // Hook this to a "Previous" button on your UI
    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
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