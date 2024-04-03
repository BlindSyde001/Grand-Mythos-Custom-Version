using UnityEngine;
using UnityEngine.Events;

public class BattleTooltipUI : MonoBehaviour
{
    public UnityEvent<string> OnPresentNewTooltip;
    public UnityEvent OnHideTooltip;
}