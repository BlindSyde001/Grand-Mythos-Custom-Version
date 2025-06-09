using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;


public interface IDisposableMenuProvider
{
    IDisposableMenu<T> NewMenuOf<T>(string seed);
}

public interface IDisposableMenu<T>
{
    Button NewButton(string label, T item, string? onHover = null, bool interactable = true);
    UniTask<T?> SelectedItem(CancellationToken cancellation);
}