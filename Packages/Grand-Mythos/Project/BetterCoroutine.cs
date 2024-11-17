
using System;
using System.Collections;
using UnityEngine;

public class BetterCoroutine
{
    private MonoBehaviour _host;
    private Coroutine _coroutine;
    private IEnumerable _logic;

    public bool Done { get; private set; }

    public static BetterCoroutine Empty = new BetterCoroutine();

    private BetterCoroutine()
    {
        Done = true;
    }

    public BetterCoroutine(MonoBehaviour host, IEnumerable logic)
    {
        _host = host;
        _logic = logic;
        _coroutine = _host.StartCoroutine(Inner());
    }

    public void Stop()
    {
        if (Done)
            return;

        _host.StopCoroutine(_coroutine);
        Debug.Assert(Done);
    }

    IEnumerator Inner()
    {
        IEnumerator enumerator = null;
        try
        {
            for (enumerator = _logic.GetEnumerator(); ; )
            {
                object yield;
                try
                {
                    if (enumerator.MoveNext() == false)
                        break;
                    yield = enumerator.Current;
                }
                catch(Exception e)
                {
                    Done = true;
                    Debug.LogException(e);
                    yield break;
                }

                yield return yield;
            }

            Done = true;
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
            if (Done == false)
            {
                Done = true;
            }
        }
    }
}
