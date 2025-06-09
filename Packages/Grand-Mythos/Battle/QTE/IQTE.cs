using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace QTE
{
    public interface IQTE
    {
        UniTask<QTEResult> Evaluate(QTEStart qte, Func<float> getT, QTEInterface qteUI, CancellationToken cancellation);
    }
}