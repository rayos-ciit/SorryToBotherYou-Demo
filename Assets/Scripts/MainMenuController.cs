using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI; // Needed to talk to the Slider!

public class MainMenuController : MonoBehaviour
{
    [Header("Transitions")]
    public LevelLoader levelLoader;
    
    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider volumeSlider; // Drag your slider here in the inspector
    
    [Header("Scene Management")]
    [Tooltip("The exact name of your gameplay scene in the Build Settings.")]
    public string gameSceneName = "SampleScene"; 

    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject howToPlayPanel;

    void Start()
    {
        // 1. Load the saved volume (or default to 0.75 if it's their first time playing)
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        
        // 2. Set the physical UI slider to match the saved number
        if (volumeSlider != null) volumeSlider.value = savedVol;
        
        // 3. Actually apply the math to the Audio Engine
        SetVolume(savedVol);

        ShowMainPanel();
    }

    public void StartGame()
    {
        levelLoader.LoadSceneWithFade(gameSceneName); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }

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
    
    public void SetVolume(float sliderValue)
    {
        // Apply the volume to the mixer
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        
        // Save the setting permanently to the player's hard drive!
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
        PlayerPrefs.Save();
    }
}