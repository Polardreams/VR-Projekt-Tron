using System.Collections;
using UnityEngine;

/// <summary>
/// Steuert die Musik und den Intro-SFX beim "Hochfahren" des Bikes.
/// Aus einer Liste an WAV files wird ein zufälliger Song abgespielt.
/// Ausgenommen davon ist der erste WAV file in der Liste (Index 0).
/// Nach einer kurzen Wartezeit zum Überbrücken von Ladezeiten der Scene, wird der Intro-SFX abgespielt.
/// </summary>

public class MusicController : MonoBehaviour
{
    #region Settings
    /// <summary>
    /// Liste der möglichen WAV files / Songs
    /// </summary>
    [SerializeField]
    AudioClip[] clips = null;

    /// <summary>
    /// Wartezeit um Ladezeiten der Scene zu überbrücken
    /// </summary>
    [SerializeField]
    float introWaitForSeconds = 1f;

    /// <summary>
    /// bike root
    /// </summary>
    private Transform bike;

    /// <summary>
    /// AudioSource für Musik
    /// </summary>
    private AudioSource audioComponent;

    /// <summary>
    /// bool zur Überprüfung des Bike-Boot-Status
    /// </summary>
    private bool bootedBike;
    #endregion

    #region Unity Callbacks

    /// <summary>
    /// AudioComponent des bikes wird geholt
    /// bool bootedBike um festzustellen ob der Intro-SFX bereits abgespielt wurde
    /// Wartezeit wird gestartet
    /// </summary>
    private void Start()
    {
        bike = GameObject.Find("motorbike_root").transform;
        audioComponent = transform.GetComponent<AudioSource>();
        bootedBike = false;
        StartCoroutine(IntroWait());
    }

    /// <summary>
    /// Um die Objects und Scripts unabhängig zu halten, beindet sich dieses Script an einem eigenem GameObject und nicht an dem Bike selbst.
    /// Daher muss die Position angeglichen werden um im Hörradius zu bleiben.
    /// Wird kein Song abgespielt (der vorherige Song ist also zu ende) und das Bike ist hochgefahren, wird der nächste zufällige Song abgespielt.
    /// </summary>
    private void Update()
    {
        transform.position = bike.position;

        if (!audioComponent.isPlaying)
        {
            if (!bootedBike)
                return;
            PlayNextClip(RandomIndex());
        }
    }
    #endregion

    #region Methods

    /// <summary>
    /// Zufälliger float
    /// Index 0 ist der Intro-SFX und wird in der Auswahl nicht berücksichtigt
    /// </summary>
    /// <returns></returns>
    private int RandomIndex()
    {
        int randomIndex = Random.Range(1, clips.Length);
        return randomIndex;
    }

    /// <summary>
    /// alter Clip wird gestoppt, Clip wird gewechselt, neuer Clip wird abgespielt
    /// </summary>
    /// <param name="index"></param>
    private void PlayNextClip(int index)
    {
        audioComponent.Stop();
        audioComponent.clip = clips[index];
        audioComponent.Play();
    }

    /// <summary>
    /// es wird die angegebene Anzahl an Sekunden gewartet bevor der Intro-SFX abgespielt wird
    /// </summary>
    /// <returns></returns>
    IEnumerator IntroWait()
    {
        yield return new WaitForSeconds(introWaitForSeconds);
        PlayNextClip(0);
    }

    /// <summary>
    /// LightCycleController.cs auf dem Object "LightCycle Smooth Prefab" löst diese Funktion aus sobald der FadeIn des Bikes abgeschlossen ist
    /// </summary>
    public void PlayMusic()
    {
        bootedBike = true;
    }

    /// <summary>
    /// Der Boot SFX wird abgespielt wenn das Motorad hoch gefahren wird
    /// </summary>
    public void PlayOnBoot()
    {
        PlayNextClip(0);
    }
    #endregion

}
