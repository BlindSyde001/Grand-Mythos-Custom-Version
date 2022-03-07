using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Armour", menuName = "Armour")]
public class Armour : Equipment
{
    public enum ArmourType { Leather, Mail, Chasis, Robes};

    [BoxGroup("BASIC DATA")]
    [PropertyOrder(0)]
    public ArmourType armourType;
}
