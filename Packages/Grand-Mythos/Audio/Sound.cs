using UnityEngine;

[System.Serializable]
public class Sound
{
    public required AudioClip clip;
    public float transitionTime;


    [Range(0f, 1f)]
    public float volume;

}
