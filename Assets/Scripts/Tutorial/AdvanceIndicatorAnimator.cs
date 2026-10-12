using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class AdvanceIndicatorAnimator : MonoBehaviour
{
    [SerializeField] private Image _imageComponent;
    [SerializeField] private Sprite[] _sprites = new Sprite[2]; // Two frames
    [SerializeField] private float _frameRate = 0.3f;

    [SerializeField] private ParticleSystem _ps01;
    [SerializeField] private ParticleSystem _ps02;
    private Coroutine _animCoroutine;

    private void OnEnable()
    {
        if (_sprites.Length >= 2)
        {
            _animCoroutine = StartCoroutine(AnimateRoutine());
        }
    }

    private void OnDisable()
    {
        ParticlePlay();
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
    }

    private IEnumerator AnimateRoutine()
    {
        int frame = 0;
        while (true)
        {
            _imageComponent.sprite = _sprites[frame];
            frame = (frame + 1) % 2;
            yield return new WaitForSeconds(_frameRate);
        }
    }

    private void ParticlePlay()
    {
        _ps01.Play();
        _ps02.Play();   
    }
}