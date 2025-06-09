using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace QTE
{
    [Serializable]
    public class Holding : IQTE
    {
        [Tooltip("The fraction of time the user should hold compared to the total time the QTE is on screen")]
        [Range(0f, 1f)] public float Ratio = 0.5f;
        public async UniTask<QTEResult> Evaluate(QTEStart qte, Func<float> getT, QTEInterface qteInterface, CancellationToken cancellation)
        {
            float t;
            float start = 2f;
            qteInterface.InputProgress.fillAmount = 0f;
            qteInterface.Text.text = $"Hold {qte.Input.action.GetBindingLabel()}";
            do
            {
                t = getT();
                qteInterface.QTETimer.fillAmount = 1f - t;
                if (qte.Input.action.WasPressedThisFrame())
                    start = t;
                if (qte.Input.action.WasReleasedThisFrame())
                    start = 2f;
                qteInterface.InputProgress.fillAmount = MathF.Max((t - start) / Ratio, 0f);

                if (t - start > Ratio)
                {
                    return QTEResult.Success;
                }

                await UniTask.Yield(cancellation);
            } while (t < 1f);

            return QTEResult.Failure;
        }
    }
}