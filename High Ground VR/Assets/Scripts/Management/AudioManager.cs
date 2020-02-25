using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundLists { placeBuildings, weaponClashes,enemySpawning };
public class AudioManager : MonoBehaviour
{
    private static AudioManager s_instance;

    [SerializeField, Tooltip("Variation on pitch.")] private float m_pitchVariation; //How much to add onto/take away from the pitch once playing.

    [Header("Enemy Spawning")]
    [SerializeField, Tooltip("Enemy Spawning Sounds."), Space(10)] public List<AudioClip> enemySpawning; //List of building placement sounds
    [SerializeField, Tooltip("Enemy Spawning Volume"), Range(0.0f, 1.0f)] private float m_enemySpawningVolume; //How much to add onto/take away from the pitch once playing.
    [SerializeField, Tooltip("Enemy Spawning Sound Max Distance")] private float m_enemyMaxDistance; //Max distance to hear the sound

    [Header ("Weapon Clashes")]
    [SerializeField, Tooltip("Weapon Clash Sounds."),Space(10)] public List<AudioClip> weaponClashes; //List of weapon clash sounds
    [SerializeField, Tooltip("weaponClash Volume"),Range(0.0f,1.0f)] private float m_weaponClashVolume; //How much to add onto/take away from the pitch once playing.
    [SerializeField, Tooltip("Weapon Clashes Sound Max Distance")] private float m_weaponClashesMaxDistance; //Max distance to hear the sound

    [Header("Placing Buildings")]
    [SerializeField, Tooltip("Building Placement Sounds."),Space(10)] public List<AudioClip> placeBuildings; //List of building placement sounds
    [SerializeField, Tooltip("place Building Volume"), Range(0.0f, 1.0f)] private float m_placeBuildingVolume; //How much to add onto/take away from the pitch once playing.
    [SerializeField, Tooltip("Place Buildings Sound Max Distance")] private float m_placeBuildingMaxDistance; //Max distance to hear the sound


    public static AudioManager Instance { get => s_instance; set => s_instance = value; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Manages playing a sound from any of the lists of sound clips. Cannot play looping audio.
    /// </summary>
    /// <param name="_list">Which list of sounds to choose from. </param>
    /// <param name="_randomIndex">If this is true, a random index will be chosen and the index inputted will be ignored.</param>
    /// <param name="_index">The index of the desired sound within the chosen list.</param>
    /// <param name="_audioTarget">Which object to attatch the audio source to.</param>
    /// <param name="_pitchVariation">Whether to add pitch variation to this sound.</param>
    /// <param name="_spawnSeperateObject">Where to play this audio clip on a seperate object, helpful if object is about to be destroyed.</param>
    /// <param name="_destroySource">Whether to destroy the audio source after it is done playing. In most cases this is true.</param>
    public void PlaySound(SoundLists _list,bool _randomIndex, int _index, GameObject _audioTarget, bool _pitchVariation, bool _spawnSeperateObject, bool _destroySource)
    {
        List<AudioClip> _chosenList = new List<AudioClip>();
        float _chosenVolume = 0.0f;
        float _maxDistance = 1000.0f;

        //Pick a random Sounds
        switch(_list)
        {
            case SoundLists.weaponClashes:
                _chosenList = weaponClashes;
                _chosenVolume = m_weaponClashVolume;
                _maxDistance = m_weaponClashesMaxDistance;
                break;
            case SoundLists.placeBuildings:
                _chosenList = placeBuildings;
                _chosenVolume = m_placeBuildingVolume;
                _maxDistance = m_placeBuildingMaxDistance;
                break;
            case SoundLists.enemySpawning:
                _chosenList = enemySpawning;
                _chosenVolume = m_enemySpawningVolume;
                _maxDistance = m_enemyMaxDistance;
                break;
        }


        if (_spawnSeperateObject == true)
        {
            GameObject _seperateObject = new GameObject(_audioTarget.name + " Audio Source. Playing - " + name);
            _seperateObject.transform.position = _audioTarget.transform.position;
            _audioTarget = _seperateObject;
        }
        AudioSource _source = _audioTarget.AddComponent<AudioSource>();


        if(_randomIndex == true)
        {
            _source.clip = _chosenList[Random.Range(0,_chosenList.Count)];
        }
        else
        {
            _source.clip = _chosenList[_index];

        }




        _source.volume = _chosenVolume;
        if(_pitchVariation == true)
        {
            _source.pitch = 1 + Random.Range(-m_pitchVariation,m_pitchVariation);
        }
        else
        {
            _source.pitch = 1;
        }


        _source.loop = false;
        _source.rolloffMode = AudioRolloffMode.Logarithmic;
        _source.minDistance = 0.1f;
        _source.maxDistance = _maxDistance;
        _source.spatialBlend = 1.0f;
        _source.Play();
        if (_destroySource == true)
        {
            if (_spawnSeperateObject == true)
            {
                Destroy(_audioTarget, _source.clip.length);
            }
            else
            {
                Destroy(_source, _source.clip.length);
            }
        }
    }

}
