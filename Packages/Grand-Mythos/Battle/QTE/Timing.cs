using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace QTE
{
    [Serializable]
    public class Timing : IQTE
    {
        public async UniTask<QTEResult> Evaluate(QTEStart qte, Func<float> getT, QTEInterface qteInterface, CancellationToken cancellation)
        {
            float t;
            qteInterface.InputProgress.fillAmount = 0f;
            qteInterface.Text.text = $"Press {qte.Input.action.GetBindingLabel()}";
            do
            {
                t = getT();
                qteInterface.QTETimer.fillAmount = 1f - t;
                if (qte.Input.action.WasPressedThisFrame())
                    return QTEResult.Success;
                await UniTask.Yield(cancellation);
            } while (t < 1f);

            return QTEResult.Failure;
        }
    }
}