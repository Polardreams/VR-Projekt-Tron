using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// gewährleistet eine ständige Ausrichtung zur Player Camera
/// </summary>
public class FacePlayerCam : MonoBehaviour
{
    [SerializeField]
    private Transform playerCam = null;

    void Update()
    {
        transform.LookAt(playerCam);
    }
}
