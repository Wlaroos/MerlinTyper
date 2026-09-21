using System;
using System.Text;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypingDisplayUI : MonoBehaviour
{
    [Header("Char Shake Settings")]
    private float _shakeDuration, _shakeMagnitude;
    private TMP_Text _wordDisplay;
    private Word _assignedWord;
    private Action<TypingDisplayUI> _onRecycle;

    private int _shakingCharIndex = -1;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _wordDisplay = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (WordManager.Instance != null)
        {
            _shakeDuration = WordManager.Instance.ShakeDuration;
            _shakeMagnitude = WordManager.Instance.ShakeMagnitude;
        }
    }

    public void Setup(Word word, Action<TypingDisplayUI> recycleCallback)
    {
        _assignedWord = word;
        _onRecycle = recycleCallback;
        _shakingCharIndex = -1;

        UpdateDisplay(_assignedWord, false);
    }

    public void ClearDisplay()
    {
        _assignedWord = null;
        _shakingCharIndex = -1;
        _onRecycle = null;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }

        if (_wordDisplay != null)
        {
            _wordDisplay.text = string.Empty;
        }
    }

    private void OnEnable()
    {
        WordManager.OnLetterTyped += OnLetterTyped;
        WordManager.OnLetterBackspace += OnLetterBackspace;
        WordManager.OnWordCompleted += OnWordCompleted;
    }

    private void OnDisable()
    {
        WordManager.OnLetterTyped -= OnLetterTyped;
        WordManager.OnLetterBackspace -= OnLetterBackspace;
        WordManager.OnWordCompleted -= OnWordCompleted;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }
        _shakingCharIndex = -1;
    }

    private void LateUpdate()
    {
        if (_shakingCharIndex < 0 || _wordDisplay == null || _assignedWord == null) return;

        // Force TMP to parse and lay out geometry for current frame
        _wordDisplay.ForceMeshUpdate();
        TMP_TextInfo textInfo = _wordDisplay.textInfo;

        // Map assigned word current index directly to TMP visible character quad
        int characterIndex = GetTMPCharacterIndexForTargetLetter(_shakingCharIndex, textInfo);
        if (characterIndex < 0 || characterIndex >= textInfo.characterCount) return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[characterIndex];
        if (!charInfo.isVisible) return;

        // Apply vertex jitter to the 4 vertices of the single character quad
        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

        Vector3 offset = new Vector3(
            UnityEngine.Random.Range(-1f, 1f) * _shakeMagnitude,
            UnityEngine.Random.Range(-1f, 1f) * _shakeMagnitude,
            0f
        );

        sourceVertices[vertexIndex + 0] += offset;
        sourceVertices[vertexIndex + 1] += offset;
        sourceVertices[vertexIndex + 2] += offset;
        sourceVertices[vertexIndex + 3] += offset;

        _wordDisplay.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    // Matches rawTargetIndex to its TMP character quad.
    // If the raw target character is a space, targets its underscore.
    private int GetTMPCharacterIndexForTargetLetter(int rawTargetIndex, TMP_TextInfo textInfo)
    {
        if (_assignedWord == null || string.IsNullOrEmpty(_assignedWord.Text)) return -1;
        if (rawTargetIndex < 0 || rawTargetIndex >= _assignedWord.Text.Length) return -1;

        bool isTargetingSpace = _assignedWord.Text[rawTargetIndex] == ' ';

        // Calculate the ordinal index for underscores (counts character slots up to rawTargetIndex)
        int underscoreOrdinal = rawTargetIndex;

        // Calculate the ordinal index for letters (counts non-space letters up to rawTargetIndex)
        int letterOrdinal = 0;
        for (int i = 0; i < rawTargetIndex; i++)
        {
            if (_assignedWord.Text[i] != ' ')
            {
                letterOrdinal++;
            }
        }

        int underscoreCounter = 0;
        int letterCounter = 0;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo info = textInfo.characterInfo[i];

            if (!info.isVisible) continue;

            if (info.character == '_')
            {
                if (isTargetingSpace && underscoreCounter == underscoreOrdinal)
                {
                    return i;
                }
                underscoreCounter++;
            }
            else
            {
                if (!isTargetingSpace && letterCounter == letterOrdinal)
                {
                    return i;
                }
                letterCounter++;
            }
        }

        return -1;
    }

    private void UpdateDisplay(Word word, bool showError = false)
    {
        if (_assignedWord == word && _wordDisplay != null)
        {
            _wordDisplay.text = FormatWordText(word, showError);
        }
    }

    private string FormatWordText(Word word, bool showError = false)
    {
        if (word == null || string.IsNullOrEmpty(word.Text)) return string.Empty;

        StringBuilder sb = new StringBuilder();
        string text = word.Text;

        for (int i = 0; i < text.Length; i++)
        {
            char rawChar = text[i];
            bool isSpace = rawChar == ' ';
            string displayChar = isSpace ? " " : rawChar.ToString();

            // <mspace=0.75em> ensures identical horizontal slot width
            sb.Append("<mspace=0.75em>");

            // Typed (Green letter over Green underline)
            if (i < word.CurrentIndex)
            {
                sb.Append($"<color=#00FF00>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></color>");
            }
            // Current Target (Red if error, Yellow if normal)
            else if (i == word.CurrentIndex)
            {
                if (showError)
                {
                    // Red letter and Red underline on error
                    sb.Append($"<color=#FF0000><b>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></b></color>");
                }
                else
                {
                    // Yellow letter and Yellow underline
                    sb.Append($"<color=#FFD700><b>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></b></color>");
                }
            }
            // Remaining (Grey letter over Grey underline)
            else
            {
                sb.Append($"<color=#CCCCCC>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></color>");
            }

            sb.Append("</mspace>");

            // Small gap between character slots
            sb.Append("<mspace=0.1em> </mspace>");
        }

        return sb.ToString();
    }

    private void OnLetterTyped(Word word, bool isCorrect)
    {
        if (_assignedWord == word)
        {
            if (!isCorrect)
            {
                TriggerCharShakeAndFlashRed();
            }
            else
            {
                _shakingCharIndex = -1;
                UpdateDisplay(word, false);
            }
        }
    }

    private void TriggerCharShakeAndFlashRed()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeAndFlashRoutine());
    }

    private IEnumerator ShakeAndFlashRoutine()
    {
        _shakingCharIndex = _assignedWord.CurrentIndex;
        
        UpdateDisplay(_assignedWord, showError: true);

        yield return new WaitForSeconds(_shakeDuration);

        _shakingCharIndex = -1;
        UpdateDisplay(_assignedWord, showError: false);

        if (_wordDisplay != null)
        {
            _wordDisplay.ForceMeshUpdate();
        }

        _shakeCoroutine = null;
    }

    private void OnLetterBackspace(Word word, bool isCorrect)
    {
        if (_assignedWord == word)
        {
            _shakingCharIndex = -1;
            UpdateDisplay(word, false);
        }
    }

    private void OnWordCompleted(Word word)
    {
        // Only clear if the completed word is still the currently assigned word
        if (_assignedWord == word)
        {
            _assignedWord = null;
            _shakingCharIndex = -1;
            _onRecycle?.Invoke(this);
        }
    }
}