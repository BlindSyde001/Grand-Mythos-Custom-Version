using UnityEngine;

[CreateAssetMenu]
public class Team : ScriptableObject
{
    public SerializableHashSet<Team> Allies = new();

    public Team() => Allies.Add(this);
}