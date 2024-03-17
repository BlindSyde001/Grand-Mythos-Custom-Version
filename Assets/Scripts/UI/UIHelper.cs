using System.Collections;
using UnityEngine;

[AddComponentMenu(" GrandMythos/UI/UIHelper")]
public abstract class UIHelper : MonoBehaviour
{
    public void PlayAnimation(Animation comp) => comp.Play(PlayMode.StopAll);

    public void DestroyAfterAnimation(Animation comp)
    {
        StartCoroutine(DestroyAfterAnimation());

        IEnumerator DestroyAfterAnimation()
        {
            while (comp.isPlaying)
                yield return null;

            Destroy(gameObject);
        }
    }
}