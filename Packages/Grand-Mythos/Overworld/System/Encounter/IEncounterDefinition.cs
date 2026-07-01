using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Screenplay;

public interface IEncounterDefinition
{
    UniTask Start(Cancellation cts);
    bool IsValid([MaybeNullWhen(true)] out string error);
}