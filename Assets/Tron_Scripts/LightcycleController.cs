using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the phasing and fading of the lightCycle. Two different lightCycle gameObjects are needed,
/// since the backface method from the spherePhase shader didn't work well with extra glass transparency.
/// Glass transparency had to be put in another shader.
/// Contains phasing out logic for debug puposes.
/// </summary>
public class LightcycleController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// GameObject of the lightCycle with the sphere phasing material. To be used during start up.
    /// </summary>
    [Header("StartUp References")]
    [SerializeField] private GameObject lightCycleSpherePhaseGO = null;

    /// <summary>
    /// AnimationCurve to control the expansion of the sphere over time.
    /// </summary>
    [SerializeField] private AnimationCurve sphereExpansionCurve = new AnimationCurve();

    /// <summary>
    /// The time, how long the expansion shall take.
    /// </summary>
    [SerializeField] private float sphereExpansionTime = 1f;

    /// <summary>
    /// The sphere phasing material.
    /// </summary>
    [SerializeField] private Material spherePhaseMat = null;

    /// <summary>
    /// The lightCycle gameObject to be used during the actual gameplay.
    /// </summary>
    [Header("Runtime References")]
    [SerializeField] private GameObject lightCycleRuntimeGO = null;

    /// <summary>
    /// Animationcurve to control the fade in of the bikes windows transparency.
    /// </summary>
    [SerializeField] private AnimationCurve glassFadeCurve = new AnimationCurve();

    /// <summary>
    /// The time, how long the transparency fade in shall take.
    /// </summary>
    [SerializeField] private float glassFadeTime = 1f;

    /// <summary>
    /// The glass material.
    /// </summary>
    [SerializeField] private Material glassMat = null;

    /// <summary>
    /// Transform of the handlebar gameObject.
    /// </summary>
    [Space]
    [SerializeField] private Transform handleBar = null;

    /// <summary>
    /// Shall sphere phasing happen during Start callback? For debug purpose only.
    /// </summary>
    [SerializeField] private bool debugPhaseOnStart = false;

    /// <summary>
    /// Distance to the bike, when lines are supposed to start glowing.
    /// </summary>
    [Header("Bike vicinity materials")]
    [Range(0f, 50f)]
    [SerializeField] private float vicinityDistance = 10f;

    /// <summary>
    /// Materials which change upon their vicinity to the lightcycle.
    /// </summary>
    [SerializeField] private Material[] vicinityMats = new Material[4];

    /// <summary>
    /// Event called when phasing and fading finished and the lightCycle is ready to roll.
    /// </summary>
    [Space]
    public UnityEvent OnPhaseInComplete = null;

    /// <summary>
    /// Is the lightCycle phased in (visible)?
    /// </summary>
    private bool isPhasedIn = false;

    /// <summary>
    /// Holder for the phasing routine for easy access.
    /// </summary>
    private Coroutine phasingRoutine = null;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        // Set phasing material to default state.
        spherePhaseMat.SetFloat("_SPHERERADIUS", 0f);
        spherePhaseMat.SetVector("_SPHEREORIGIN", new Vector4(handleBar.position.x, handleBar.position.y, handleBar.position.z));

        if (debugPhaseOnStart)
            PhasingExpandSphere(true);

        for (int i = 0; i < vicinityMats.Length; i++)
            vicinityMats[i].SetFloat("_DISTANCETHRESHOLD", vicinityDistance);

    }

    private void Update()
    {
        if (isPhasedIn)
        {
            for (int i = 0; i < vicinityMats.Length; i++)
                vicinityMats[i].SetVector("_BIKEPOS", transform.position);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initiates phasing of lightCycle depending on current state.
    /// </summary>
    /// <param name="phaseIn">Shall lightCycle phase in?</param>
    public void PhasingExpandSphere(bool phaseIn)
    {
        if (isPhasedIn == phaseIn)
            return;

        if (phasingRoutine != null)
            return;

        phasingRoutine = StartCoroutine(PhasingRoutine(phaseIn));
    }

    /// <summary>
    /// Routine to manage the phase in procedure.
    /// </summary>
    /// <param name="phaseIn"></param>
    /// <returns></returns>
    IEnumerator PhasingRoutine(bool phaseIn)
    {
        float t = phaseIn ? 0f : 1f;

        if (phaseIn)
        {
            while (t < 1f)
            {
                t += Time.deltaTime / sphereExpansionTime;
                spherePhaseMat.SetFloat("_SPHERERADIUS", sphereExpansionCurve.Evaluate(t));
                yield return null;
            }
        }
        else
        {
            while (t > 0f)
            {
                t -= Time.deltaTime / sphereExpansionTime;
                spherePhaseMat.SetFloat("_SPHERERADIUS", sphereExpansionCurve.Evaluate(t));
                yield return null;
            }
        }

        spherePhaseMat.SetFloat("_SPHERERADIUS", sphereExpansionCurve.Evaluate(phaseIn ? 1f : 0f));

        isPhasedIn = phaseIn;
        phasingRoutine = null;

        SwitchLightcycleGO();
    }

    /// <summary>
    /// Switches lightCycle gameObjects and initiates glass fading.
    /// </summary>
    private void SwitchLightcycleGO()
    {
        lightCycleSpherePhaseGO.SetActive(false);
        lightCycleRuntimeGO.SetActive(true);
        StartCoroutine(FadeGlassRoutine());
    }

    /// <summary>
    /// Routine to manage the glass fading procedure.
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeGlassRoutine()
    {
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / glassFadeTime;
            glassMat.SetFloat("_ALPHATHRESHOLD", glassFadeCurve.Evaluate(t));
            yield return null;
        }

        glassMat.SetFloat("_ALPHATHRESHOLD", 0f);
        OnPhaseInComplete?.Invoke();
    }
    #endregion
}
