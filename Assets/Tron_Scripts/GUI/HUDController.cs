using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HUDController : MonoBehaviour
{
    /// <summary>
    ///  Steuert das onBoard HUD des Bikes 
    /// </summary>

    #region Settings
    /// <summary>
    /// script zur Steuerung der Bike Bewegungen
    /// </summary>
    [SerializeField]
    private moving_v1 bike_moving_script = null;

    /// <summary>
    /// Sprite zur Anzeige der Geschwindigkeit
    /// </summary>
    [SerializeField]
    private Image tachometer = null;

    /// <summary>
    /// Sprite zur Darstellung der Neigung
    /// </summary>
    [SerializeField]
    private RectTransform image_tilt_Object = null;

    /// <summary>
    /// numerische Angabe der Geschwindigkeit
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI speed_amount = null;

    /// <summary>
    /// numerische Angabe der Neigung
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI tilt_amount = null;

    /// <summary>
    /// canvasObject Dashboard
    /// </summary>
    [SerializeField]
    private GameObject dashboard = null;

    /// <summary>
    /// canvasObject Reset Button
    /// </summary>
    [SerializeField]
    private GameObject resetboard = null;

    /// <summary>
    /// maximale Geschwindigkeit des Bikes
    /// </summary>
    private float maxSpeed;

    /// <summary>
    /// Liste der Geschwindigkeits Inkremente des HUD's
    /// </summary>
    [SerializeField]
    TextMeshProUGUI[] speedIncrements = new TextMeshProUGUI[11];
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        maxSpeed = bike_moving_script.max_velocity;
        resetboard.SetActive(false);
        dashboard.SetActive(true);
        setIncrementText();
    }

    void Update()
    {
        speed_amount.text = Mathf.RoundToInt(bike_moving_script.velocity).ToString();
        tachometer.fillAmount = (bike_moving_script.velocity / maxSpeed);
        tilt_amount.text = Mathf.RoundToInt(bike_moving_script.tilt).ToString();
        image_tilt_Object.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -bike_moving_script.tilt * -1f));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Bei einer Kollision mit einem Hindernis, soll der Reset Button sichtbar werden
    /// </summary>
    public void switchToResetboard(bool resetFlag)
    {
        resetboard.SetActive(resetFlag);
        dashboard.SetActive(!resetFlag);
    }

    /// <summary>
    /// Die HUD Elemente zur Darstellung der Geschwindigkeitsinkremente werden berechnet
    /// </summary>
    private void setIncrementText()
    {
        for (int i = 0; i < speedIncrements.Length; i++)
        {
            float speedIncrement = (maxSpeed / (speedIncrements.Length + 1) * (i + 1));
            speedIncrements[i].text = Mathf.RoundToInt(speedIncrement).ToString();
        }
    }
    #endregion
}
