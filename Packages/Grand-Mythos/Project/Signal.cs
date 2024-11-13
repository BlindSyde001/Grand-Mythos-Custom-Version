
public class Signal<T>
{
    public bool Signaled { get; private set; }
    public T Value { get; private set; }

    public void Set(T value)
    {
        Signaled = true;
        Value = value;
    }
}
