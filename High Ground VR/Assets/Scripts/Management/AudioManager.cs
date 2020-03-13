using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public enum AudioMixers {UI, Effects, Music}
public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance;
    //Audio Mixers
    [SerializeField, Tooltip("UI Audio Mixer")] private AudioMixerGroup m_UIMixer;
    [SerializeField, Tooltip("Effects Audio Mixer")] private AudioMixerGroup m_effectsMixer;
    [SerializeField, Tooltip("Music Audio Mixer")] private AudioMixerGroup m_musicMixer;

    //Sound List
    [SerializeField, Header("Local Sounds"),Space(20)] private Sound[] m_sounds;


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
    /// <param name="_mixer">Mixer on which to play it on</param>
    /// <param name="_destroyAfterPlaying">Whether to destroy the audioSource after the clip has played.</param>
    /// <param name="_playin2D">Whether to acknowledge the spatialBlend and min/max distance of the clip. If true, spatial blend will be set to 0.</param>
    /// <param name="_targetObject">Target GameObject to attatch the audio source to.</param>
    /// <param name="_pitchShiftAmount">An amount of pitch shift to apply to the audio source. If 0, no pitch shift will occur.</param>
    public void PlaySound(string _name,AudioMixers _mixer, bool _destroyAfterPlaying, bool _playin2D,GameObject _targetObject,float _pitchShiftAmount)
    {
        Sound _sound = null;
        foreach (Sound s in m_sounds)
        {
            if (s.name == _name)
            {
                _sound = s;
                break;
            }
        }
        if (_sound == null)
        {
            Debug.LogError("Cannot Play \n No sound of name: \"" + name + "\" found");
            return;
        }


        AudioSource _source = _targetObject.AddComponent<AudioSource>(); //Attach to the target Object. Only really makes a difference if the sound is played in 3D.
        _source.clip = _sound.clip;
        _source.priority = _sound.priority;
        _source.volume = _sound.volume;
        _source.pitch = _sound.pitch + Random.Range(-_pitchShiftAmount,_pitchShiftAmount); //Pitch shifting, if 0 is passed in nothing will happen.

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

        if(_playin2D == true)  //Set spatialBlend based on 2D or 3D audio
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

        if(_destroyAfterPlaying == true) //Destroys the source after playing. Useful for spot effects.
        {
            Destroy(_source, _sound.clip.length);
        }

    }
}