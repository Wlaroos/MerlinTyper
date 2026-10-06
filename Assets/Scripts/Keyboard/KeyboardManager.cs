using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    [SerializeField] private KeyboardLayoutSO _activeLayout;
    [SerializeField] private GameObject _keyPrefab;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private Transform _rowParent; // Container with Vertical Layout Group
    [SerializeField] private float _keyScale = 1f;

    private Dictionary<KeyCode, KeyView> _keyLookup = new Dictionary<KeyCode, KeyView>();

    private void Start()
    {
        BuildKeyboard();
    }

    private void BuildKeyboard()
    {
        foreach (var rowDef in _activeLayout.rows)
        {
            // Spawn a container for each row (with HorizontalLayoutGroup attached)
            GameObject rowGO = Instantiate(_rowPrefab, _rowParent);
            RectTransform rowTransform = rowGO.GetComponent<RectTransform>();

            rowTransform.sizeDelta = new Vector2(rowTransform.sizeDelta.x, 13 * _keyScale); // Set height based on prefab height

            foreach (var keyData in rowDef.keys)
            {
                KeyView view = Instantiate(_keyPrefab, rowGO.transform).GetComponent<KeyView>();
                view.Initialize(keyData);

                RectTransform viewTransform = view.GetComponent<RectTransform>();
                viewTransform.sizeDelta = new Vector2(keyData.hoveredSprite.rect.width * _keyScale, keyData.hoveredSprite.rect.height * _keyScale); // Set size based on sprite dimensions
                
                if (!_keyLookup.ContainsKey(keyData.keyCode))
                {
                    _keyLookup.Add(keyData.keyCode, view);
                }
            }
        }
    }

    private void Update()
    {
        // Example O(1) dictionary lookup for press state
        foreach (var key in _keyLookup)
        {
            if (Input.GetKeyDown(key.Key))
                key.Value.SetState(KeyView.KeyState.Pressed);
            else if (Input.GetKeyUp(key.Key))
                key.Value.SetState(KeyView.KeyState.Default);
        }
    }
}