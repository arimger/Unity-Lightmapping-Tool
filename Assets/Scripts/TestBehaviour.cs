using Toolbox.Lighting;
using UnityEngine;
using UnityEngine.UI;

public class TestBehaviour : MonoBehaviour
{
    public Slider blendSlider;
    public LightmappingManager manager;

    public void Awake()
    {
        blendSlider.minValue = 0.0f;
        blendSlider.maxValue = 1.0f;
        blendSlider.onValueChanged.AddListener((value) =>
        {
            manager.BlendValue = value;
        });
        blendSlider.value = 0.0f;
    }
}