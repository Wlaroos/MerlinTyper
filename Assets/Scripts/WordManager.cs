using System;
using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private WordBank[] _wordBanks;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeMagnitude = 3f;
    public float ShakeDuration => _shakeDuration;
    public float ShakeMagnitude => _shakeMagnitude;
    public List<Word> ActiveWords { get; private set; } = new List<Word>();
    public Word TargetWord { get; private set; }

    private readonly HashSet<char> _usedStartingLetters = new HashSet<char>();

    // Events
    public static event Action<Word> OnWordAdded;
    public static event Action<Word> OnWordTargeted;
    public static event Action<Word> OnWordUntargeted;
    public static event Action<Word, bool> OnLetterTyped;
    public static event Action<Word, bool> OnLetterBackspace;
    public static event Action<Word> OnWordCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Word RequestWordForHotSpot(WordBank customBank = null)
    {
        WordBank bankToUse = customBank != null 
            ? customBank 
            : (_wordBanks != null && _wordBanks.Length > 0 ? _wordBanks[UnityEngine.Random.Range(0, _wordBanks.Length)] : null);

        if (bankToUse == null) return null;

        string newString = bankToUse.GetRandomWord(_usedStartingLetters);
        if (string.IsNullOrEmpty(newString)) return null;

        Word newWord = new Word(newString);
        
        ActiveWords.Add(newWord);
        _usedStartingLetters.Add(newWord.GetFirstChar());

        OnWordAdded?.Invoke(newWord);
        return newWord;
    }

    public void ReleaseWord(Word word)
    {
        if (word == null) return;

        if (ActiveWords.Contains(word))
        {
            ActiveWords.Remove(word);
        }

        _usedStartingLetters.Remove(word.GetFirstChar());

        if (TargetWord == word)
        {
            TargetWord = null;
            OnWordUntargeted?.Invoke(word);
        }
    }

    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char c in input)
        {
            if (c == '\n' || c == '\r') continue;

            // Backspace handling
            if (c == '\b')
            {
                if (TargetWord != null && TargetWord.CurrentIndex > 0)
                {
                    TargetWord.Backspace();
                    OnLetterBackspace?.Invoke(TargetWord, true);

                    if (TargetWord.CurrentIndex == 0)
                    {
                        Word untargeted = TargetWord;
                        TargetWord = null;
                        OnWordUntargeted?.Invoke(untargeted);
                    }
                }
                continue;
            }

            // Locked Target Word input
            if (TargetWord != null)
            {
                bool isCorrect = TargetWord.TypeLetter(c);
                
                // Broadcast letter typed event (isCorrect will be false on wrong keypress)
                OnLetterTyped?.Invoke(TargetWord, isCorrect);

                if (isCorrect && TargetWord.IsCompleted())
                {
                    CompleteWord(TargetWord);
                }
            }
            // Search Active Words
            else
            {
                foreach (Word word in ActiveWords)
                {
                    if (word.GetNextChar().ToString().Equals(c.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        TargetWord = word;
                        TargetWord.TypeLetter(c);

                        OnWordTargeted?.Invoke(TargetWord);
                        OnLetterTyped?.Invoke(TargetWord, true);

                        if (TargetWord.IsCompleted())
                        {
                            CompleteWord(TargetWord);
                        }
                        break;
                    }
                }
            }
        }
    }

    private void CompleteWord(Word word)
    {
        OnWordCompleted?.Invoke(word);
        ReleaseWord(word);
    }
}