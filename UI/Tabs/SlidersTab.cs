using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    public class SlidersTab
    {
        private const float THROTTLE_SECONDS = 0.05f;

        private float _linearValue = 0.5f;
        private int _integerValue = 5;
        private float _exponentialValue = 1.0f;
        private float _displayMappedValue = 50f;
        private float _throttledValue = 0.5f;
        private float _lastCommittedThrottledValue = 0.5f;
        private float _lastPushTime = -999f;
        private bool _pushPending = false;

        public void UpdatePending()
        {
            if (_pushPending && Time.time - _lastPushTime >= THROTTLE_SECONDS)
            {
                _lastCommittedThrottledValue = _throttledValue;
                _lastPushTime = Time.time;
                _pushPending = false;
            }
        }

        public void Draw()
        {
            // Linear Slider
            GUILayout.Label(string.Format(IMGUIHelperUIStrings.Sliders.Linear, _linearValue));
            float newLinear = GUILayout.HorizontalSlider(_linearValue, 0f, 1f);
            if (!Mathf.Approximately(newLinear, _linearValue))
                _linearValue = newLinear;

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Integer Slider
            GUILayout.Label(string.Format(IMGUIHelperUIStrings.Sliders.Integer, _integerValue));
            float f = _integerValue;
            float newF = GUILayout.HorizontalSlider(f, 0f, 10f);
            int newInt = Mathf.RoundToInt(newF);
            if (newInt != _integerValue)
                _integerValue = newInt;

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Exponential Slider
            GUILayout.Label(string.Format(IMGUIHelperUIStrings.Sliders.Exponential, _exponentialValue));
            float expMin = 0.1f, expMax = 10f, exponent = 2f;
            float norm = Mathf.InverseLerp(expMin, expMax, _exponentialValue);
            float sliderT = Mathf.Pow(norm, 1.0f / exponent);
            float newSliderT = GUILayout.HorizontalSlider(sliderT, 0f, 1f);
            float newNorm = Mathf.Pow(newSliderT, exponent);
            float newExp = Mathf.Lerp(expMin, expMax, newNorm);
            if (!Mathf.Approximately(newExp, _exponentialValue))
                _exponentialValue = newExp;

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Display-Mapped Slider
            GUILayout.Label(string.Format(IMGUIHelperUIStrings.Sliders.DisplayMapped, _displayMappedValue));
            float newDisplay = GUILayout.HorizontalSlider(_displayMappedValue, 0f, 100f);
            _displayMappedValue = newDisplay;

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Throttled Slider
            GUILayout.Label(string.Format(IMGUIHelperUIStrings.Sliders.Throttled, _throttledValue, _lastCommittedThrottledValue));
            float newThrottled = GUILayout.HorizontalSlider(_throttledValue, 0f, 1f);
            if (!Mathf.Approximately(newThrottled, _throttledValue))
            {
                _throttledValue = newThrottled;
                _pushPending = true;
                if (Time.time - _lastPushTime >= THROTTLE_SECONDS)
                {
                    _lastCommittedThrottledValue = _throttledValue;
                    _lastPushTime = Time.time;
                    _pushPending = false;
                }
            }
        }
    }
}
