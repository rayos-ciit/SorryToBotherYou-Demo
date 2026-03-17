using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("Drag your Pause Menu UI Panel here.")]
    public GameObject pauseMenuUI;
    
    private bool isPaused = false;

    void Start()
    {
        // Ensure it is hidden when the shift starts
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Toggle the menu when the player hits the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unfreezes the game and timers
        AudioListener.pause = false; // Unmutes all audio
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freezes everything
        AudioListener.pause = true; // Mutes all AudioSources instantly
        isPaused = true;
    }

    public void QuitToMainMenu()
    {
        // ALWAYS reset time and audio before loading a new scene!
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu"); 
    }
}