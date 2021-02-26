using UnityEngine;

/// <summary>
/// Controller for one of two grabbing points of the handle bar
/// </summary>
public class HandleGrabPointController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// Controller of the handlebar
    /// </summary>
    [HideInInspector] public HandleBarController handleBar = null;

    /// <summary>
    /// Is this grabbing point on the left side?
    /// </summary>
    [SerializeField] private bool isLeft = false;

    /// <summary>
    /// Latest gameObject with readController component, which entered this gameObjects trigger collider
    /// </summary>
    private ReadController currentController = null;

    /// <summary>
    /// Is this grabPoint already grabbed?
    /// </summary>
    private bool isGrabbed = false;

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
    private float hoverDistance = 0.05f;

    /// <summary>
    /// A velocity reference for smoothDamp operation
    /// </summary>
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Duration of the current hovering time frame.
    /// </summary>
    private float hoverTimeFrame = 1f;

    /// <summary>
    /// Remaining time of the current hover time frame.
    /// </summary>
    private float hoverTime = 0f;

    /// <summary>
    /// Maximal distance a grab Point will follow its currentController during glued movement.
    /// </summary>
    private float glueDistance = 0.1f;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        localStartPos = transform.localPosition;

        for (int i = 0; i < localHoverTargets.Length; i++)
            localHoverTargets[i] = localStartPos;
    }

    private void Update()
    {
        if (!isGrabbed)
        {
            SmoothHovering();

            if (currentController)
            {
                GlueToController();
                CheckForGrab();
            }
        }
        else
        {
            FollowController();
        }
    }

    //Check for incoming controller
    private void OnTriggerEnter(Collider other)
    {
        ReadController controller = other.GetComponent<ReadController>();

        if (controller)
        {
            if (isGrabbed || controller.isLeft != isLeft || currentController == controller)
                return;

            currentController = controller;
        }
    }

    //Check for currentController leaving
    private void OnTriggerExit(Collider other)
    {
        ReadController controller = other.GetComponent<ReadController>();

        if (controller)
        {
            if (isGrabbed || controller != currentController)
                return;

            currentController = null;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Small movement of the grabPoint while it waits to be grabbed.
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
    /// Glue movement of grabPoint to controller within a set radius
    /// </summary>
    private void GlueToController()
    {
        float distanceToController = Vector3.Distance(currentController.transform.position, transform.parent.TransformPoint(localStartPos));

        if (distanceToController <= glueDistance)
            transform.position = Vector3.Lerp(transform.position, currentController.transform.position, 0.1f);
        else
            transform.position = Vector3.Lerp(transform.position, (currentController.transform.position - transform.parent.TransformPoint(localStartPos)).normalized * glueDistance + transform.parent.TransformPoint(localStartPos), 0.1f); 
    }

    /// <summary>
    /// Check currentController for pressed gripButton.
    /// </summary>
    private void CheckForGrab()
    {
        if (currentController.gripButton)
        {
            isGrabbed = true;
            handleBar.AssignGrabbingController(currentController, isLeft);
        }
    }

    /// <summary>
    /// Keep grabPoint snapped to the currentController once it grabbed this grabPoint.
    /// </summary>
    private void FollowController()
    {
        transform.position = currentController.transform.position;
    }
    #endregion
}
