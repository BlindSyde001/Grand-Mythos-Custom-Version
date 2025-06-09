using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace QTE
{
    [Serializable]
    public class Mashing : IQTE
    {
        public int HitRequired = 20;
        public async UniTask<QTEResult> Evaluate(QTEStart qte, Func<float> getT, QTEInterface qteInterface, CancellationToken cancellation)
        {
            float t;
            int counter = 0;
            qteInterface.InputProgress.fillAmount = 0f;
            qteInterface.Text.text = $"Mash {qte.Input.action.GetBindingLabel()}";
            do
            {
                t = getT();
                qteInterface.QTETimer.fillAmount = 1f - t;
                if (qte.Input.action.WasPressedThisFrame())
                {
                    counter++;
                    if (counter >= HitRequired)
                    {
                        return QTEResult.Success;
                    }
                }

                // ReSharper disable once PossibleLossOfFraction
                qteInterface.InputProgress.fillAmount = counter / HitRequired;

                await UniTask.Yield(cancellation);
            } while (t < 1f);

            return QTEResult.Failure;
        }
    }
}