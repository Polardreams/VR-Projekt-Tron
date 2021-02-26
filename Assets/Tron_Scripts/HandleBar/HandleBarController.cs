using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages lineRenderer between grab points and invokes event when bnoth grab points have been grabbed.
/// </summary>
public class HandleBarController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// LineRenderer represending the handleBar
    /// </summary>
    [SerializeField] private LineRenderer handlebarLine = null;

    /// <summary>
    /// Transform of the left hrabbing point.
    /// </summary>
    [SerializeField] private Transform leftGrabPoint = null;

    /// <summary>
    /// transform of the right grabbing point.
    /// </summary>
    [SerializeField] private Transform rightGrabPoint = null;

    /// <summary>
    /// Reference to the controller grabbing the right grab point.
    /// </summary>
    private ReadController grabbingRightController = null;

    /// <summary>
    /// Reference to the controller grabbing the left grab point.
    /// </summary>
    private ReadController grabbingLeftController = null;

    /// <summary>
    /// Event called, when both grab Points have been grabbed.
    /// </summary>
    public UnityEvent OnBarGrabbed = null;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        leftGrabPoint.GetComponent<HandleGrabPointController>().handleBar = this;
        rightGrabPoint.GetComponent<HandleGrabPointController>().handleBar = this;
    }

    private void Update()
    {
        UpdateLineRenderer();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Assign grabbing controller.
    /// </summary>
    /// <param name="controller">The grabbing controller</param>
    /// <param name="isRight">Which side does the controller belong to?</param>
    public void AssignGrabbingController(ReadController controller, bool isRight)
    {
        if (isRight)
        {
            if (grabbingRightController)
            {
                Debug.LogWarning("Right controller has already been assigned.");
                return;
            }

            grabbingRightController = controller;
        }
        else
        {
            if (grabbingLeftController)
            {
                Debug.LogWarning("Left controller has already been assigned.");
                return;
            }

            grabbingLeftController = controller;
        }

        if (grabbingRightController && grabbingLeftController)
            OnBarGrabbed?.Invoke();
    }

    /// <summary>
    /// Updates the lineRenderer to the grab points positions.
    /// </summary>
    private void UpdateLineRenderer()
    {
        handlebarLine.SetPositions(new Vector3[] { leftGrabPoint.transform.localPosition, rightGrabPoint.transform.localPosition });
    }
    #endregion
}
