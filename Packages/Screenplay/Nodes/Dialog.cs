using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    public class Dialog : Action, ILocalizableNode
    {
        [HideLabel]
        public Interlocutor? Interlocutor;

        [InlineProperty, HideLabel]
        public LocalizableText Line = new("Dialog Line\n\nAnother Line");

        IEnumerable<string> Lines()
        {
            foreach (string s in Line.Content.Split("\n\n"))
            {
                var trimmed = s.Trim();
                if (trimmed.Length == 0)
                    continue;
                yield return trimmed;
            }
        }

        public IEnumerable<LocalizableText> GetTextInstances()
        {
            yield return Line;
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public override void FastForward(IContext context) { }

        public override IEnumerable<Signal> Execute(IContext context) => RunDialog(context, false);

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (fastForwarded == false)
                previewer.PlayCustomSignal(RunDialog(previewer, true));

            if (previewer.GetDialogUI() is {} ui && Lines().LastOrDefault() is {} lastLine)
            {
                ui.StartDialogPresentation();
                ui.StartLineTypewriting(lastLine);
                ui.SetTypewritingCharacter(lastLine.Length);
                ui.FinishedTypewriting();
                ui.EndDialogPresentation();
            }
        }

        private IEnumerable<Signal> RunDialog(IContext context, bool previewMode)
        {
            if (context.GetDialogUI() is {} ui == false)
            {
                Debug.LogWarning($"{nameof(ScreenplayGraph.DialogUIPrefab)} has not been set, no interface to present this {nameof(Dialog)} on");
                yield return Signal.BreakInto(Next);
                yield break;
            }

            ui.StartDialogPresentation();
            foreach (var text in Lines())
            {
                ui.StartLineTypewriting(text);
                ui.SetTypewritingCharacter(0);
                float time = 0f;
                int lastChatter = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    ui.SetTypewritingCharacter(i + 1);

                    if (i + 1 == text.Length)
                        break; // Don't delay for the last character

                    if (Interlocutor != null && i - lastChatter >= Interlocutor.CharactersPerChatter)
                        Chatter(ref lastChatter, i, text, Interlocutor, ui);

                    time += Interlocutor?.GetDuration(text[i]) ?? 0.1f;
                    for (; time > 0f; time -= Time.unscaledDeltaTime)
                    {
                        if (ui.FastForwardRequested)
                        {
                            yield return Signal.NextFrame;
                            goto BREAK_TYPEWRITING;
                        }

                        yield return Signal.NextFrame;
                    }
                }

                if (Interlocutor != null)
                    Chatter(ref lastChatter, text.Length - 1, text, Interlocutor, ui);

                BREAK_TYPEWRITING:
                ui.SetTypewritingCharacter(text.Length);
                ui.FinishedTypewriting();

                while (previewMode || ui.DialogAdvancesAutomatically == false)
                {
                    if (ui.FastForwardRequested)
                    {
                        yield return Signal.NextFrame;
                        break;
                    }

                    yield return Signal.NextFrame;
                }
            }
            ui.EndDialogPresentation();

            yield return Signal.BreakInto(Next);
        }

        private void Chatter(ref int last, int current, string text, Interlocutor interlocutor, Component.UIBase ui)
        {
            int hash = 0;
            int processed = 0;
            for (; last <= current; last++)
            {
                if (interlocutor.GetDuration(text[last]) == 0f)
                {
                    for (; last <= current && interlocutor.GetDuration(text[last]) == 0f; last++) { }
                    break;
                }

                hash = HashCode.Combine(hash, text[last]);
                processed++;
            }

            if (interlocutor.Chatter.Length == 0 || processed == 0)
                return;

            var index = hash % interlocutor.Chatter.Length;
            index = index < 0 ? interlocutor.Chatter.Length + index : index;
            var chatter = interlocutor.Chatter[index];
            ui.PlayChatter(chatter, interlocutor);
        }
    }
}
