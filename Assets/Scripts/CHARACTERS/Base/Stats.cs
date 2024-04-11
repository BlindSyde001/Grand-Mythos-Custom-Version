using System;
using Sirenix.OdinInspector;

[Serializable]
public struct Stats
{
    [HorizontalGroup("POINTS"), GUIColor(0.5f, 1f, 0.5f)]
    public int HP;

    [HorizontalGroup("POINTS"), GUIColor(0.5f, 0.5f, 0.9f)]
    public int MP;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public int Attack;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public int MagAttack;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public int Defense;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public int MagDefense;

    public int Speed;

    public string ToStringOneStatPerLine()
    {
        return
            @$"HP: {HP}
MP: {MP}
Attack: {Attack}
Magic Attack: {MagAttack}
Defense: {Defense}
Magic Defense: {MagDefense}
Speed: {Speed}";
    }
}