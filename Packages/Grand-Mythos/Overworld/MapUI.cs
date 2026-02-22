using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapUI : MonoBehaviourWithSceneGUI
{
    public required Image WholeMapCursor;
    public required RawImage WholeMapElement;
    public required RectTransform Minimap;
    public required RawImage MinimapMaskedElement;
    public required Image Cursor;
    public Bounds Bounds = new (default, new Vector3(1, 1, 1));
    public bool ShowWholeMap;
    public required InputActionReference OpenMap, CloseMap;

    private void Update()
    {
        Vector3 playerPosition = default;
        foreach (var controller in OverworldPlayerController.Instances)
        {
            playerPosition = controller.transform.position;
        }

        if (OpenMap.action.WasPerformedThisFrameUnique() || CloseMap.action.WasPerformedThisFrameUnique())
        {
            ShowWholeMap = !ShowWholeMap;
            if (ShowWholeMap)
                InputManager.PushGameState(GameState.Pause, this);
            else
                InputManager.PopGameState(this);
        }
        
        WholeMapCursor.gameObject.SetActive(ShowWholeMap);
        WholeMapElement.gameObject.SetActive(ShowWholeMap);
        Minimap.gameObject.SetActive(ShowWholeMap == false);
        
        Vector2 unitXY;
        unitXY.x = (playerPosition.x - Bounds.min.x) / (Bounds.max.x - Bounds.min.x);
        unitXY.y = (playerPosition.z - Bounds.min.z) / (Bounds.max.z - Bounds.min.z);

        if (ShowWholeMap)
        {
            var texXY = WholeMapElement.rectTransform.sizeDelta;
            WholeMapCursor.rectTransform.localPosition = unitXY * texXY - texXY / 2f;
        }
        else
        {
            var texXY = MinimapMaskedElement.rectTransform.sizeDelta;
            var centered = ((RectTransform)MinimapMaskedElement.rectTransform.parent).sizeDelta / 2f;
            MinimapMaskedElement.rectTransform.localPosition = -unitXY * texXY + centered;
        }

        if (Camera.main is {} c)
        {
            var dir = c.transform.forward;
            dir.y = 0;
            dir = Vector3.Normalize(dir);
            (ShowWholeMap ? WholeMapCursor : Cursor).rectTransform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(dir, -Vector3.forward, Vector3.up));
        }
    }

    protected override void DuringSceneGui(SceneGUIProxy sceneGUI)
    {
        using (sceneGUI.AutoUndo(this, "Bounds"))
        {
            Bounds = sceneGUI.Bounds(Bounds);
        }
    }
}
