using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class VideoAnimation : MonoBehaviour
{
    [SerializeField]
    internal VideoPlayer videoPlayer;
    [SerializeField]
    internal RawImage videoScreen;
    public VideoClip[] videoClips;

    internal float time;

    public IEnumerator PlayVideoClip(VideoClip videoClip)
    {
        time = (float)videoClip.length;
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
        yield return new WaitForSeconds(time);
        videoScreen.DOFade(0, 1f);
    }
}
