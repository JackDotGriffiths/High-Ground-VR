using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioLists { Combat, Building, UI }
public enum AudioMixers {UI, Effects, Music}

public enum MusicArrangements { IdleToCombat, CombatToIdle}
public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance;
    //Audio Mixers
    [SerializeField, Tooltip("UI Audio Mixer")] private AudioMixerGroup m_UIMixer;
    [SerializeField, Tooltip("Effects Audio Mixer")] private AudioMixerGroup m_effectsMixer;
    [SerializeField, Tooltip("Music Audio Mixer")] private AudioMixerGroup m_musicMixer;

    //Sound List
    [Header ("Sound Lists")]
    [Space(10)] public List<Sound> buildingSounds;
    [Space(5)] public List<Sound> combatSounds;
    [Space(5)] public List<Sound> userInterfaceSounds;


    [Header("Music Audio Sources"), Space(10)]
    [SerializeField] private AudioSource m_idleMusic;
    [SerializeField] private AudioSource m_combatMusic;
    [SerializeField, Range(0.1f, 1.0f)] private float m_musicVolume;
    [SerializeField, Range(0.01f, 1.0f)] private float m_musicFadeSpeed;



    public static AudioManager Instance { get => _instance; set => _instance = value; }

    private void Awake()
    {
        if (_instance)
        {
            Destroy(_instance.gameObject);
            _instance = this;
        }

        _instance = this;
    }

    /// <summary>
    /// Plays either a 2D or 3D sound on the passed in targetObject
    /// </summary>
    /// <param name="_name">Name of the clip to play.</param>
    /// <param name="_audioType">The type of audio list to choose from.</param>
    /// <param name="_mixer">Mixer on which to play it on</param>
    /// <param name="_seperateObject">Whether to spawn a seperate object with the source on. Useful for things about to be destroyed.</param>
    /// <param name="_destroyAfterPlaying">Whether to destroy the audioSource after the clip has played.</param>
    /// <param name="_playin2D">Whether to acknowledge the spatialBlend and min/max distance of the clip. If true, spatial blend will be set to 0.</param>
    /// <param name="_targetObject">Target GameObject to attatch the audio source to.</param>
    /// <param name="_pitchShiftAmount">An amount of pitch shift to apply to the audio source. If 0, no pitch shift will occur.</param>
    public void PlaySound(string _name,AudioLists _audioType, AudioMixers _mixer,bool _seperateObject, bool _destroyAfterPlaying, bool _playin2D,GameObject _targetObject,float _pitchShiftAmount)
    {
        Sound _sound = null;
        switch (_audioType)
        {
            case AudioLists.Combat:
                foreach (Sound s in combatSounds)
                {
                    if (s.name == _name)
                    {
                        _sound = s;
                        break;
                    }
                }
                break;
            case AudioLists.Building:
                foreach (Sound s in buildingSounds)
                {
                    if (s.name == _name)
                    {
                        _sound = s;
                        break;
                    }
                }
                break;
            case AudioLists.UI:
                foreach (Sound s in userInterfaceSounds)
                {
                    if (s.name == _name)
                    {
                        _sound = s;
                        break;
                    }
                }
                break;
        }
        if (_sound == null)
        {
            Debug.LogError("Cannot Play \n No sound of name: \"" + name + "\" found");
            return;
        }


        if(_seperateObject == false)
        {
            AudioSource _source = _targetObject.AddComponent<AudioSource>(); //Attach to the target Object. Only really makes a difference if the sound is played in 3D.
            _source.clip = _sound.clip;
            _source.priority = _sound.priority;
            _source.volume = _sound.volume;
            _source.pitch = _sound.pitch + Random.Range(-_pitchShiftAmount, _pitchShiftAmount); //Pitch shifting, if 0 is passed in nothing will happen.

            switch (_mixer) //Set the mixer that the audio clip is meant to play on.
            {
                case AudioMixers.UI:
                    _source.outputAudioMixerGroup = m_UIMixer;
                    break;
                case AudioMixers.Effects:
                    _source.outputAudioMixerGroup = m_effectsMixer;
                    break;
                case AudioMixers.Music:
                    _source.outputAudioMixerGroup = m_musicMixer;
                    break;
            }

            if (_playin2D == true)  //Set spatialBlend based on 2D or 3D audio
            {
                _source.spatialBlend = 0;
            }
            else
            {
                _source.spatialBlend = _sound.spatialBlend;
                _source.minDistance = _sound.minDistance;
                _source.maxDistance = _sound.maxDistance;
            }



            _source.loop = _sound.loop;
            _source.Play();

            if (_destroyAfterPlaying == true) //Destroys the source after playing. Useful for spot effects.
            {
                Destroy(_source, _sound.clip.length);
            }
        }
        else
        {
            GameObject _go = new GameObject("AudioSource for "  + _targetObject.name);
            _go.transform.position = _targetObject.transform.position;
            _go.transform.rotation = _targetObject.transform.rotation;
            _go.transform.localScale = _targetObject.transform.localScale;


            AudioSource _source = _go.AddComponent<AudioSource>(); //Attach to the target Object. Only really makes a difference if the sound is played in 3D.
            _source.clip = _sound.clip;
            _source.priority = _sound.priority;
            _source.volume = _sound.volume;
            _source.pitch = _sound.pitch + Random.Range(-_pitchShiftAmount, _pitchShiftAmount); //Pitch shifting, if 0 is passed in nothing will happen.

            switch (_mixer) //Set the mixer that the audio clip is meant to play on.
            {
                case AudioMixers.UI:
                    _source.outputAudioMixerGroup = m_UIMixer;
                    break;
                case AudioMixers.Effects:
                    _source.outputAudioMixerGroup = m_effectsMixer;
                    break;
                case AudioMixers.Music:
                    _source.outputAudioMixerGroup = m_musicMixer;
                    break;
            }

            if (_playin2D == true)  //Set spatialBlend based on 2D or 3D audio
            {
                _source.spatialBlend = 0;
            }
            else
            {
                _source.spatialBlend = _sound.spatialBlend;
                _source.minDistance = _sound.minDistance;
                _source.maxDistance = _sound.maxDistance;
            }



            _source.loop = _sound.loop;
            _source.Play();

            if (_destroyAfterPlaying == true) //Destroys the source after playing. Useful for spot effects.
            {
                Destroy(_go, _sound.clip.length);
            }
        }
     }


    #region Music Management
    /// <summary>
    /// Used to transition music between the 2 tracks between rounds.
    /// </summary>
    /// <param name="_arragement"></param>
    public void fadeMusic(MusicArrangements _arragement)
    {
        if(_arragement == MusicArrangements.IdleToCombat)
        {
            StartCoroutine(fadeBetweenTracks(m_idleMusic, m_combatMusic));
        }
        else
        {
            StartCoroutine(fadeBetweenTracks(m_combatMusic, m_idleMusic));
        }
    }

    IEnumerator fadeBetweenTracks(AudioSource _source1, AudioSource _source2)
    {
        do
        {
            _source1.volume = _source1.volume - m_musicFadeSpeed;
            _source2.volume = _source2.volume + m_musicFadeSpeed;
            yield return new WaitForSeconds(0.1f);
        } while (_source2.volume <= m_musicVolume && _source1.volume >= 0);
        yield return null;
    }


    #endregion
}