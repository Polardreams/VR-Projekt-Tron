using UnityEngine;

/// <summary>
/// Steuert die SFX des Bikes das beinhaltet
/// Pitch und Lautstärke des Motorgeräusches 
/// Stereo Pan : das Verschieben der Stereo position des Motorgeräusches
/// </summary>
public class SFXController : MonoBehaviour
{

    #region Settings
    /// <summary>
    /// bike root transform
    /// </summary>
    [SerializeField]
    private Transform motorbike = null;
    
    /// <summary>
    /// minimaler Pitch 
    /// </summary>
    [SerializeField]
    private float minPitch = 0.5f;
    
    /// <summary>
    /// maximaler Pitch
    /// </summary>
    [SerializeField]
    private float maxPitch = 1f;
    
    /// <summary>
    /// minimale Lautstärke
    /// </summary>
    [SerializeField]
    private float minVolume = 0.1f;
    
    /// <summary>
    /// maximaler Lautstärke
    /// </summary>
    [SerializeField]
    private float maxVolume = 0.5f;
    
    /// <summary>
    /// Annäherungsgeschwindigkeit von Pitch und Lautstärke bei Veränderungen der Geschwindigkeit
    /// </summary>
    [SerializeField]
    private float interpolationSpeed = 0.9f;
    
    /// <summary>
    /// Möglichkeit zum Abschalten der Stereo Pan's
    /// </summary>
    [SerializeField]
    private bool stereoManipulation = true;

    /// <summary>
    /// Richtung des Stereo Pan's | Links <-> Rechts
    /// </summary>
    [SerializeField]
    private bool switchStereoDirection = false;

    /// <summary>
    /// Interpolationszeit
    /// </summary>
    private float interpolationTime = 0f;

    /// <summary>
    /// AudioSource Bike SFX
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// maximale Geschwindigkeit
    /// </summary>
    private float maxSpeed;

    /// <summary>
    /// Bereich zwischen minimaler und maximaler Lautstärke
    /// </summary>
    private float volumeRange;

    /// <summary>
    /// Bereich zwischen minimalen und maximalen Pitch
    /// </summary>
    private float pitchRange;

    /// <summary>
    /// maximal mögliche Neigung
    /// </summary>
    private float maxTilt;

    /// <summary>
    /// Faktor zum drehen der Stereo Pan Richtung
    /// </summary>
    private float stereoDirection =-1f;

    #endregion

    #region Unity Callsbacks
    /* SFX für das Motorrad sollen abhängig von der Geschwindigkeit abgespielt werden.
     * Es werden Lautstärke und Pitch manipuliert.
    */
    private void Start()
    {
        audioSource = transform.GetComponent<AudioSource>();

        maxSpeed = motorbike.GetComponent<moving_v1>().max_velocity;
        audioSource.pitch = minPitch;
        audioSource.volume = minVolume;
        volumeRange = maxVolume - minVolume;
        pitchRange = maxPitch - minPitch;
                
        maxTilt = motorbike.GetComponent<moving_v1>().max_tilt;
       
        if (switchStereoDirection)
            stereoDirection = 1f;
    }

    private void Update()
    {
        float currentSpeed = motorbike.GetComponent<moving_v1>().velocity;
        float speedAmount = currentSpeed / maxSpeed;

        /* die angepeilte Lautstärke und Pitch werden berechnet | die Annäherung mit Lerp verhindert ein Poppen/Klicken bei starken Geschwindigkeitsveränderungen
         */
        if (currentSpeed > 0f)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else
            {
                transform.position = motorbike.position;

                float desiredVolume = minVolume + (volumeRange * speedAmount);
                audioSource.volume = Mathf.Lerp(audioSource.volume, desiredVolume, speedAmount);
                float desiredPitch = minPitch + (pitchRange * speedAmount);
                audioSource.pitch = Mathf.Lerp(audioSource.pitch, desiredPitch, speedAmount);
            }
        }

        /* Pitch und Volume der Bike SFX werden herunter gedreht
         * sobald die Mindestlautstärke erreicht wurde, wird die Audiowiedergabe gestoppt
         */
        else if (currentSpeed < 0.1f)
        {
            if (!audioSource.isPlaying)
                return;

            interpolationTime += Time.deltaTime;
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, minPitch, interpolationTime * interpolationSpeed);
            audioSource.volume = Mathf.Lerp(audioSource.volume, minVolume, interpolationTime * interpolationSpeed);

            if (audioSource.volume == minVolume)
            {
                audioSource.pitch = minPitch;
                audioSource.Stop();
                interpolationTime = 0f;
            }
        }

        /* abhängig von der Neigung des bikes wird der Stereo Pan angepasst
         * Um mit maxTilt vergleichen und zwischen Links und Rechts unterscheiden zu können, werden Neigungen über 180° mit -360 addiert.
         */
        if (stereoManipulation)
        {
            float bikeTilt = motorbike.rotation.eulerAngles.z;
            if (bikeTilt > 180f)
                bikeTilt = -360f + bikeTilt;

            audioSource.panStereo = stereoDirection * (bikeTilt / maxTilt);
        }
    }
    #endregion
}
