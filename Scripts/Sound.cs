using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string name;
    public AudioClip clip;

    [Range(0f,1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    [Range(-3f, 0f)]
    public float pitchRandomMin;
    [Range(0f, 3f)]
    public float pitchRandomMax;
    [Range(0f, 1f)]
    public float spatialBlend;

    [Range(0f, 5f)]
    public float dopplerLevel;


    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
