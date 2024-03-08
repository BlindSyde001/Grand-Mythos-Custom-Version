using UnityEngine.Events;

public class DamageText : UIHelper
{
    public UnityEvent<string> OnDamage;
    public UnityEvent<string> OnHeal;
    public float Lifetime = 1f;
}