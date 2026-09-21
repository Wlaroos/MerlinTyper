using UnityEngine;
using UnityEngine.UI;

public class ManaSlider : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void AddMana(float amount)
    {
        if (_slider != null)
        {
            _slider.value += amount;
        }
    }

    public void ReduceMana(float amount)
    {
        if (_slider != null)
        {
            _slider.value -= amount;
        }
    }
}
