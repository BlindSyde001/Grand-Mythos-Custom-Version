using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCamera : MonoBehaviour
{
    [Required] public Camera Camera;
    [Required] public BattleUIOperation BattleController;
    [Required] public InputActionReference RotationInput;
    [InfoBox("The position of the pivot local to the center of the unit's bounding box")]
    public Vector3 Pivot = new Vector3(0.5f, 0.75f, 0f);
    public float Distance = 3f;
    public float MinimumAngle = -90f, MaximumAngle = 90f;
    Vector3 _euler;

    void Update()
    {
        if (BattleController.UnitSelected == null || BattleController.BattleManagement.enabled == false)
            return;

        // Prefer skinned mesh renderer if there are multiple renderers
        var center = BattleController.UnitSelected.transform.position;
        var pivotReference = BattleController.UnitSelected.transform.rotation;
        var renderer = BattleController.UnitSelected.GetComponentInChildren<SkinnedMeshRenderer>();
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

        var input = RotationInput.action.ReadValue<Vector2>();
        _euler += new Vector3(input.x, -input.y, 0);
        _euler.y = Mathf.Clamp(_euler.y, MinimumAngle, MaximumAngle);

        var cameraRotation = Quaternion.Euler(_euler.y, _euler.x, 0);
        Camera.transform.SetPositionAndRotation(center + cameraRotation * Vector3.back * Distance, cameraRotation);
    }

    void OnValidate()
    {
        if (Camera == null)
            Camera = Camera.main;
    }
}