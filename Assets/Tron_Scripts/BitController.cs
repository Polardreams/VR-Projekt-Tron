using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages the States of the Bit
/// </summary>
public class BitController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// Holder gameObject of the bits default states.
    /// </summary>
    [SerializeField] private Transform defaultStateHolder = null;

    /// <summary>
    /// First variant of the bits default state.
    /// </summary>
    [SerializeField] private Transform defaultState01 = null;

    /// <summary>
    /// Secondd variant of the bits default state.
    /// </summary>
    [SerializeField] private Transform defaultState02 = null;

    /// <summary>
    /// The bits true state.
    /// </summary>
    [SerializeField] private Transform trueState = null;

    /// <summary>
    /// The bits false state.
    /// </summary>
    [SerializeField] private Transform falseState = null;

    /// <summary>
    /// AnimationCurve to control the bits scaling during transitions.
    /// </summary>
    [Space]
    [SerializeField] private AnimationCurve scalingCurve = new AnimationCurve();

    /// <summary>
    /// Value between 0 and 1 to toggle between the bits two default state variants.
    /// </summary>
    private float defaultCycleValue = 0f;

    /// <summary>
    /// How many seconds should one cycle take?
    /// A cycle is the bits' change to another state and back.
    /// </summary>
    [SerializeField] private float cycleTimeframe = 1f;

    /// <summary>
    /// Duration of the current hovering time frame.
    /// </summary>
    private float hoverTimeFrame = 1f;

    /// <summary>
    /// Remaining time of the current hover time frame.
    /// </summary>
    private float hoverTime = 0f;

    /// <summary>
    /// Startposition of grabPoint in its parent coordinate system
    /// </summary>
    private Vector3 localStartPos = Vector3.zero;

    /// <summary>
    /// Vector3 array. Contains the points for random hover movements of grab point while being ungrabbed.
    /// </summary>
    private Vector3[] localHoverTargets = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };

    /// <summary>
    /// Max distance from local position on start to hover from.
    /// </summary>
    private float hoverDistance = 0.02f;

    /// <summary>
    /// A velocity reference for smoothDamp operation
    /// </summary>
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Routine holder for easy access.
    /// </summary>
    private Coroutine trueFalseRoutine = null;

    /// <summary>
    /// script to control SFX for Bit
    /// </summary>
    private SfxBitController sfxBitController;

    /// <summary>
    /// Queue of states the bit is supposed to display. Workaround for player entering new tile and immediatly crashing
    /// </summary>
    private Queue<bool> stateQueue = new Queue<bool>();
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        localStartPos = transform.localPosition;

        for (int i = 0; i < localHoverTargets.Length; i++)
            localHoverTargets[i] = localStartPos;

        trueState.localScale = Vector3.zero;
        falseState.localScale = Vector3.zero;
        sfxBitController = transform.GetComponent<SfxBitController>();
    }

    private void Update()
    {
        UpdateDefaultState();

        if (stateQueue.Count > 0 && trueFalseRoutine == null)
            trueFalseRoutine = StartCoroutine(TrueFalseRoutine(stateQueue.Dequeue()));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Frame by frame update of the local scale of the bits default state.
    /// </summary>
    private void UpdateDefaultState()
    {
        // ranges from 0 to 1. Frequency depends on cycleTimeFrame. Starts at 0.
        defaultCycleValue = Mathf.PingPong(Time.time / cycleTimeframe, 1f);

        defaultState01.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, scalingCurve.Evaluate(defaultCycleValue));
        defaultState02.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, scalingCurve.Evaluate((1 - defaultCycleValue)));

        // Put random movement here
        SmoothHovering();
    }

    /// <summary>
    /// Small movement of the bit.
    /// Supposed to create a smooth translation between three points without major cuts.
    /// </summary>
    private void SmoothHovering()
    {
        if (hoverTime <= 0f)
        {
            for (int i = 1; i < localHoverTargets.Length; i++)
                localHoverTargets[i] = localHoverTargets[i - 1];

            localHoverTargets[0] = localStartPos + Random.insideUnitSphere * hoverDistance;
            hoverTime = hoverTimeFrame;
        }

        hoverTime -= Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, Utility.GetPoint(transform.parent.TransformPoint(localHoverTargets[0]), transform.parent.TransformPoint(localHoverTargets[1]), transform.parent.TransformPoint(localHoverTargets[2]), hoverTime), ref velocity, 0.5f);
    }

    /// <summary>
    /// Set bit to true or false state.
    /// </summary>
    /// <param name="state">True or False state.</param>
    public void TriggerState(bool state)
    {
        if (trueFalseRoutine != null)
        {
            stateQueue.Enqueue(state);
            return;
        }

        trueFalseRoutine = StartCoroutine(TrueFalseRoutine(state));
    }

    /// <summary>
    /// Routine to manage the transition to its true or false state.
    /// </summary>
    /// <param name="state">True or False state.</param>
    /// <returns></returns>
    IEnumerator TrueFalseRoutine(bool state)
    {
        float t = 0f;

        // transition to state
        while (t < 1f)
        {
            t += Time.deltaTime * (1 / cycleTimeframe) * 2f;

            if (state)
            {
                trueState.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scalingCurve.Evaluate(t));
                sfxBitController?.BitTalk(1);
            }
            else
            {
                falseState.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scalingCurve.Evaluate(t));
                sfxBitController?.BitTalk(0);
            }

            defaultStateHolder.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, scalingCurve.Evaluate(t));

            yield return null;
        }

        yield return new WaitForSeconds(cycleTimeframe);

        t = 0f;

        // transition to default
        while (t < 1f)
        {
            t += Time.deltaTime * (1 / cycleTimeframe) * 2f;

            if (state)
                trueState.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, scalingCurve.Evaluate(t));
            else
                falseState.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, scalingCurve.Evaluate(t));

            defaultStateHolder.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, scalingCurve.Evaluate(t));

            yield return null;
        }

        trueFalseRoutine = null;
    }
    #endregion
}
