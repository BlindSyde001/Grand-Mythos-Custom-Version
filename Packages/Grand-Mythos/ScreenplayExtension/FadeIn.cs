using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;
using UnityEngine.UI;
using YNode;


[Serializable, NodeVisuals(Icon = "d_Preset.Current")]
public class FadeIn : ExecutableLinear
{
    public static Dictionary<IEventContext, RawImage> Instances = new();

    public Color Color = new(0,0,0,1);
    public SortingMode Sorting = SortingMode.InFrontOfUI;
    public float Duration = 1.5f;

    public override void CollectReferences(ReferenceCollector references){ }

    protected override async UniTask LinearExecution(IEventContext context, Cancellation cancellation)
    {
        DiscardExisting(context);

        var img = CreateUI();
        Instances[context] = img;

        for (float f = 0; f < Duration; f += Time.deltaTime)
        {
            var t = Mathf.SmoothStep(0, 1, f / Duration);
            img.color = Color * new Color(1, 1, 1, t);
            await Uni.NextFrame(cancellation, cancelImmediately:true);
        }

        img.color = Color;
    }

    public override UniTask Persistence(IEventContext context, Cancellation cancellation)
    {
        FastForward(context);
        return UniTask.CompletedTask;
    }

    private void FastForward(IEventContext context)
    {
        DiscardExisting(context);

        var img = CreateUI();
        Instances[context] = img;
        img.color = Color;
        InputManager.PushGameState(GameState.Cutscene, context.Source);
    }

    public RawImage CreateUI()
    {
        var fade = new GameObject("Fade");
        var canvas = fade.AddComponent<Canvas>();
        fade.AddComponent<CanvasScaler>();
        fade.AddComponent<GraphicRaycaster>();
        fade.AddComponent<CanvasRenderer>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = Sorting == SortingMode.BehindUI ? short.MinValue : short.MaxValue;
        var img = fade.AddComponent<RawImage>();
        img.color = Color;
        return img;
    }

    void DiscardExisting(IEventContext context)
    {
        if (Instances.TryGetValue(context, out var img))
        {
            img.gameObject.ForceDestroy();
        }
    }

    public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        previewer.RegisterRollback(() => DiscardExisting(previewer));
        if (fastForwarded)
            FastForward(previewer);
        else
            previewer.PlaySafeAction(this);
    }

    public enum SortingMode
    {
        InFrontOfUI,
        BehindUI
    }
}