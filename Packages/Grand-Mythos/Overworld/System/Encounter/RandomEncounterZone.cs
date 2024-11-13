using Sirenix.OdinInspector;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[AddComponentMenu(" GrandMythos/RandomEncounterZone")]
public class RandomEncounterZone : MonoBehaviour
{
    [PropertyTooltip("Amount of meters to traverse within the collision before an encounter occurs, random between the min and max value")]
    public Range MetersPerEncounter = new Range(10, 20);
    public Random Random = new Random(1);
    [SerializeReference] public IEncounterDefinition Encounter;

    double lastMetersTraversed;
    double metersLeftToTraverse;

    void Awake()
    {
        metersLeftToTraverse = Random.NextFloat(MetersPerEncounter.Min, MetersPerEncounter.Max);
    }

    void OnDrawGizmos()
    {
        if (Encounter == null)
            GizmosHelper.Label(transform.position, $"No {nameof(Encounter)} set on this {nameof(RandomEncounterZone)}", Color.red);
        else if (Encounter.IsValid(out string error) == false)
            GizmosHelper.Label(transform.position, error, Color.red);
        else if (Random.state == 0)
            GizmosHelper.Label(transform.position, $"{nameof(Random)} with a state of zero is not allowed", Color.red);
        else if (GetComponent<Collider>() is Collider c && c != null)
        {
            if (c.isTrigger == false)
                GizmosHelper.Label(transform.position, "Set this collider to trigger", Color.red);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
        }
        else
            GizmosHelper.Label(transform.position, $"Add a collider to this {nameof(RandomEncounterZone)}", Color.red);


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != OverworldPlayerController.CharacterLayer)
            return;

        if (other.GetComponentInParent<OverworldPlayerController>() is not OverworldPlayerController controller)
            return;
        
        lastMetersTraversed = controller.UnitsWalked;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != OverworldPlayerController.CharacterLayer)
            return;

        if (other.GetComponentInParent<OverworldPlayerController>() is not OverworldPlayerController controller)
            return;

        double walkDelta = controller.UnitsWalked - lastMetersTraversed;
        lastMetersTraversed = controller.UnitsWalked;
        metersLeftToTraverse -= walkDelta;
        if (metersLeftToTraverse <= 0)
        {
            metersLeftToTraverse += Random.NextFloat(MetersPerEncounter.Min, MetersPerEncounter.Max);
            Encounter.Start(controller);
        }
    }
}