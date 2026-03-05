using UnityEngine;

public class RulebookController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The massive 2D Canvas object that holds the Rulebook pages.")]
    public GameObject rulebookCanvas;

    [Header("Audio Muffling Sabotage")]
    [Tooltip("Drag the AudioSources here so we can muffle them when the book is open.")]
    public AudioSource ambientAudioSource;
    public AudioSource phoneAudioSource;
    
    [Tooltip("How quiet the game gets when reading (0.2 = 20% volume)")]
    [Range(0f, 1f)]
    public float muffledVolumeLevel = 0.2f;

    private float originalAmbientVol;
    private float originalPhoneVol;
    private bool isBookOpen = false;

    void Start()
    {
        // Save the original volumes so we can restore them when the book closes
        if (ambientAudioSource != null) originalAmbientVol = ambientAudioSource.volume;
        if (phoneAudioSource != null) originalPhoneVol = phoneAudioSource.volume;
        
        // Ensure the book starts closed
        rulebookCanvas.SetActive(false);
    }

    // Hook this up to a UI Button representing the closed binder on the desk, 
    // AND a "Close" button on the open binder pages.
    public void ToggleRulebook()
    {
        isBookOpen = !isBookOpen;
        rulebookCanvas.SetActive(isBookOpen);
        
        Debug.Log(isBookOpen ? "Rulebook Opened. Audio muffled." : "Rulebook Closed. Audio restored.");

        // Apply the audio sabotage!
        if (isBookOpen)
        {
            if (ambientAudioSource != null) ambientAudioSource.volume = originalAmbientVol * muffledVolumeLevel;
            if (phoneAudioSource != null) phoneAudioSource.volume = originalPhoneVol * muffledVolumeLevel;
        }
        else
        {
            if (ambientAudioSource != null) ambientAudioSource.volume = originalAmbientVol;
            if (phoneAudioSource != null) phoneAudioSource.volume = originalPhoneVol;
        }
    }
}