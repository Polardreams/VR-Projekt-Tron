using UnityEngine;

/// <summary>
/// bestimmt die Position folgender Streckenabschnitte und löst die Instanzierung dessen aus+
/// Funktioniert in Kombination mit RoadTileController.cs
/// </summary>
public class CreateNextTile : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// position für folgenden Streckenabschnitt
    /// </summary>
    [SerializeField]
    private Transform link = null;

    /// <summary>
    /// Script zur Erstellung der Streckenabschnitte
    /// </summary>
    private RoadTileController roadTileController;

    /// <summary>
    /// Position des folgenden Streckenabschnitts
    /// </summary>
    //private Vector3 newTilePosition;

    /// <summary>
    /// Script zur Steuerung des "Bit"'s
    /// </summary>
    private BitController bitController;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        roadTileController = GameObject.Find("Tiles").GetComponent<RoadTileController>();
        //Transform ground = transform.Find("ground");
        //newTilePosition = transform.position + (Vector3.forward * ground.localScale.x * 1000f);
        bitController = GameObject.Find("Bit").GetComponent<BitController>();
    }
        
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            roadTileController.CreateNewTile(link);
            bitController.TriggerState(true);
        }
    }
    #endregion
}
