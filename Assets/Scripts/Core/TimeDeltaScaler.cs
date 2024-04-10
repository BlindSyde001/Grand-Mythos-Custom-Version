using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public class TimeDeltaScaler : InputProcessor<float>
{
    public override float Process(float value, InputControl control)
    {
        return value * Time.deltaTime;
    }

#if UNITY_EDITOR
    static TimeDeltaScaler()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<TimeDeltaScaler>();
    }
}
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public class TimeDeltaScaler2D : InputProcessor<Vector2>
{
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        return value * Time.deltaTime;
    }

#if UNITY_EDITOR
    static TimeDeltaScaler2D()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<TimeDeltaScaler2D>();
    }
}