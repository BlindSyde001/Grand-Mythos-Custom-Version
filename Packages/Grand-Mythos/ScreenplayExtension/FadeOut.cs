using System;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;
using UnityEngine.UI;
using YNode;

namespace Rogue.ScreenplayExtensions
{
    [Serializable, NodeVisuals(Icon = "d_PreMatQuad")]
    public class FadeOut : ExecutableLinear
    {
        [SerializeReference, Input] public FadeIn? Input;
        public float Duration = 1.5f;

        public override void CollectReferences(ReferenceCollector references){ }

        protected override async UniTask LinearExecution(IEventContext context, Cancellation cancellation)
        {
            var img = FetchExisting(context);
            try
            {
                var color = img.color;

                for (float f = 0; f < Duration; f += Time.deltaTime)
                {
                    var t = Mathf.SmoothStep(1, 0, f / Duration);
                    img.color = color * new Color(1, 1, 1, t);
                    await Uni.NextFrame(cancellation, cancelImmediately:true);
                }
            }
            finally
            {
                Discard(img);
                InputManager.PopGameState(context.Source);
            }
        }

        public override UniTask Persistence(IEventContext context, Cancellation cancellation)
        {
            FastForward(context);
            return UniTask.CompletedTask;
        }

        private void FastForward(IEventContext context)
        {
            if (FadeIn.Instances.Remove(context, out var img))
                Discard(img);
            InputManager.PopGameState(context.Source);
        }

        private RawImage FetchExisting(IEventContext context)
        {
            return FadeIn.Instances.Remove(context, out var img) ? img : Input.CreateUI();
        }

        private void Discard(RawImage img)
        {
            img.gameObject.ForceDestroy();
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            previewer.RegisterRollback(() =>
            {
                if (FadeIn.Instances.Remove(previewer, out var img))
                    Discard(img);
            });
            if (fastForwarded)
                FastForward(previewer);
            else
                previewer.PlaySafeAction(this);
        }
    }
}
