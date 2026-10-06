using UnityEngine;
using UnityEngine.UI;

public class KeyView : MonoBehaviour
{
    private Image _keyImage;
    private KeyDataSO _keyData;

    public void Initialize(KeyDataSO keyData)
    {
        _keyImage = GetComponent<Image>();
        _keyData = keyData;
        SetState(KeyState.Default);
        
        // Adjust RectTransform width based on key multiplier
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
    }

    public enum KeyState { Default, Hovered, Pressed }

    public void SetState(KeyState state)
    {
        switch (state)
        {
            case KeyState.Default:
                _keyImage.sprite = _keyData.defaultSprite;
                break;
            case KeyState.Hovered:
                _keyImage.sprite = _keyData.hoveredSprite;
                break;
            case KeyState.Pressed:
                _keyImage.sprite = _keyData.pressedSprite;
                break;
        }
    }
}