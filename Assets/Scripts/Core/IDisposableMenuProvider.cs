using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public interface IDisposableMenuProvider
{
    IDisposableMenu<T> NewMenuOf<T>(string seed);
}

public interface IDisposableMenu<T>
{
    Button NewButton(string label, T item, [MaybeNull] string onHover = null, bool interactable = true);
    Task<T> SelectedItem();
}