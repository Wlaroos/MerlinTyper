using System;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WordBank wordBank;

    // Active tracking
    public Word CurrentWord { get; private set; }

    // Events for UI and Systems
    public static event Action<Word> OnWordStarted;
    public static event Action<Word, bool> OnLetterTyped; // (word, isCorrect)
    public static event Action<Word, bool> OnLetterBackspace; // (word, isCorrect)
    public static event Action<Word> OnWordCompleted;

    private void Start()
    {
        SetNextWord();
    }

    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        // Unity's native string input captures keyboard chars across layouts
        string input = Input.inputString;

        if (string.IsNullOrEmpty(input) || CurrentWord == null)
            return;

        foreach (char c in input)
        {
            // Ignore control keys like backspace or enter
            if (c == '\n' || c == '\r')
                continue;
            
            if (c == '\b') // Backspace
            {
                bool canBackspace = CurrentWord.CurrentIndex > 0; // Can only backspace if there's something typed
                if (canBackspace)
                {
                    CurrentWord.Backspace();
                }
                OnLetterBackspace?.Invoke(CurrentWord, canBackspace);
                continue;
            }

            bool isCorrect = CurrentWord.TypeLetter(c);
            OnLetterTyped?.Invoke(CurrentWord, isCorrect);

            if (CurrentWord.IsCompleted())
            {
                OnWordCompleted?.Invoke(CurrentWord);
                SetNextWord();
                break;
            }
        }
    }

    public void SetNextWord()
    {
        string newString = wordBank != null ? wordBank.GetRandomWord() : "unity";
        CurrentWord = new Word(newString);
        OnWordStarted?.Invoke(CurrentWord);
    }
}