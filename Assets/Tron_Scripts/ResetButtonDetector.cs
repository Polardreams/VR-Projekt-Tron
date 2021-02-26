using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ResetButtonDetector : MonoBehaviour
{
    /// <summary>
    /// Steuert das Verhalten des Reset Buttons
    /// </summary>

    #region Settings
    /// <summary>
    /// Top Sprite des Reset Button's
    /// </summary>
    [SerializeField]
    private GameObject buttonTop = null;
    
    /// <summary>
    /// Ausgangsposisition Top Sprite
    /// </summary>
    private Vector3 localTopStartPosition;

    /// <summary>
    /// Bild Component
    /// </summary>
    private Image image;

    /// <summary>
    /// Button Farbe ungedrückt
    /// </summary>
    private Color colorUnpressed;

    /// <summary>
    /// Button Farbe gedrückt
    /// </summary>
    private Color colorPressed;
    #endregion

    [SerializeField]
    private UnityEvent resetButtonEvent = null;

    #region Unity Callbacks
    private void Start()
    {
        localTopStartPosition = buttonTop.transform.localPosition;
        colorPressed = buttonTop.transform.parent.GetComponent<Image>().color;
        image = buttonTop.transform.GetComponent<Image>();
        colorUnpressed = image.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("reverbController"))
        {
            buttonTop.transform.localPosition = Vector3.zero;
            image.color = colorPressed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("reverbController"))
        {
            buttonTop.transform.localPosition = localTopStartPosition;
            image.color = colorUnpressed;
            resetButtonEvent.Invoke();
        }
    }

    #endregion
}
