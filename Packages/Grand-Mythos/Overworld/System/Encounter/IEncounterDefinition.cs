using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IEncounterDefinition
{
    UniTask Start(CancellationToken cts);
    bool IsValid([MaybeNullWhen(true)] out string error);
}