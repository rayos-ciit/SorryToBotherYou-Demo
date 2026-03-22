using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour
{
    [Header("Transitions")]
    public LevelLoader levelLoader;
    
    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    
    [Header("Scene Management")]
    [Tooltip("The exact name of your gameplay scene in the Build Settings.")]
    public string gameSceneName = "SampleScene"; 

    [Header("Menu Panels")]
    [Tooltip("Drag the Main Menu buttons panel here.")]
    public GameObject mainPanel;
    [Tooltip("Drag the Options panel here.")]
    public GameObject optionsPanel;
    [Tooltip("Drag the How To Play panel here.")]
    public GameObject howToPlayPanel;

    void Start()
    {
        // Ensure only the main buttons are showing when the game boots up
        ShowMainPanel();
    }

    // --- GAME LOOP BUTTONS ---
    public void StartGame()
    {
        Debug.Log("Starting Shift with transition...");
        // Tell the LevelLoader to do its thing instead of instantly loading!
        levelLoader.LoadSceneWithFade(gameSceneName); 
    }
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // --- NAVIGATION BUTTONS ---
    
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

    // Call this from the "Back" buttons inside the Options and How To Play menus
    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
    }
    
    // Called dynamically by the UI Slider
    public void SetVolume(float sliderValue)
    {
        // Converts the 0-1 slider value into a -80dB to 0dB logarithmic scale
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }
}