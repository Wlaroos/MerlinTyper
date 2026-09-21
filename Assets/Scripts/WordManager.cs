using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // Stats
    [SerializeField] private TextMeshProUGUI _statsDisplay;
    private int _correctKeysTyped = 0;
    private int _wrongKeysTyped = 0;
    private int _totalKeysTyped = 0;
    private int _totalWordsCompleted = 0;
    private float _totalTypingTime = 0f;
    private float _averageTypingSpeed = 0f; // Words Per Minute (WPM)

    public int CorrectKeysTyped => _correctKeysTyped;
    public int WrongKeysTyped => _wrongKeysTyped;
    public int TotalKeysTyped => _totalKeysTyped;
    public int TotalWordsCompleted => _totalWordsCompleted;
    public float TotalTypingTime => _totalTypingTime;
    public float AverageTypingSpeed => _averageTypingSpeed;

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

    private void Update()
    {
        // Track typing time whenever words are active on screen
        if (ActiveWords.Count > 0)
        {
            _totalTypingTime += Time.deltaTime;
            RecalculateWPM();
        }

        ProcessInput();

        _statsDisplay.text = //$"WPM: {_averageTypingSpeed:F1}\n" +
                             $"Correct Keys: {_correctKeysTyped}\n" +
                             $"Wrong Keys: {_wrongKeysTyped}\n" +
                             $"Total Keys: {_totalKeysTyped}\n" +
                             $"Words Completed: {_totalWordsCompleted}";
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
                
                // Track keypress stats
                RecordKeypress(isCorrect);

                // Broadcast letter typed event
                OnLetterTyped?.Invoke(TargetWord, isCorrect);

                if (isCorrect && TargetWord.IsCompleted())
                {
                    CompleteWord(TargetWord);
                }
            }
            // Search Active Words
            else
            {
                bool matchedWord = false;

                foreach (Word word in ActiveWords)
                {
                    if (word.GetNextChar().ToString().Equals(c.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        matchedWord = true;
                        TargetWord = word;
                        TargetWord.TypeLetter(c);

                        // Track keypress stats
                        RecordKeypress(true);

                        OnWordTargeted?.Invoke(TargetWord);
                        OnLetterTyped?.Invoke(TargetWord, true);

                        if (TargetWord.IsCompleted())
                        {
                            CompleteWord(TargetWord);
                        }
                        break;
                    }
                }

                // Increment misstrokes when typing while no word is locked and no starting letter matches
                if (!matchedWord)
                {
                    RecordKeypress(false);
                }
            }
        }
    }

    private void RecordKeypress(bool isCorrect)
    {
        _totalKeysTyped++;

        if (isCorrect)
        {
            _correctKeysTyped++;
        }
        else
        {
            _wrongKeysTyped++;
        }

        RecalculateWPM();
    }

    private void CompleteWord(Word word)
    {
        _totalWordsCompleted++;
        RecalculateWPM();

        ReleaseWord(word);
        OnWordCompleted?.Invoke(word);
    }

    private void RecalculateWPM()
    {
        if (_totalTypingTime <= 0f) return;

        // Standard typing speed calculation (1 word = 5 keypresses)
        float minutes = _totalTypingTime / 60f;
        _averageTypingSpeed = (_correctKeysTyped / 5f) / minutes;
    }
}