using UnityEngine;
using TMPro;

/// <summary>
/// Steuert die SFX des Bit's
/// zählt die erreichten Streckenabschnitte
/// </summary>
/// 
public class SfxBitController : MonoBehaviour
{

    #region Settings
    /// <summary>
    /// Liste der verwendeten Audioclips | "ja" und "Nein"
    /// </summary>
    [SerializeField]
    AudioClip[] clips = new AudioClip[2];

    /// <summary>
    /// AudioSource des Bit's
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// HUD element zur Anzeige der erreichten Streckenabschnitte
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI roadTileNr = null;

    /// <summary>
    /// Anzahl der erreichten Streckenabschnitte
    /// </summary>
    private int nr = 0;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        audioSource = transform.GetComponent<AudioSource>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Spielt entsprechenden AudioClip ab | index 0 : "Nein" | index 1 : "Ja"
    /// Für jeden erreichten Streckenabschnitt wird der Counter erhöht
    /// Für jeden Zusammenprall mit Hindernissen wird der Counter gesenkt
    /// </summary>
    /// <param name="index"></param>
    /// 
    public void BitTalk(int index)
    {
        if (audioSource.isPlaying)
        {
            if (audioSource.clip == clips[index])
                return;
        }

        if (index == 1)
            nr += 1;
        else if (index == 0)
            nr -= 1;

        audioSource.Stop();
        audioSource.clip = clips[index];
        audioSource.Play();

        roadTileNr.text = nr.ToString();
    }
    #endregion
}