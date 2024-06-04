using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillTreeMenu : MenuContainerWithHeroSelection
{
    [Required] public RectTransform HeroSelectionContainer;
    [Required] public TMP_Text PointsLeft;
    [Required] public InputActionReference PointInput;
    [Required] public InputActionReference StickInput;
    [Required] public Image DragArea;
    [ReadOnly, SerializeField] SkillTree _activeTree;
    [ReadOnly, SerializeField] EventTrigger _trigger;
    bool _isDragging;
    Vector2 _lastMousePos;

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (var yields in base.Open(menuInputs))
        {
            yield return yields;
        }

        gameObject.SetActive(true);

        if (_trigger == null)
        {
            _trigger = DragArea.gameObject.AddComponent<EventTrigger>();
            _trigger.triggers.Add(NewCallback(EventTriggerType.EndDrag, e => _isDragging = false));
            _trigger.triggers.Add(NewCallback(EventTriggerType.BeginDrag, e =>
            {
                _isDragging = true;
                _lastMousePos = PointInput.action.ReadValue<Vector2>();
            }));
        }

        if (gameObject.TryGetComponent<CanvasGroup>(out var canvas) == false)
            canvas = gameObject.AddComponent<CanvasGroup>();
        canvas.alpha = 0f;
        canvas.DOFade(1f, menuInputs.Speed);

        HeroSelectionContainer.DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);

        static EventTrigger.Entry NewCallback(EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var t = new EventTrigger.Entry { eventID = type, callback = new() };
            t.callback.AddListener(callback);
            return t;
        }
    }

    protected override void OnSelectedHeroChanged()
    {
        if (_activeTree != null)
        {
            Destroy(_activeTree.gameObject);
            _activeTree = null;
        }

        _activeTree = Instantiate(SelectedHero.SkillTree, this.transform);
        Destroy(_activeTree.GetComponent<CanvasRenderer>());
        Destroy(_activeTree.GetComponent<CanvasScaler>());
        Destroy(_activeTree.GetComponent<GraphicRaycaster>());
        Destroy(_activeTree.GetComponent<Canvas>());
        var rect = _activeTree.gameObject.GetComponent<RectTransform>();
        rect.anchorMin = default;
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = default;
        rect.offsetMax = default;
        rect.localScale = Vector3.one;
        _activeTree.transform.SetSiblingIndex(DragArea.transform.GetSiblingIndex() + 1);
        _activeTree.SelectedHero = SelectedHero;
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (gameObject.TryGetComponent<CanvasGroup>(out var canvas) == false)
            canvas = gameObject.AddComponent<CanvasGroup>();
        canvas.alpha = 1f;
        canvas.DOFade(0f, menuInputs.Speed);

        HeroSelectionContainer.DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
        if (_activeTree != null)
        {
            Destroy(_activeTree.gameObject);
            _activeTree = null;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (SelectedHero == null)
            return;

        if (_isDragging)
        {
            var currentMousePos = PointInput.action.ReadValue<Vector2>();

            Vector2 v = currentMousePos - _lastMousePos;
            _activeTree.transform.position += (Vector3)v;
            _lastMousePos = currentMousePos;
        }

        if (StickInput.action.IsPressed())
        {
            _activeTree.transform.position += (Vector3)StickInput.action.ReadValue<Vector2>();
        }

        var selection = EventSystem.current.currentSelectedGameObject;
        if (selection != null && selection.GetComponentInParent<SkillTree>() == _activeTree)
        {
            var worldCenter = transform.TransformPoint(((RectTransform)this.transform).rect.center);
            var selectionCenter = selection.transform.TransformPoint(((RectTransform)selection.transform).rect.center);
            _activeTree.transform.position += worldCenter - selectionCenter;
        }

        uint pointsLeft = SelectedHero.SkillPointsTotal - (uint)SelectedHero.UnlockedTreeNodes.Count;
        if (uint.TryParse(PointsLeft.text, out uint result) == false || result != pointsLeft)
            PointsLeft.text = pointsLeft.ToString();
    }
}