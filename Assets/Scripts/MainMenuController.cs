using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Transitions")]
    public LevelLoader levelLoader;
    
    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider ambienceSlider;
    
    [Header("Scene Management")]
    public string gameSceneName = "SampleScene"; 

    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject howToPlayPanel;

    void Start()
    {
        // Load all saved volumes, defaulting to 0.75f (75%)
        LoadAndSetVolume("MasterVolume", masterSlider);
        LoadAndSetVolume("SFXVolume", sfxSlider);
        LoadAndSetVolume("AmbienceVolume", ambienceSlider);

        ShowMainPanel();
    }

    public void StartGame() => levelLoader.LoadSceneWithFade(gameSceneName); 
    public void QuitGame() => Application.Quit();

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void ShowHowToPlay()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
    }
    
    // --- NEW STREAMLINED AUDIO LOGIC ---

    private void LoadAndSetVolume(string paramName, Slider slider)
    {
        float savedVol = PlayerPrefs.GetFloat(paramName, 0.75f);
        if (slider != null) slider.value = savedVol;
        
        // Ensure the mixer updates immediately on startup (avoiding 0 values if muted)
        float mixerVol = savedVol <= 0.001f ? -80f : Mathf.Log10(savedVol) * 20;
        mainMixer.SetFloat(paramName, mixerVol);
    }

    // Hook these three methods directly to their respective UI Sliders OnValueChanged events!
    public void SetMasterVolume(float value) => UpdateMixerAndSave("MasterVolume", value);
    public void SetSFXVolume(float value) => UpdateMixerAndSave("SFXVolume", value);
    public void SetAmbienceVolume(float value) => UpdateMixerAndSave("AmbienceVolume", value);

    private void UpdateMixerAndSave(string paramName, float sliderValue)
    {
        // Math tip: If the slider hits 0, drop the mixer to -80db (total silence)
        float mixerVol = sliderValue <= 0.001f ? -80f : Mathf.Log10(sliderValue) * 20;
        
        mainMixer.SetFloat(paramName, mixerVol);
        PlayerPrefs.SetFloat(paramName, sliderValue);
        PlayerPrefs.Save();
    }
}