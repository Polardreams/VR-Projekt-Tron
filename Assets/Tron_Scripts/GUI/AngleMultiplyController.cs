using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Manages slider and input field for setting the angle multiplier value.
/// </summary>
public class AngleMultiplyController : MonoBehaviour
{
    #region Settings

    [System.Serializable]
    public class AngleUpdateEvent : UnityEvent<float> { }

    /// <summary>
    /// UI slider to set value
    /// </summary>
    [SerializeField] private Slider angleSlider = null;

    /// <summary>
    /// Input field to set value
    /// </summary>
    [SerializeField] private TMP_InputField angleInput = null;

    /// <summary>
    /// The current angle multiplier.
    /// </summary>
    private float currentAngleMultiplier = 1f;

    /// <summary>
    /// Public current angle multiplier.
    /// </summary>
    public float CurrentAngleMultiplier
    {
        get { return currentAngleMultiplier; }
        set
        {
            if (currentAngleMultiplier == value)
                return;

            currentAngleMultiplier = Mathf.Clamp(value, 0.01f, angleSlider.maxValue);
            OnAngleMultiplierChange?.Invoke(value);
        }
    }

    /// <summary>
    /// Event is called, when value of angle multiplier changes.
    /// </summary>
    public AngleUpdateEvent OnAngleMultiplierChange = null;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        angleSlider.SetValueWithoutNotify(CurrentAngleMultiplier);
        angleInput.text = CurrentAngleMultiplier.ToString("F2");

        angleSlider.onValueChanged.AddListener(SliderInput);
        angleInput.onValueChanged.AddListener(TextInput);
        angleInput.onDeselect.AddListener(delegate { UpdateTextField(); }); // Update text field on deselection to make sure, value does not exceed slider.maxValue.
    }

    #endregion

    #region Methods
    /// <summary>
    /// Gets input from the text field and updates slider.
    /// </summary>
    /// <param name="arg0">Input from the text field.</param>
    private void TextInput(string arg0)
    {
        CurrentAngleMultiplier = float.Parse(arg0);
        angleSlider.SetValueWithoutNotify(CurrentAngleMultiplier);
    }

    /// <summary>
    /// Gets input from the slider and updates text field.
    /// </summary>
    /// <param name="newAngle">slider input</param>
    private void SliderInput(float newAngle)
    {
        CurrentAngleMultiplier = newAngle;
        angleInput.text = CurrentAngleMultiplier.ToString("F2");
    }

    /// <summary>
    /// Updates text field separately
    /// </summary>
    private void UpdateTextField()
    {
        angleInput.text = CurrentAngleMultiplier.ToString("F2");
    }
    #endregion
}
