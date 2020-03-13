using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name; //Name of the audio clip, used to call and play it.
    public string description; //Optional description of the audio clip. Helps with working out which is which later on
    public AudioClip clip; //Clip file itself
    [Range(0f, 1f)] public float volume; //Volume to play clip at
    [Range(0.1f, 3f)] public float pitch; //Pitch to play clip at 
    [Range(0, 256)] public int priority; //Priority of clip 
    [Range(0.0f, 1.0f)] public float spatialBlend; //Spatial blend of clip
    public int minDistance; // Min distance of the 3D audio
    public int maxDistance; // Max distance of the 3D audio
    public bool loop; //Whether or not to Loop.
}