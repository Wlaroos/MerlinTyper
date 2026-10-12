using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private SpriteRenderer _sr;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically fade back in whenever any scene finishes loading
        StartCoroutine(FadeScreen(true));
    }

    void Update()
    {
        // GetGey for modifier keys
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }

    public void NextScene()
    {
        StartCoroutine(FadeScreen(false, SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void FirstScene()
    {
        StartCoroutine(FadeScreen(false, 0));
    }
    public void RestartScene()
    {
        StartCoroutine(FadeScreen(false, SceneManager.GetActiveScene().buildIndex));
    }

    private IEnumerator FadeScreen(bool isFadingIn, int sceneBuildIndex = -1)
    {
        _sr.enabled = true;
        _sr.color = new Color(0, 0, 0, isFadingIn ? 0 : 1); 
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            if (isFadingIn)
                alpha = 1 - alpha; // Fade in
            _sr.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Load the scene only after the fade-out is complete
        if (!isFadingIn && sceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }
}