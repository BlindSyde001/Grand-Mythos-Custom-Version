using UnityEngine;

/// <summary>
/// Handles rebuilding un-serialized state back after a domain reload, see <see cref="OnEnabled"/>.
/// </summary>
public abstract class ReloadableBehaviour : MonoBehaviour
{
    static DomainReloadHelper _reloadHelper;
    static SerializableHashSet<ReloadableBehaviour> _compLeftToReload = new();

    protected void OnEnable()
    {
        bool isRightAfterDomainReload = _compLeftToReload.Remove(this);
        OnEnabled(isRightAfterDomainReload);
    }

    protected void OnDisable()
    {
        if (_reloadHelper)
        {
            _reloadHelper.ReloadableBehaviours.Add(this);
            OnDisabled(true);
        }
        OnDisabled(false);
    }

    /// <summary>
    /// Same as <see cref="OnEnable"/>>, so it can also be called after the domain is reloaded in play mode,
    /// when saving a script while a game is running in the editor for example.
    /// The parameter should be used when you need to re-initialize stuff that are not serialized, like private or static fields since
    /// a domain only keeps serialized data.
    /// </summary>
    protected abstract void OnEnabled(bool afterDomainReload);

    /// <summary>
    /// Same as <see cref="OnDisable"/>>, so it can also be called after the domain is reloaded in play mode,
    /// when saving a script while a game is running in the editor for example.
    /// The parameter should be used when you need to re-initialize stuff that are not serialized, like private or static fields since
    /// a domain only keeps serialized data.
    /// </summary>
    protected abstract void OnDisabled(bool beforeDomainReload);

    static ReloadableBehaviour()
    {
        DomainReloadHelper.BeforeReload += obj => _compLeftToReload = obj.ReloadableBehaviours;
        DomainReloadHelper.AfterReload += obj => _reloadHelper = obj;
    }
}

public partial class DomainReloadHelper
{
    public SerializableHashSet<ReloadableBehaviour> ReloadableBehaviours = new();
}
