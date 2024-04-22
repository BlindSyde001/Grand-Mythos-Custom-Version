using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillTreeMenu : MenuContainer
{
    public UIElementList<Button> HeroSelections;
    [Required] public RectTransform HeroSelectionContainer;
    [Required] public InputActionReference SwitchHero;
    [Required] public TMP_Text PointsLeft;
    [Required] public InputActionReference PointInput;
    [Required] public InputActionReference StickInput;
    [Required] public Image DragArea;
    [ReadOnly, SerializeField] SkillTree _activeTree;
    HeroExtension _selectedHero;
    [ReadOnly, SerializeField] EventTrigger _trigger;
    bool _isDragging;
    Vector2 _lastMousePos;

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetHeroSelection();
        SwapSelection(GameManager.PartyLineup[0]);
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
        SwitchHero.action.performed += Switch;


        static EventTrigger.Entry NewCallback(EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var t = new EventTrigger.Entry { eventID = type, callback = new() };
            t.callback.AddListener(callback);
            return t;
        }
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        SwitchHero.action.performed -= Switch;

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
        if (_selectedHero == null)
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

        int pointsLeft = _selectedHero.SkillPointsTotal - _selectedHero.UnlockedTreeNodes.Count;
        if (int.TryParse(PointsLeft.text, out int result) == false || result != pointsLeft)
            PointsLeft.text = pointsLeft.ToString();
    }

    void SetHeroSelection()
    {
        HeroSelections.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelections.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.onClick.AddListener(delegate {SwapSelection(hero); });
        }
    }

    void Switch(InputAction.CallbackContext input)
    {
        int indexOf = GameManager.PartyLineup.IndexOf(_selectedHero);
        indexOf += input.ReadValue<float>() >= 0f ? 1 : -1;
        indexOf = indexOf < 0 ? GameManager.PartyLineup.Count + indexOf : indexOf % GameManager.PartyLineup.Count;

        SwapSelection(GameManager.PartyLineup[indexOf]);
    }

    public void SwapSelection(HeroExtension hero)
    {
        _selectedHero = hero;
        if (_activeTree != null)
        {
            Destroy(_activeTree.gameObject);
            _activeTree = null;
        }

        _activeTree = Instantiate(_selectedHero.SkillTree, this.transform);
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
        _activeTree.SelectedHero = _selectedHero;
    }
}