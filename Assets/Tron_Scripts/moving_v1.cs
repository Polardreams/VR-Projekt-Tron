using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class moving_v1 : MonoBehaviour
{
    /// <summary>
    /// moving_v1 Class
    /// diese Klasse muss als Komponente einem root-Object zugefügt werden
    /// dieses Root-Objekt bewegt sich wie ein Motorrad
    /// 
    /// Das Root-Objekt kann sich in alle Achsen hin bewegen(x, y, z)
    /// Es kann sich nur der z Achse entsprechen Rückwärts bewegen(kein Lenken möglich)
    ///     Das Root-Objekt kann entsprechend der y-Achse auch Berg auf fahren und bewegt sich bei Verlust des
    ///     Bodenkontaktes nach unten(downwards)
    ///         Die Funktion dazu wurde in die FixedUpdate-Methode implementiert, da durch die Schwerelosigkeit
    ///         das Root-Objekt wegschwebt.
    /// Die Beschleunigung verhält sich entsprechend der accelerating_curve (AnimationCurve)
    /// Das Bremsereignis verhältsich entsprechend der break_curve (AnimationCurve)
    /// 
    /// Die Lenkung (steer) und die Neigung in entsprechenden Kurven (Tilt)
    /// sind von der Geschwindigkeit (velocity) des Root-Objektes abhängig.
    ///     Je schneller desto mehr Neigung und weniger Lenkung
    ///     Je langsamer desto weniger Neigung und mehr Lenkung    
    /// 
    /// Auf das Root-Objekt wirkt eine natürliche Reibung (friction), durch die Methode motorbikeDrag()
    /// </summary>
    
    #region settings inspector
    //Interface 
    public float max_velocity = 120;
    public float max_break = 2f;
    public float max_acceleration = 2f;
    public float acceleration = 0;
    public float backward_velocity = 1.5f;
    public float max_tilt = 65f;
    public float max_steeringAngle = 30;
    public float max_steerIncrease = 3f;
    public float rate_of_fall = 13f;
    public float friction = 0.1f;
    /// <summary>
    /// bescheibt in Prozent den Verlauf der Beschleunigung
    /// </summary>
    public AnimationCurve accelerating_curve;
    /// <summary>
    /// bescheibt in Prozent den Verlauf der Empfindlichkeit der Beschleunigung ausgehend vom Input
    /// </summary>
    public AnimationCurve accelerating_behavior_curve;
    /// <summary>
    /// beschreibt in Prozent den Verlauf der Bremsung
    /// </summary>
    public AnimationCurve break_curve;
    /// <summary>
    /// Geschwindigkeit des Motorrads
    /// </summary>
    public float velocity = 0;
    /// <summary>
    /// Neigung z - Achse 
    /// </summary>
    public float tilt = 0;
    /// <summary>
    /// Fläche auf die der Richtungsvektor der Left-u.RightTrigger projiziert wird, 
    /// um bei Neigungen ein fehlerhaftes Lenkvermögen zu verhindern
    /// </summary>
    public Transform lenkstange;
    /// <summary>
    /// Fallgeschwindigkeit
    /// </summary>
    public bool falldown;

    /// <summary>
    /// RestButton Switch Modus
    /// </summary>
    public GameObject HUD;

    /// <summary>
    ///  switch sound "ja" "nein" von Tron
    /// </summary>
    public BitController bitController;

    /// <summary>
    /// Multiplier for the angle used to turn the bike. Used to reduce neccessary hand movement.
    /// </summary>
    [HideInInspector]
    public float AngleMultiplier { get; set; }
    #endregion
    #region private variables
    /// <summary>
    /// temporäre Variable zur Richtungsänderung
    /// </summary>
    private float y;
    /// <summary>
    /// horozontale Achse berechnet aus calSteeringValue
    /// </summary>
    private float horizont;
    /// <summary>
    /// Position leftTrigger
    /// </summary>
    private Vector3 lefthand = Vector3.zero;
    /// <summary>
    /// Position rightTrigger
    /// </summary>
    private Vector3 righthand = Vector3.zero;
    /// <summary>
    /// absoluter Inkrementierungswert für y Variable
    /// </summary>
    private float steeringIncrease = 3f;
    /// <summary>
    /// Input Achsen Beschleunigung und Bremsung
    /// </summary>
    private float accForwardControllValue, accBackwardControllValue;
    /// <summary>
    /// bremsen aktivieren
    /// </summary>
    private bool flag_break = false;
    /// <summary>
    /// Motorbike an den Boden ketten
    /// </summary>
    private bool isConstraint = false;
    /// <summary>
    /// Steuerung der Vignette
    /// </summary>
    [SerializeField]
    private VolumeProfile postProcessVolume = null;
    private Vignette vignette;

    /// <summary>
    /// Variable für ResetDashboard, schaltet ResetButton ein und aus
    /// </summary>
    private bool flagReset = false;

    /// <summary>
    /// Position bei der Kollision
    /// </summary>
    private Vector3 colPos;

    /// <summary>
    /// das Hinderniss bei der Kollision
    /// </summary>
    private Transform obstacle;

    /// <summary>
    /// ist das Bike gestartet
    /// </summary>
    private bool isBikeBooted = false;

    #endregion

    #region Methods
    private void FixedUpdate()
    {
        if (isBikeBooted)
        {
            if (falldown)
            {
                move_downward();
            }
            else
            {

                /*
                * Nur wenn das Motorbike auf dem Boden ist, kann gebremst und gelenkt werden 
                * Nur wenn das Motorbike in Bewegung ist, kann gelenkt werden
                */
                if (Mathf.Abs(velocity) > 0.1f)
                {
                    if (lefthand != Vector3.zero && righthand != Vector3.zero)
                        horizont = calSteeringValue(lefthand, righthand);

                    if (Mathf.Abs(horizont) > 0.1f)
                        y += horizont * steeringIncrease * AngleMultiplier;

                    /* 
                    * Durch Subtraktion der Geschwindigkeit wird gebremst. Bei <=0 wird velocity auf 0 gesetzt (reset)
                    * Er fährt also nicht Rückwärts
                    */
                    if (flag_break)
                    {
                        if (velocity > 0)
                        {
                            velocity -= getBreak();
                        }
                        else
                        {
                            resetVelocity();
                        }
                    }
                }
            }

            //Vorwärtsbeschleunigung
            if (accForwardControllValue > 0.01)
            {
                Rigidbody r = gameObject.GetComponent<Rigidbody>();
                if (r.velocity.magnitude < 3)//Friction bei velocity = 0 herausrechnen
                {
                    resetVelocity();
                }

                if (velocity <= max_velocity)
                {
                    /*
                    * accForwardControllValue durch accelerating_behavior_curve (AnimCurve) erst bei 50% Trigger steigernd, 
                    * vorher kann man seine Geschwindigkeit halten
                    * Friction mit Addition herausrechnen, damit man die Geschwindigkeit vor den 50 % halten kann
                    */
                    if (accForwardControllValue > 0)
                    {
                        velocity += (getAcceleration() * accForwardControllValue) + friction;//auflösung motorbikeDrag() um Geschwindigkeit zu halten
                    }
                    else
                    {
                        velocity += (getAcceleration() * accForwardControllValue);
                    }
                }
            }


            //Bremsvorgang

            if (accBackwardControllValue > 0)
            {
                if (velocity <= 0)
                {
                    move_backward();
                }
                else
                {
                    flag_break = true;
                }
            }
            else
            {
                flag_break = false;
            }

            move_forward();
            motorbikeDrag();

            if (flagReset)
            {
                resetResetBoard();
            }

            vignette.intensity.value = (velocity / max_velocity) / 1.2f;
        }
    }

    /// <summary>
    /// kalkuliere Bremswert aus AnimCurve
    /// </summary>
    private float getBreak()
    {
        float vp = velocity * 100 / max_velocity;
        return max_break * break_curve.Evaluate(vp / 100);
    }

    /// <summary>
    /// kalkuliere Beschleunigungswert aus AnimCurve
    /// </summary>
    private float getAcceleration()
    {
        float vp = velocity * 100 / max_velocity;
        return (max_acceleration * accelerating_curve.Evaluate(vp / 100));
    }

    /// <summary>
    /// Alle Energien (Geschwindigkeiten) zurücksetzen
    /// </summary>
    private void resetVelocity()
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    /// <summary>
    /// Reibung des Mototbikes 
    /// (stellt eine abstrakte Reibung dar, um das Fahrgefühl zu verbessern)
    /// diese Reibung hat nur bedingt einen physikalischen Anspruch
    /// </summary>
    private void motorbikeDrag()
    {
        if (velocity > 0)
        {
            velocity -= friction;//es gibt über den Rigidbody auch einen Angluar Drag 
        }
        else
        {
            velocity = 0;
        }
    }

    /// <summary>
    /// Neigung des Motorrades 
    /// Z-Achse
    /// </summary>
    private float tilt_motorbike(float axis)
    {
        float vp = velocity * 100 / max_velocity;
        steeringIncrease = Mathf.Clamp(max_steerIncrease - (max_steerIncrease / 100 * vp), 1f, max_steerIncrease);
        float sp = steeringIncrease * 100 / max_steerIncrease;
        float tmp_tilt = Mathf.Clamp(max_tilt * ((100 - sp) / 100), 0, max_tilt);
        float angle = axis * tmp_tilt;
        tilt = angle;
        return angle;
    }

    /// <summary>
    /// Simulation der Vorwärtsbewegung
    /// </summary>
    private void move_forward()
    {
        transform.Translate(Vector3.forward * Time.fixedDeltaTime * velocity);//deltaTime verstrichene Zeit zwischen Frames. GameObject wird um verstrichene Zeit in Position versetzt. ca. 1UE/Sek (ohne Speed)
        transform.rotation = Quaternion.Euler(new Vector3(0f, y, -tilt_motorbike(horizont)));
    }
    /// <summary>
    /// Simulation des Falls
    /// </summary>
    private void move_downward()
    {
        transform.Translate(Vector3.down * Time.fixedDeltaTime * rate_of_fall);
    }

    /// <summary>
    /// Simulation der Rückwärtsbewegung
    /// </summary>
    private void move_backward()
    {
        transform.Translate(Vector3.back * Time.fixedDeltaTime * backward_velocity);
    }

    private void Start()
    {
        y = transform.rotation.eulerAngles.y;
        falldown = false;
        postProcessVolume.TryGet(out vignette);
        AngleMultiplier = 1f;
    }
    #endregion
    #region Callbacks
    /// <summary>
    /// Motorrad Collider kollidiert nicht mehr mit Ground-Collider
    /// Einsetzen des Falls
    /// </summary>
    private void OnTriggerExit(UnityEngine.Collider other)
    {       
        if (other.transform.tag.Equals("ground") && transform.position.y > 1f)
        {
            falldown = true;//Schwerkraft an
        }
    }

    /// <summary>
    /// Motorrad Collider kolldiert mit dem Ground
    /// Schwerkraft ausschalten
    /// </summary>
    private void OnTriggerEnter (UnityEngine.Collider other)
    {
        if (other.transform.tag.Equals("ground"))
        {
            isConstraint = true;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;//*unterbinden des Rückstoßes vom Boden durch das Aufkommen auf den Boden (weil Mototbike im Prinzip ohne Gravity)
            falldown = false;//Schwerkraft aus
        } else 
        {   
            if (other.transform.tag.Equals("obstacle"))
            {
                //ResetDashBoard und Geschwindigkeit und Beschleunigung auf 0 setzen
                if (!flagReset)
                    flagReset = true;
                velocity = 0;
                accForwardControllValue = 0;
                obstacle = other.transform;
                obstacle.GetComponent<MeshRenderer>().enabled = false;
                HUD.GetComponent<HUDController>().switchToResetboard(flagReset);
                colPos = transform.position;
                bitController.TriggerState(false);
            }
        }
    }
    /// <summary>
    /// Schaltet ResetDashboard aus, sobald 0.5f UEinheiten weiter gefahren
    /// </summary>
    private void resetResetBoard ()
    {
        if (Vector3.Distance(colPos, transform.position) > 0.5f)
        {
            obstacle.GetComponent<MeshRenderer>().enabled = true;
            flagReset = false;
            HUD.GetComponent<HUDController>().switchToResetboard(flagReset);
        }
    }

    public void setBikeBoot (bool isboot)
    {
        isBikeBooted = isboot;
    }

    /// <summary>
    /// Motorbike befindet sich auf dem Boden (ständige Kollision Motorrad Collider und Ground Collider
    /// </summary>
    private void OnTriggerStay(UnityEngine.Collider other)
    {
        
        if (isConstraint && other.transform.tag.Equals("ground"))
        {
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;//*Abschalten des Unterbindens des Rückstoßes
            isConstraint = false;
        }
    }

    /// <summary>
    /// Inputwert Lenkung (GamePad)
    /// </summary>
    public void OnSteer(Vector2 context) 
    {
        horizont = context.x;
    }

    /// <summary>
    /// Inputwert Beschleunigung
    /// </summary>
    public void OnACC(float context)
    {
       accForwardControllValue = accelerating_behavior_curve.Evaluate(context);  
    }

    /// <summary>
    /// Inputwert Bremsung
    /// </summary>
    public void OnACCBack(float context)
    {
        accBackwardControllValue = context;
    }

    /// <summary>
    /// Inputwert Lenkung HP Reverb (Position linke Hand)
    /// </summary>
    public void OnLeftController (Vector3 leftpos)
    {
        lefthand = leftpos;
    }

    /// <summary>
    /// Inputwert Lenkung HP Reverb (Position rechte Hand)
    /// </summary>
    public void OnRightController(Vector3 rightpos)
    {
        righthand = rightpos;
    }

    /// <summary>
    /// Berechnung des Inkrementierungswertes zur Lenkung, um y zu berechnen, 
    /// wenn der Input aus der HP Reverb kommt
    /// </summary>
    private float calSteeringValue (Vector3 leftHand, Vector3 rightHand)
    {
        #region Sebastians Refactoring
        // Project bikes forward vector on handleBar rotationary plane
        Vector3 fwdDir = transform.forward;
        fwdDir = Vector3.ProjectOnPlane(fwdDir, lenkstange.transform.right);

        Debug.DrawRay(lenkstange.position, fwdDir.normalized, Color.blue);

        // translate into local space of handlebar
        fwdDir = lenkstange.transform.InverseTransformDirection(fwdDir);
        fwdDir.Normalize();

        // Project vector from leftHand to rightHand onto rotationary plane
        Vector3 rightToLeftDir = leftHand - rightHand;
        rightToLeftDir = Vector3.ProjectOnPlane(rightToLeftDir, lenkstange.transform.right);

        Debug.DrawRay(lenkstange.position, rightToLeftDir.normalized, Color.yellow);

        // translate direction into local space of handlebar
        rightToLeftDir = lenkstange.transform.InverseTransformDirection(rightToLeftDir);
        rightToLeftDir.Normalize();

        // Calculate dot value of normalized vectors
        float dotValue = Vector3.Dot(fwdDir, rightToLeftDir);
        //Debug.Log("Skalar: " + dotValue + "\nAngle: " + Mathf.Sin(dotValue)); // Steering to the right returns a positive dot product value, steering to the left returns a negativ dot product value

        /*
         *  Discussion about TransformPoint, TransformVector and TransformDirection:
         *  https://answers.unity.com/questions/1331232/what-is-the-difference-between-transformdirection.html
         *  
         *  Dot Product values range from 1 for parallel vectors pointing in the same direction (0°), over 0 for orthogonal vectors (90°), to -1 for parallel vectors pointing to opposite directions (180°)
         *  https://docs.unity3d.com/2019.3/Documentation/uploads/Main/CosineValues.png
         */
        #endregion

        return  dotValue;
    }
    #endregion
}
