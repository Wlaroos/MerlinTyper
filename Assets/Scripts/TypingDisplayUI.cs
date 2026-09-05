using TMPro;
using UnityEngine;

public class TypingDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text wordDisplay;

    private void OnEnable()
    {
        WordManager.OnWordStarted += UpdateDisplay;
        WordManager.OnLetterTyped += OnLetterTyped;
        WordManager.OnLetterBackspace += OnLetterBackspace;
    }

    private void OnDisable()
    {
        WordManager.OnWordStarted -= UpdateDisplay;
        WordManager.OnLetterTyped -= OnLetterTyped;
        WordManager.OnLetterBackspace -= OnLetterBackspace;
    }

    private void UpdateDisplay(Word word)
    {
        if (wordDisplay != null && word != null)
        {
            wordDisplay.text = word.GetFormattedText();
        }
    }

    private void OnLetterTyped(Word word, bool isCorrect)
    {
        UpdateDisplay(word);
        
        if (!isCorrect)
        {
            // Do stuff here
        }
    }

    private void OnLetterBackspace(Word word, bool isCorrect)
    {
        UpdateDisplay(word);
        
        if (!isCorrect)
        {
            // Do stuff here
        }
    }
}