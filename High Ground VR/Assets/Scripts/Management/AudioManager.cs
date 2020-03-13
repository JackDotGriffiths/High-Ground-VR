using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance;



    //sounds
    [SerializeField, Header("Local Sounds")]
    private Sound[] localSounds;
    [SerializeField]
    private AudioMixerGroup localGameSounds;

    public static AudioManager Instance { get => _instance; set => _instance = value; }

    private void Awake()
    {
        if (_instance)
        {
            Destroy(_instance.gameObject);
            _instance = this;
        }

        _instance = this;

        foreach (Sound s in localSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.priority = s.priority;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.outputAudioMixerGroup = localGameSounds;
            s.source.spatialBlend = 0;
            s.source.loop = s.loop;
        }
    }

    public void PlayLocalSound(string name)
    {
        Sound _sound = null;
        foreach (Sound s in localSounds)
        {
            if (s.name == name)
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
        _sound.source.Play();
    }

    public void StopLocalSound(string name)
    {
        Sound _sound = null;
        foreach (Sound s in localSounds)
        {
            if (s.name == name)
            {
                _sound = s;
                break;
            }
        }

        if (_sound == null)
        {
            Debug.LogError("Cannot Stop \n No sound of name: \"" + name + "\" found");
            return;
        }

        _sound.source.Stop();
    }

    public bool isLocalSoundPlaying(string name)
    {
        Sound _sound = null;
        foreach (Sound s in localSounds)
        {
            if (s.name == name)
            {
                _sound = s;
                break;
            }
        }

        if (_sound == null)
        {
            Debug.LogError("Cannot return time \n No sound of name: \"" + name + "\" found");
            return false;
        }

        return _sound.source.isPlaying;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}