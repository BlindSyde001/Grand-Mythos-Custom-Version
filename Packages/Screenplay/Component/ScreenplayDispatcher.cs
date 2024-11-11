using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Component
{
    public class ScreenplayDispatcher : MonoBehaviour
    {
        private IEnumerator<object?>? _existing;

        [Required] public ScreenplayGraph Screenplay = null!;

        private void OnEnable()
        {
            if (transform.parent != null)
                transform.parent = null;

            DontDestroyOnLoad(gameObject);
            StartCoroutine(_existing = Screenplay.StartExecution());
        }

        private void OnDisable()
        {
            _existing?.Dispose();
            _existing = null;
        }

        private void OnDrawGizmos()
        {
            if (Screenplay == null)
                return;

            ScreenplayGizmos.Draw(Screenplay);
        }
    }
}
