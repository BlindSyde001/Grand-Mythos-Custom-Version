using UnityEngine.Events;
using UnityEngine.UI;

public class DamageText : UIHelper
{
    public required Graphic ElementColorTarget;
    public UnityEvent<string>? OnDamage;
    public UnityEvent<string>? OnHeal;
    public float Lifetime = 1f;
}