using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // VARIABLES
    public static AudioManager _instance;
    AudioSource BGMSource;
    AudioSource sfxSource;

    public Sound overworldTheme;
    public Sound battleTheme;

    //UPDATES
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        BGMSource = gameObject.AddComponent<AudioSource>();
        BGMSource.loop = true;
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    //METHODS
    public void SwitchMusicTrackEvent(GameState gs)
    {
        switch(gs)
        {
            case GameState.Overworld:
                //PlayMusicWithFade(overworldTheme);
                PlayMusicWithDelay(overworldTheme);
                break;

            case GameState.Battle:
                PlayMusicWithFade(battleTheme);
                break;
        }
    }

    #region Switching Tracks
    public void PlayMusicWithDelay(Sound music)
    {
        // Play Song, with a Delay window
        StartCoroutine(UpdateMusicWithDelay(music, 1f));
    }

    IEnumerator UpdateMusicWithDelay(Sound musicToPlay, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        BGMSource.clip = musicToPlay.clip;
        BGMSource.volume = musicToPlay.volume;
        BGMSource.Play();

    }

    public void PlayMusicWithFade(Sound music)
    {
        StartCoroutine(UpdateMusicWithFade(music));
    }

    IEnumerator UpdateMusicWithFade(Sound newMusicToPlay)
    {
        if(BGMSource.clip == newMusicToPlay.clip)
        {
            Debug.LogWarning("Playing the same music clip!!");
        }
        float t;

        //Fade out
        for (t = 0; t < newMusicToPlay.transitionTime; t += Time.deltaTime)
        {
            BGMSource.volume = 1 - (t / newMusicToPlay.transitionTime);
            yield return null;
        }
        BGMSource.Stop();
        BGMSource.clip = newMusicToPlay.clip;
        BGMSource.Play();

        //Fade in
        for (t = 0; t < newMusicToPlay.transitionTime; t += Time.deltaTime)
        {
            BGMSource.volume = t / newMusicToPlay.transitionTime;
            yield return null;
        }

    }
    #endregion

    public void PlayOneShotSFX(AudioClip clip, float volume)
    {
        // Play a sound effect (PlayOneShot aloows multiple sound effects to occur)
        sfxSource.PlayOneShot(clip, volume);
    }
}
