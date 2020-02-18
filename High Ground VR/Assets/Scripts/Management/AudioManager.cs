using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum SoundLists {placeBuildings, weaponClashes}


    private static AudioManager s_instance;

    public List<Sound> weaponClashes; //List of weapon clash sounds
    public List<Sound> placeBuildings; //List of building placement sounds


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
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="seperateObject"></param>
    /// <param name="destroy"></param>
    public void PlaySound(SoundLists _list, int _index, GameObject _audioTarget, bool _spawnSeperateObject, bool _destroySource)
    {
        List<Sound> _chosenList = new List<Sound>();
        //Pick a random Sounds
        switch(_list)
        {
            case SoundLists.weaponClashes:
                _chosenList = weaponClashes;
                break;
            case SoundLists.placeBuildings:
                _chosenList = placeBuildings;
                break;
        }
        Sound s = _chosenList[_index];
        if (_spawnSeperateObject == true)
        {
            GameObject _seperateObject = new GameObject(_audioTarget.name + " Audio Source. Playing - " + name);
            _seperateObject.transform.position = _audioTarget.transform.position;
            _audioTarget = _seperateObject;
        }
        AudioSource _source = _audioTarget.AddComponent<AudioSource>();
        _source.clip = s.clip;
        _source.volume = s.volume;
        _source.pitch = s.pitch;
        _source.loop = s.loop;
        _source.priority = s.priority;
        _source.rolloffMode = AudioRolloffMode.Linear;
        _source.minDistance = 30;
        _source.maxDistance = 1000;
        _source.spatialBlend = 1f;
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
