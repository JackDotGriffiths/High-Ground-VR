using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name; //Name of the audio clip, used to call and play it.
    public string description; //Optional description of the audio clip. Helps with working out which is which later on
    public AudioClip clip; //Clip file itself
    [Range(0f, 1f)] public float volume = 0.5f; //Volume to play clip at
    [Range(-3.0f, 3.0f)] public float pitch = 0.0f; //Pitch to play clip at 
    [Range(0, 256)] public int priority = 0; //Priority of clip 
    [Range(0.0f, 1.0f)] public float spatialBlend; //Spatial blend of clip
    public int minDistance; // Min distance of the 3D audio
    public int maxDistance; // Max distance of the 3D audio
    public bool loop; //Whether or not to Loop.
}