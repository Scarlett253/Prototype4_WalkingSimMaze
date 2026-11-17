using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Volume globalVolume;
    [SerializeField] Light sun;

    [Header("Ambience")]
    [SerializeField] float maxExposure = 0f;
    [SerializeField] float minExposure = -3f;
    [SerializeField] float maxSunIntensity = 1f;
    [SerializeField] float minSunIntensity = 0.2f;

    [Header("Scale")]
    [SerializeField] int snowmenForFullDark = 10;

    ColorAdjustments _colorAdjustments;

    void Start()
    {
        if (globalVolume == null) return;

        if (globalVolume.profile.TryGet(out _colorAdjustments))
            _colorAdjustments.active = true;
    }

    void Update()
    {
        if (SnowmanDirector.Instance == null) return;

        float t = Mathf.Clamp01((float)SnowmanDirector.Instance.ActiveCount / snowmenForFullDark);
        ApplyExposure(t);
        ApplySun(t);
    }

    void ApplyExposure(float t)
    {
        if (_colorAdjustments == null) return;
        _colorAdjustments.postExposure.value = Mathf.Lerp(maxExposure, minExposure, t);
    }

    void ApplySun(float t)
    {
        if (sun == null) return;
        sun.intensity = Mathf.Lerp(maxSunIntensity, minSunIntensity, t);
    }
}
