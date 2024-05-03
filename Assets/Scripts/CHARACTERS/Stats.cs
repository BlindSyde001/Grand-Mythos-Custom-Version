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

    [HorizontalGroup("MISC"), GUIColor(0.75f, 0.75f, 0.75f)]
    public int Speed;

    [HorizontalGroup("MISC"), GUIColor(0.75f, 0.75f, 0.75f)]
    public int Luck;

    public int this[Stat i]
    {
        get => i switch
        {
            Stat.Health => HP,
            Stat.Mana => MP,
            Stat.Attack => Attack,
            Stat.MagicAttack => MagAttack,
            Stat.Defense => Defense,
            Stat.MagicDefense => MagDefense,
            Stat.Speed => Speed,
            Stat.Luck => Luck,
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
        set
        {
            switch (i)
            {
                case Stat.Health: HP = value; return;
                case Stat.Mana: MP = value; return;
                case Stat.Attack: Attack = value; return;
                case Stat.MagicAttack: MagAttack = value; return;
                case Stat.Defense: Defense = value; return;
                case Stat.MagicDefense: MagDefense = value; return;
                case Stat.Speed: Speed = value; return;
                case Stat.Luck: Luck = value; return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(i), i, null);
            }
        }
    }

    public string ToStringOneStatPerLine()
    {
        return
            @$"HP: {HP}
MP: {MP}
Attack: {Attack}
Magic Attack: {MagAttack}
Defense: {Defense}
Magic Defense: {MagDefense}
Speed: {Speed}
Luck: {Luck}";
    }
}