using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    [SerializeField] private KeyboardLayoutSO _activeLayout;
    [SerializeField] private GameObject _keyPrefab;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private Transform _rowParent;
    [SerializeField] private float _keyScale = 1f;

    private readonly Dictionary<KeyCode, KeyView> _keyLookup = new Dictionary<KeyCode, KeyView>();
    private readonly List<List<KeyView>> _rowViews = new List<List<KeyView>>();
    private readonly HashSet<KeyView> _activeHighlights = new HashSet<KeyView>();

    private void OnEnable()
    {
        WordManager.KeyPressedEvent += OnKeyPressed;
        WordManager.WordTargetedEvent += OnWordTargeted;
        WordManager.WordUntargetedEvent += OnWordUntargeted;
        WordManager.WordCompletedEvent += OnWordCompleted;
        WordManager.LetterTypedEvent += OnLetterTyped;
    }

    private void OnDisable()
    {
        WordManager.KeyPressedEvent -= OnKeyPressed;
        WordManager.WordTargetedEvent -= OnWordTargeted;
        WordManager.WordUntargetedEvent -= OnWordUntargeted;
        WordManager.WordCompletedEvent -= OnWordCompleted;
        WordManager.LetterTypedEvent -= OnLetterTyped;
    }

    private void Awake()
    {
        BuildKeyboard();
    }

    private void BuildKeyboard()
    {
        _rowViews.Clear();
        foreach (var rowDef in _activeLayout.rows)
        {
            GameObject rowGO = Instantiate(_rowPrefab, _rowParent);
            RectTransform rowTransform = rowGO.GetComponent<RectTransform>();
            rowTransform.sizeDelta = new Vector2(rowTransform.sizeDelta.x, 13 * _keyScale);

            List<KeyView> currentRowViews = new List<KeyView>();

            foreach (var keyData in rowDef.keys)
            {
                KeyView view = Instantiate(_keyPrefab, rowGO.transform).GetComponent<KeyView>();
                view.Initialize(keyData);

                RectTransform viewTransform = view.GetComponent<RectTransform>();
                viewTransform.sizeDelta = new Vector2(keyData.highlightedSprite.rect.width * _keyScale, keyData.highlightedSprite.rect.height * _keyScale);

                if (!_keyLookup.ContainsKey(keyData.keyCode))
                {
                    _keyLookup.Add(keyData.keyCode, view);
                }

                currentRowViews.Add(view);
            }

            _rowViews.Add(currentRowViews);
        }
    }

    private void Update()
    {
        // Continuously poll physical press/release state per key
        foreach (var kvp in _keyLookup)
        {
            KeyCode code = kvp.Key;
            KeyView keyView = kvp.Value;

            if (Input.GetKeyDown(code))
            {
                keyView.SetState(KeyView.KeyState.Pressed);
            }
            else if (Input.GetKeyUp(code))
            {
                // If the key is currently highlighted by the tutorial, revert to hovered instead of default
                if (_activeHighlights.Contains(keyView))
                {
                    keyView.SetState(KeyView.KeyState.Highlighted);
                }
                else
                {
                    keyView.SetState(KeyView.KeyState.Default);
                }
            }
        }
    }

    private void OnKeyPressed(WordManager.KeyPressInfo info)
    {
        if (info.KeyCode == KeyCode.None) return;

        if (_keyLookup.TryGetValue(info.KeyCode, out KeyView keyView))
        {
            FlashFeedback(keyView, info.IsCorrect);
        }
    }

    public void FlashFeedback(KeyView keyView, bool isCorrect)
    {
        Color feedbackColor = isCorrect ? Color.green : Color.red;
        StartCoroutine(FlashRoutine(feedbackColor, keyView));
    }

    private IEnumerator FlashRoutine(Color targetColor, KeyView keyView)
    {
        keyView.SetColor(targetColor);
        yield return new WaitForSeconds(1f);
        keyView.SetColor(Color.white); 

        // Restore highlight state if active, otherwise default
        if (_activeHighlights.Contains(keyView))
        {
            keyView.SetState(KeyView.KeyState.Highlighted);
        }
        else
        {
            keyView.SetState(KeyView.KeyState.Default);
        }
    }

    private void OnWordTargeted(Word word) { }
    private void OnWordUntargeted(Word word) { }
    private void OnWordCompleted(Word word) { }

    private void OnLetterTyped(Word word, char typedChar, bool isCorrect)
    {
        KeyCode code = WordManager.Instance.ConvertCharToKeyCode(typedChar);
        if (_keyLookup.TryGetValue(code, out KeyView keyView))
        {
            FlashFeedback(keyView, isCorrect);
        }
    }

    public void HighlightRow(int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < _rowViews.Count)
        {
            foreach (var keyView in _rowViews[rowIndex])
            {
                keyView.SetState(KeyView.KeyState.Highlighted);
                _activeHighlights.Add(keyView);
            }
        }
    }

    public void HighlightKey(KeyCode key)
    {
        if (_keyLookup.TryGetValue(key, out KeyView keyView))
        {
            keyView.SetState(KeyView.KeyState.Highlighted);
            _activeHighlights.Add(keyView);
        }
    }

    public void ClearHighlights()
    {
        foreach (var keyView in _activeHighlights)
        {
            if (keyView != null)
            {
                keyView.SetState(KeyView.KeyState.Default);
            }
        }
        _activeHighlights.Clear();
    }
}