using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCamera : MonoBehaviour
{
    [Required] public Camera Camera;
    [Required] public BattleUIOperation BattleController;
    [Required] public InputActionReference RotationInput;
    public float DistanceMultiplier = 2.5f;
    public float MinimumAngle = -90f, MaximumAngle = 90f;
    public float InterpolationFactor = 25f;
    Vector3 _euler = new(45, 0, 0);

    void Update()
    {
        if (BattleController.UnitSelected == null || BattleController.BattleManagement.enabled == false)
            return;

        Vector3 aggregatePosition = default;
        foreach (var unit in BattleController.BattleManagement.Units)
            aggregatePosition += unit.transform.position;

        aggregatePosition /= BattleController.BattleManagement.Units.Count;

        float edge = 0f;
        foreach (var unit in BattleController.BattleManagement.Units)
            edge = Mathf.Max((aggregatePosition - unit.transform.position).sqrMagnitude, edge);

        var input = RotationInput.action.ReadValue<Vector2>();
        _euler += new Vector3(-input.y, input.x, 0);
        _euler.x = Mathf.Clamp(_euler.x, MinimumAngle, MaximumAngle);

        var cameraRotation = Quaternion.Euler(_euler.x, _euler.y, 0);
        cameraRotation = Quaternion.Lerp(cameraRotation, Camera.transform.rotation, Mathf.Exp(-InterpolationFactor * Time.deltaTime));
        Camera.transform.SetPositionAndRotation(aggregatePosition + cameraRotation * Vector3.back * Mathf.Sqrt(edge) * DistanceMultiplier, cameraRotation);
    }

    static void OverTheShoulder(BattleCharacterController character, Vector3 Pivot, Vector3 euler, float Distance, Camera Camera)
    {
        // Prefer skinned mesh renderer if there are multiple renderers
        var center = character.transform.position;
        var pivotReference = character.transform.rotation;
        var renderer = character.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer)
        {
            var localBounds = renderer.localBounds;
            var worldToLocal = renderer.rootBone.worldToLocalMatrix;
            var localToWorld = renderer.rootBone.localToWorldMatrix;
            // Compute the pivot from this transform
            var worldPivot = pivotReference * Pivot;
            // Scale it by the local bounds' extents, we must convert it into the same space as the bounds' for this
            worldPivot = worldToLocal.rotation * worldPivot;
            worldPivot = Vector3.Scale(worldPivot, localBounds.extents);
            worldPivot = localToWorld.MultiplyPoint(worldPivot);
            worldPivot -= localToWorld.MultiplyPoint(default); // Remove any translation, maybe better to just do one MultiplyVector instead of this
            center = renderer.bounds.center + worldPivot;
        }

        var cameraRotation = Quaternion.Euler(euler.y, euler.x, 0);
        Camera.transform.SetPositionAndRotation(center + cameraRotation * Vector3.back * Distance, cameraRotation);
    }

    void OnValidate()
    {
        if (Camera == null)
            Camera = Camera.main;
    }
}