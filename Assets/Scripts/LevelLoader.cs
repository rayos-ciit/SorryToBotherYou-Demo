using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("UI Mappings")]
    [Tooltip("The black UI image that covers the whole screen.")]
    public Image fadeImage;

    [Header("Settings")]
    [Tooltip("How fast the screen fades. (Lower = Slower)")]
    public float fadeSpeed = 1.5f;

    void Start()
    {
        // When any scene starts, automatically fade from black to clear
        if (fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    // Call this from other scripts to trigger the transition
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1); // Start fully solid black

        // Slowly reduce the alpha until it is 0 (invisible)
        while (fadeImage.color.a > 0)
        {
            float newAlpha = fadeImage.color.a - (Time.deltaTime * fadeSpeed);
            fadeImage.color = new Color(0, 0, 0, newAlpha);
            yield return null; // Wait a frame
        }

        // Hide the image so it doesn't block mouse clicks!
        fadeImage.gameObject.SetActive(false); 
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // Start completely clear

        // Slowly increase the alpha until it is 1 (solid black)
        while (fadeImage.color.a < 1)
        {
            float newAlpha = fadeImage.color.a + (Time.deltaTime * fadeSpeed);
            fadeImage.color = new Color(0, 0, 0, newAlpha);
            yield return null; 
        }

        // Once the screen is completely black, load the next scene!
        SceneManager.LoadScene(sceneName);
    }
}