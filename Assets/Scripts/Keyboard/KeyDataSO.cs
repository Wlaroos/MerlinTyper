using UnityEngine;

[CreateAssetMenu(fileName = "KeyData_", menuName = "Keyboard/Key Data")]
public class KeyDataSO : ScriptableObject
{
    [Header("Identity & Input")]
    public string keyId;            // e.g., "Key_A"
    public KeyCode keyCode;         // Physical key bind

    [Header("Visual States")]
    public Sprite defaultSprite;
    public Sprite hoveredSprite;
    public Sprite pressedSprite;
}