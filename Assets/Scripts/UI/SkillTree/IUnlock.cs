public interface IUnlock
{
    public void OnUnlock(HeroExtension hero, guid guid);
    public void OnLock(HeroExtension hero, guid guid);
}