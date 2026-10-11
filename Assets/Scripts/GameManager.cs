using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private SpriteRenderer _sr;
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        StartCoroutine(FadeScreen(true));
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void NextScene()
    {
        StartCoroutine(FadeScreen(false));
    }

    private IEnumerator FadeScreen(bool isFadingIn)
    {
        _sr.enabled = true;
        _sr.color = new Color(0, 0, 0, isFadingIn ? 0 : 1); // Start with transparent black if fading in, opaque if fading out
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
        if (!isFadingIn)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
