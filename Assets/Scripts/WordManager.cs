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

    [Header("Input Settings")]
    [Tooltip("If false, holding down a physical key will only register a single press until released.")]
    [SerializeField] private bool _allowKeyHoldRepeat = false;

    public float ShakeDuration => _shakeDuration;
    public float ShakeMagnitude => _shakeMagnitude;
    public List<Word> ActiveWords { get; private set; } = new List<Word>();
    public Word TargetWord { get; private set; }

    private readonly HashSet<char> _usedStartingLetters = new HashSet<char>();

    // Tracks physical key held state to prevent auto-repeat
    private readonly HashSet<KeyCode> _heldKeys = new HashSet<KeyCode>();

    // Lookup table for zero-allocation keycode resolution
    private static readonly KeyCode[] CharToKeyCodeMap = new KeyCode[128];

    static WordManager()
    {
        for (char c = 'a'; c <= 'z'; c++)
        {
            if (Enum.TryParse(c.ToString(), true, out KeyCode code))
            {
                CharToKeyCodeMap[c] = code;
                CharToKeyCodeMap[char.ToUpper(c)] = code;
            }
        }
        CharToKeyCodeMap[' '] = KeyCode.Space;
        CharToKeyCodeMap['\b'] = KeyCode.Backspace;
    }

    public struct KeyPressInfo
    {
        public char Character;
        public KeyCode KeyCode;
        public bool IsCorrect;
        public bool IsBackspace;
    }

    // Events
    public static event Action<Word> WordAddedEvent;
    public static event Action<Word> WordTargetedEvent;
    public static event Action<Word> WordUntargetedEvent;
    public static event Action<Word, char, bool> LetterTypedEvent;
    public static event Action<Word, bool> LetterBackspaceEvent;
    public static event Action<Word> WordCompletedEvent;
    public static event Action<KeyPressInfo> KeyPressedEvent;

    // Stats
    [SerializeField] private TextMeshProUGUI _statsDisplay;
    private int _correctKeysTyped = 0;
    private int _wrongKeysTyped = 0;
    private int _totalKeysTyped = 0;
    private int _totalWordsCompleted = 0;
    private float _totalTypingTime = 0f;
    private float _averageTypingSpeed = 0f;

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
        // Release tracking for held keys
        if (!_allowKeyHoldRepeat && _heldKeys.Count > 0)
        {
            _heldKeys.RemoveWhere(code => Input.GetKeyUp(code));
        }

        if (ActiveWords.Count > 0)
        {
            _totalTypingTime += Time.deltaTime;
            RecalculateWPM();
        }

        ProcessInput();

        if (_statsDisplay != null)
        {
            _statsDisplay.text = $"Correct Keys: {_correctKeysTyped}\n" +
                                 $"Wrong Keys: {_wrongKeysTyped}\n" +
                                 $"Total Keys: {_totalKeysTyped}\n" +
                                 $"Words Completed: {_totalWordsCompleted}";
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

        WordAddedEvent?.Invoke(newWord);
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
            WordUntargetedEvent?.Invoke(word);
        }
    }

    private void ProcessInput()
    {
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char c in input)
        {
            if (c == '\n' || c == '\r') continue;

            KeyCode code = ConvertCharToKeyCode(c);

            // Ignore input if repeat holds are disallowed and key is currently held down
            if (!_allowKeyHoldRepeat && code != KeyCode.None && _heldKeys.Contains(code))
            {
                continue;
            }

            // Register keypress as active
            if (code != KeyCode.None)
            {
                _heldKeys.Add(code);
            }

            // Backspace handling
            if (c == '\b')
            {
                KeyPressedEvent?.Invoke(new KeyPressInfo 
                { 
                    Character = c, 
                    KeyCode = KeyCode.Backspace, 
                    IsCorrect = true, 
                    IsBackspace = true 
                });

                if (TargetWord != null && TargetWord.CurrentIndex > 0)
                {
                    TargetWord.Backspace();
                    LetterBackspaceEvent?.Invoke(TargetWord, true);

                    if (TargetWord.CurrentIndex == 0)
                    {
                        Word untargeted = TargetWord;
                        TargetWord = null;
                        WordUntargetedEvent?.Invoke(untargeted);
                    }
                }
                continue;
            }

            // Locked Target Word input
            if (TargetWord != null)
            {
                bool isCorrect = TargetWord.TypeLetter(c);
                RecordKeypress(isCorrect);

                KeyPressedEvent?.Invoke(new KeyPressInfo 
                { 
                    Character = c, 
                    KeyCode = code, 
                    IsCorrect = isCorrect, 
                    IsBackspace = false 
                });

                LetterTypedEvent?.Invoke(TargetWord, c, isCorrect);

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

                        RecordKeypress(true);

                        KeyPressedEvent?.Invoke(new KeyPressInfo 
                        { 
                            Character = c, 
                            KeyCode = code, 
                            IsCorrect = true, 
                            IsBackspace = false 
                        });

                        WordTargetedEvent?.Invoke(TargetWord);
                        LetterTypedEvent?.Invoke(TargetWord, c, true);

                        if (TargetWord.IsCompleted())
                        {
                            CompleteWord(TargetWord);
                        }
                        break;
                    }
                }

                if (!matchedWord)
                {
                    RecordKeypress(false);

                    KeyPressedEvent?.Invoke(new KeyPressInfo 
                    { 
                        Character = c, 
                        KeyCode = code, 
                        IsCorrect = false, 
                        IsBackspace = false 
                    });
                }
            }
        }
    }

    public KeyCode ConvertCharToKeyCode(char c)
    {
        if (c < 128)
        {
            return CharToKeyCodeMap[c];
        }
        return KeyCode.None;
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
        WordCompletedEvent?.Invoke(word);
    }

    private void RecalculateWPM()
    {
        if (_totalTypingTime <= 0f) return;

        float minutes = _totalTypingTime / 60f;
        _averageTypingSpeed = (_correctKeysTyped / 5f) / minutes;
    }
}