using UnityEngine;
using UnityEngine.UI;
public class MerlinAnim : MonoBehaviour
{
    [SerializeField] private Sprite[] _merlinHandsSprites;
    [SerializeField] private Sprite[] _merlinManaSprites;
    [SerializeField] Image _merlinLeftHandImage;
    [SerializeField] Image _merlinRightHandImage;
    [SerializeField] Image _merlinManaImage;

    private void OnEnable()
    {
        WordManager.KeyPressedEvent += OnKeyPressed;
    }

    private void OnDisable()
    {
        WordManager.KeyPressedEvent -= OnKeyPressed;
    }

    private void OnKeyPressed(WordManager.KeyPressInfo info)
    {
        _merlinLeftHandImage.sprite = _merlinHandsSprites[Random.Range(0, _merlinHandsSprites.Length)];
        _merlinRightHandImage.sprite = _merlinHandsSprites[Random.Range(0, _merlinHandsSprites.Length)];
        _merlinManaImage.sprite = _merlinManaSprites[Random.Range(0, _merlinManaSprites.Length)];

        _merlinLeftHandImage.SetNativeSize();
        _merlinRightHandImage.SetNativeSize();
        _merlinManaImage.SetNativeSize();
    }
}
