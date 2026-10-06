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

    private void Start()
    {
        BuildKeyboard();
    }

    private void BuildKeyboard()
    {
        foreach (var rowDef in _activeLayout.rows)
        {
            GameObject rowGO = Instantiate(_rowPrefab, _rowParent);
            RectTransform rowTransform = rowGO.GetComponent<RectTransform>();
            rowTransform.sizeDelta = new Vector2(rowTransform.sizeDelta.x, 13 * _keyScale);

            foreach (var keyData in rowDef.keys)
            {
                KeyView view = Instantiate(_keyPrefab, rowGO.transform).GetComponent<KeyView>();
                view.Initialize(keyData);

                RectTransform viewTransform = view.GetComponent<RectTransform>();
                viewTransform.sizeDelta = new Vector2(keyData.hoveredSprite.rect.width * _keyScale, keyData.hoveredSprite.rect.height * _keyScale);

                if (!_keyLookup.ContainsKey(keyData.keyCode))
                {
                    _keyLookup.Add(keyData.keyCode, view);
                }
            }
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
                keyView.SetState(KeyView.KeyState.Default);
            }
        }
    }

    private void OnKeyPressed(WordManager.KeyPressInfo info)
    {
        // Ignore backspace if you don't want a key visual for it, or handle it if mapped
        if (info.KeyCode == KeyCode.None) return;

        if (_keyLookup.TryGetValue(info.KeyCode, out KeyView keyView))
        {
            // Trigger visual feedback (e.g., turn red if info.IsCorrect is false)
            FlashFeedback(keyView, info.IsCorrect);
        }
    }

    public void FlashFeedback(KeyView keyView, bool isCorrect)
    {
        // Flash red if incorrect, green if correct
        Color feedbackColor = isCorrect ? Color.green : Color.red;
        
        StartCoroutine(FlashRoutine(keyView.GetComponent<Image>(), feedbackColor));
    }

    private IEnumerator FlashRoutine(Image image, Color targetColor)
    {
        // Apply feedback color, wait a fraction of a second, then restore
        image.color = targetColor;
        yield return new WaitForSeconds(1f);
        image.color = Color.white; // Or revert to active KeyState
    }

    private void OnWordTargeted(Word word)
    {
        
    }

    private void OnWordUntargeted(Word word)
    {
        
    }

    private void OnWordCompleted(Word word)
    {
        
    }

    private void OnLetterTyped(Word word, char typedChar, bool isCorrect)
    {
        // Optionally, you can highlight the key corresponding to typedChar
        KeyCode code = WordManager.Instance.ConvertCharToKeyCode(typedChar);
        if (_keyLookup.TryGetValue(code, out KeyView keyView))
        {
            FlashFeedback(keyView, isCorrect);
        }
    }
}