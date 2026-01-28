using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class VideoAnimation : MonoBehaviour
{
    [SerializeField]
    public required VideoPlayer videoPlayer;
    [SerializeField]
    public required RawImage videoScreen;
    public VideoClip[] videoClips = Array.Empty<VideoClip>();

    internal float time;

    public async UniTask PlayVideoClip(VideoClip videoClip, CancellationToken cancellation)
    {
        time = (float)videoClip.length;
        videoScreen.enabled = true;
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
        await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: cancellation);
        videoScreen.DOFade(0, 1f);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellation);
        videoScreen.enabled = false;
    }
}
