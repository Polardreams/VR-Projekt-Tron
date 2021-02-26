using UnityEngine;

/// <summary>
/// Erstellt neue Strecken Abschnitte zufällig ausgewählt anhand einer Liste
/// Abschnitte die nicht mehr benötigt werden, werden zerstört
/// Funktioniert in Kombinantion mit CreateNextTile.cs auf den Prefabs der Streckenabschnitte
/// </summary>

public class RoadTileController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// Liste der möglichen Streckenabschnitte
    /// </summary>
    [SerializeField]
    GameObject[] Tiles = null;

    /// <summary>
    /// Startbereich
    /// </summary>
    [SerializeField]
    GameObject startArea = null;

    [SerializeField]
    private Transform startLink = null;
    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Es wird sichergestellt, dass das Startfeld aktiv ist
    /// der erste Abschnitt wird erstellt
    /// </summary>
    private void Start()
    {
        startArea.SetActive(true);
        CreateNewTile(startLink);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Bei Kollision (Trigger) mit dem Anfang eines Streckenabschnitts wird diese Funktion ausgelöst
    /// (CreateNextTile.cs) 
    /// Die Abschnitte geben die Position des darauf folgenden Abschnitts an
    /// Ein neuer Abschnitt wird instanziert und der älteste Abschnitt in der Liste (index 0) wird zerstört
    /// </summary>
    /// <param name="position"></param>
    public void CreateNewTile(Transform link)
    {
        Instantiate(Tiles[RandomIndex()], link.position, link.rotation, transform);
        if (transform.childCount > 2)
            Destroy(transform.GetChild(0).gameObject);
    }

    /// <summary>
    /// erstellt einen zufälligen float
    /// </summary>
    /// <returns></returns>
    private int RandomIndex()
    {
        int randomIndex = Random.Range(0, Tiles.Length);
        return randomIndex;
    }
    #endregion
}
