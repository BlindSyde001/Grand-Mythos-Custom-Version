#nullable enable

using System;

namespace Nodalog
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    [UnitCategory("Nodalog")]
    [UnitTitle("Line")]
    [TypeIcon(typeof(_DummyIconClass))]
    public class NodeLine : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput Enter { get; private set; } = null!;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput Exit { get; private set; } = null!;

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public DialogIssues Issues = new();

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public Interlocutor? Interlocutor;

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public LineData Line = new();

        protected override void Definition()
        {
            Enter = ControlInputCoroutine(nameof(Enter), DialogFlow);
            Exit = ControlOutput(nameof(Exit));
            Succession(Enter, Exit);
        }

        // Workaround to ensure errors are shown and updated whenever connections changes
        [DoNotSerialize]
        public override bool isControlRoot
        {
            get
            {
#if UNITY_EDITOR
                ValidateConnections();
#endif
                return false;
            }
            protected set
            {

            }
        }

        protected IEnumerator DialogFlow(Flow flow)
        {
            NodeStart.Starts.TryGetValue(flow, out var comp);
            var ui = comp.ui;
            var text = Line.RawString;
            ui.StartLineTypewriting(text);
            ui.SetTypewritingCharacter(0);
            float time = 0f;
            int lastChatter = 0;
            for (int i = 0; i < text.Length; i++)
            {
                ui.SetTypewritingCharacter(i+1);

                if (i + 1 == text.Length)
                    break; // Don't delay for the last character

                if (Interlocutor != null && i - lastChatter >= Interlocutor.CharactersPerChatter)
                    Chatter(ref lastChatter, i, text, Interlocutor, ui);

                time += Interlocutor?.GetDuration(text[i]) ?? 0.1f;
                for (; time > 0f; time -= Time.unscaledDeltaTime)
                {
                    if (ui.FastForwardRequested)
                    {
                        yield return null;
                        goto BREAK_TYPEWRITING;
                    }

                    yield return null;
                }
            }

            if (Interlocutor != null)
                Chatter(ref lastChatter, text.Length - 1, text, Interlocutor, ui);

            BREAK_TYPEWRITING:
            ui.SetTypewritingCharacter(text.Length);
            ui.FinishedTypewriting();

            while (ui.DialogAdvancesAutomatically == false)
            {
                if (ui.FastForwardRequested)
                {
                    yield return null;
                    break;
                }

                yield return null;
            }

            yield return Exit;
        }

        void Chatter(ref int last, int current, string text, Interlocutor interlocutor, UIBase ui)
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

        void ValidateConnections()
        {
            Issues.Issues.Clear();
            CheckStart();
            CheckEnd();

            void CheckStart()
            {
                // Check for dialog start
                var toCheck = new Queue<Unit>();
                var checkedUnits = new HashSet<Unit>(); // To ensure we don't end up in an infinite loop
                toCheck.Enqueue(this);

                while (toCheck.TryDequeue(out var unit))
                {
                    foreach (var port in unit.controlInputs)
                    {
                        int thisPortConnectionCount = 0;
                        foreach (var connection in port.validConnections)
                        {
                            thisPortConnectionCount++;
                            if (connection.source.unit is NodeStart)
                                continue;
                            if (connection.source.unit is Unit u && checkedUnits.Add(u))
                                toCheck.Enqueue(u);
                        }

                        if (thisPortConnectionCount == 0)
                        {
                            Issues.Issues.Add(new(){ Text = $"Requires a {nameof(NodeStart)} set before hand", Type = DialogIssues.Type.Error});
                        }
                    }
                }
            }

            void CheckEnd()
            {
                // Check for dialog end
                var toCheck = new Queue<Unit>();
                var checkedUnits = new HashSet<Unit>(); // To ensure we don't end up in an infinite loop
                toCheck.Enqueue(this);

                while (toCheck.TryDequeue(out var unit))
                {
                    foreach (var port in unit.controlOutputs)
                    {
                        int thisPortConnectionCount = 0;
                        foreach (var connection in port.validConnections)
                        {
                            thisPortConnectionCount++;
                            if (connection.destination.unit is NodeEnd)
                                continue;

                            if (connection.destination.unit is Unit u && checkedUnits.Add(u))
                                toCheck.Enqueue(u);
                        }

                        if (thisPortConnectionCount == 0)
                        {
                            Issues.Issues.Add(new(){ Text = $"Requires a {nameof(NodeEnd)} set before reaching the end of this branch", Type = DialogIssues.Type.Error});
                        }
                    }
                }
            }
        }
    }
}