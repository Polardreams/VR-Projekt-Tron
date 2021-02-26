using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


//https://stackoverflow.com/questions/36244660/simple-event-system-in-unity
[System.Serializable]
 public class _UnityEventFloat : UnityEvent<float> { }
[System.Serializable]
public class _UnityEventVector2 : UnityEvent<Vector2> { }
[System.Serializable]
public class _UnityEventVector3 : UnityEvent<Vector3> { }

/// <summary>
/// Skript aus dem Seminar Mixed Reality (Simon Adler) zum Auslesen der wichtigsten Inputwerte 
/// der VR - Trigger (z.b.:HP Reverb)
/// </summary>
/// 
public class ReadController : MonoBehaviour
{
    public bool isLeft = false;

    public _UnityEventFloat onSelect;
    public _UnityEventVector2 onsndAxis;
    public _UnityEventVector3 onleftPos;
    public _UnityEventVector3 onrightPos;

    //speziell für die Gamepads
    public _UnityEventFloat onGamePadTR;
    public _UnityEventFloat onGamePadTL;

    [ReadOnly] private float select;

    [ReadOnly] public bool menuButton;
    [ReadOnly] public bool gripButton;

    [ReadOnly] public Vector2 sndAxis = new Vector2();
    [ReadOnly] public bool sndAxisClick;

    [ReadOnly] public Vector2 priAxis = new Vector2();
    [ReadOnly] public bool priAxisTouch;
    [ReadOnly] public bool priAxisClick;

    private ArrayList controlls = new ArrayList() { "Spatial Controller - Left", "Spatial Controller - Right"};//Input.GetJoystickNames() zur Unterscheidung von InputControls

    public bool isXBox_controller = false;
    public bool isPS4_controller = false;

    void Start()
    {
        /*
        * Inputwerte als Unity Events in einzelne Skripte Referenzieren
        * siehe Inspector LeftController und RightController
        */
        
        //VR Trigger
        onSelect.AddListener(delegate { setTrigger(0f); });
        onsndAxis.AddListener(delegate { setJoyStick(Vector2.zero); });
        
        onleftPos.AddListener(delegate { setJoyStick(Vector3.zero); });
        onrightPos.AddListener(delegate { setJoyStick(Vector3.zero); });//Warum haben wir hier JoyStick verwendet ... zum schluss ist es egal, da wir den Param als Vector3 definiert haben!

        //Gamepad
        onGamePadTR.AddListener(delegate { setTR(0f); });
        onGamePadTL.AddListener(delegate { setTL(0f); });
    }

    public void readController()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();

        InputDeviceCharacteristics character = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        if (!isLeft) character = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

        InputDevices.GetDevicesWithCharacteristics(character, inputDevices);

        if (inputDevices.Count == 0) return;

        InputDevice dev = inputDevices[0];

        dev.TryGetFeatureValue(CommonUsages.trigger, out select);//gas und bremsen und rückwärts fahren
        dev.TryGetFeatureValue(CommonUsages.secondary2DAxis, out sndAxis);//steering
        dev.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out sndAxisClick);
        dev.TryGetFeatureValue(CommonUsages.menuButton, out menuButton);//kleiner rechteckiger Button
        dev.TryGetFeatureValue(CommonUsages.gripButton, out gripButton);//seitlicher Button 
        dev.TryGetFeatureValue(CommonUsages.primary2DAxis, out priAxis);//rundes Touchpad
        dev.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out priAxisClick);
        dev.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out priAxisTouch);

        onsndAxis.Invoke(sndAxis);// ev. doppelter Steering Input. Hat eventuell Überempfindlichkeit verursacht. (ggf. entfernen)
        onSelect.Invoke(select);

        if (isLeft)
        {
            onleftPos.Invoke(transform.position);
        }
        else
        {
            onrightPos.Invoke(transform.position);
        }
            
    }
    /// <summary>
    /// Input GamePad und Keyboard
    /// </summary>
    public void readGamepad ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            reloadScene();
        }

        /*
        * Unterscheidung Xbox Controller und PS4 Controller
        */
        if (isXBox_controller && isLeft)
        {
            select = Input.GetAxis("xbox_vertical");//Input Manager -> Vertical Axis -> Sensitivity = 1, Trigger durchgedrückt = 1 
            if (select > 0)
            {
                onGamePadTR.Invoke(select);
            } else
            {
                onGamePadTL.Invoke(Mathf.Abs(select));
            }
            sndAxis = new Vector2(Input.GetAxis("horizontal"), 0);
            onsndAxis.Invoke(sndAxis);
        } else
        {
            
            if (isPS4_controller && isLeft)
            {
                float select_gas = Mathf.Lerp(0, 1, Input.GetAxis("ps4_vertical_gas"));//unpressed -1; range is: (-1) - (0) - (+1); 0.01 => PS4
                float select_break = Mathf.Lerp(0, 1, Input.GetAxis("ps4_vertical_break"));//unpressed -1; range is: (-1) - (0) - (+1); 0.01 => PS4
                if (select_gas > 0)
                {
                    onGamePadTR.Invoke(select_gas);
                    //Debug.Log("TR: " + select_gas);
                }
                else
                {
                    if (select_break > 0)
                    {
                        onGamePadTL.Invoke(Mathf.Abs(select_break));
                        //Debug.Log("TL: " + select_break);
                    }
                }

                if(Input.GetAxis("horizontal") > 0.1f || Input.GetAxis("horizontal") < -0.1f)
                {
                    sndAxis = new Vector2(Input.GetAxis("horizontal"), 0);
                    onsndAxis.Invoke(sndAxis);
                    //Debug.Log("Steer(snAxis): " + sndAxis);
                }
                else
                {
                    sndAxis = Vector2.zero;
                    onsndAxis.Invoke(sndAxis);
                }
            }
            
        }
    }

    void Update()
    {
        readController();
        if(isLeft)readGamepad(); //ausführung nur auf einem HP Reverb Trigger
    }

    /// <summary>
    /// Methoden Definitionen für Unity Events
    /// </summary>
    private void setTrigger (float axis)
    {

    }

    private void setJoyStick(Vector2 stick)
    {

    }

    private void setTR(float axis)
    {

    }

    private void setTL(float axis)
    {

    }

    private void setLeftPos (Vector3 pos)
    {

    }

    private void setRightPos(Vector3 pos)
    {

    }


    /// <summary>
    /// Reload Scene
    /// </summary>
    public void reloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("reload");
    }
}
