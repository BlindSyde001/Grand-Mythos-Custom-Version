using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Conditions;
using Effects;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleUIOperation : MonoBehaviour
{
    const bool MultiSelection = false;

    public BattleStateMachine BattleManagement;

    [Header("Selected Character")]
    [Required] public InputActionReference SwitchSpecialInput;
    [Required] public InputActionReference SwitchCharacter;
    [Required] public HeroPrefabUIData SelectedUI;
    [ReadOnly, CanBeNull] public BattleCharacterController UnitSelected;

    [Header("UI Info")]
    public List<HeroPrefabUIData> HeroUIData;
    [ReadOnly, SerializeField] public List<HeroExtension> HeroData;

    [Required] public GameObject EnemyUIPrefab;
    [ReadOnly, SerializeField] List<CharacterTemplate> EnemyData;
    [ReadOnly, SerializeField] List<EnemyPrefabUIData> EnemyUIData;

    [Required] public BattleTooltipUI TooltipUI;

    [Header("Modifier Display")]
    [Required] public RectTransform ModifierContainer;
    [ReadOnly] public Dictionary<IModifier, ModifierDisplay> ModifierDisplays = new();

    [Header("Action Selection")]
    [Required] public RectTransform ActionSelectionContainer;
    [Required] public Button Attack;
    [Required] public Button Skills;
    [Required] public Button Items;
    [Required] public Button Tactics;
    [Required] public Button Schedule;
    [Required] public Button Discard;

    [Header("Sub-Action Selection")]
    [Required] public InputActionReference CancelInput;
    [FormerlySerializedAs("SelectionContainer"), Required] public RectTransform SubActionSelectionContainer;
    [Required] public Button AcceptSelection;
    [ValidateInput(nameof(HasButton), "Must have a button")] public RectTransform ItemTemplate;
    [ValidateInput(nameof(HasButton), "Must have a button")] public RectTransform SkillTemplate;
    public TargetList Targets = new();

    [Header("Previews")]
    [Required] public UIActionPreview ActionPreviewTemplate;
    [Required] public GameObject TargetCursorTemplate;

    [Required] public DamageText DamageTextTemplate;

    public SerializableHashSet<BattleCharacterController> ProcessedUnits = new();

    (Coroutine coroutine, IDisposable disposable)? _runningUIOperation;
    bool _listenerBound;
    Color _initialTacticsColor, _disabledTacticsColor;

    List<GameObject> _targetCursors = new();
    Queue<(float availableAfter, DamageText component)> _damageTextCache = new();
    Tactics _order = new();

    (IAction action, UIActionPreview ui, PreviewType type)[] _existingPreviews = Array.Empty<(IAction, UIActionPreview, PreviewType)>();
    PreviewType _previewType;
    GroupSelection _groupSelection;
    TargetSelection _targetSelection;
    IAction _lastActionCursor;
    BattleCharacterController _waitingForScheduledUnit;

    public BattleUIOperation()
    {
        _groupSelection = new(this);
        _targetSelection = new(this);
    }

    bool HasButton(RectTransform val, ref string errorMessage)
    {
        return val != null &&  val.GetComponentInChildren<Button>();
    }

    void OnEnable()
    {
        ResetNavigation();
        if (_listenerBound == false)
        {
            _listenerBound = true;
            _initialTacticsColor = Tactics.colors.normalColor;
            _disabledTacticsColor = Tactics.colors.disabledColor;
            UpdateTacticsButtonColor();
            Attack.onClick.AddListener(() => TryOrderWizard(PresentAttackUI()));
            Skills.onClick.AddListener(() => TryOrderWizard(PresentSkillsUI()));
            Items.onClick.AddListener(() => TryOrderWizard(PresentItemUI()));
            Tactics.onClick.AddListener(TacticsPressed);

            Schedule.onClick.AddListener(() => ScheduleOrder(_order));
            Discard.onClick.AddListener(CancelFullOrder);

            for (int i = ModifierContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(ModifierContainer.transform.GetChild(i).gameObject);
        }

        ActionPreviewTemplate.gameObject.SetActive(false);
        AttributeAdd.OnApplied += DamageHandler;
    }

    void OnDisable()
    {
        AttributeAdd.OnApplied -= DamageHandler;
    }

    void DamageHandler(BattleCharacterController target, int initialAttributeValue, ComputableDamageScaling computeDamage)
    {
        if (computeDamage.Attribute != Attribute.Health)
            return;

        DamageText damageText;
        if (_damageTextCache.TryPeek(out var data) && Time.time >= data.availableAfter)
            damageText = _damageTextCache.Dequeue().component;
        else
            damageText = Instantiate(DamageTextTemplate.gameObject).GetComponent<DamageText>();

        var center = Vector3.zero;
        var hits = 0;
        foreach (var renderer in target.PooledGetInChildren<Renderer>())
        {
            hits++;
            center += renderer.bounds.center;
        }

        if (hits == 0)
            center = target.transform.position;
        else
            center /= hits;

        damageText.transform.position = center;

        int finalAttributeValue = initialAttributeValue;
        computeDamage.ApplyDelta(ref finalAttributeValue);
        int delta = finalAttributeValue - initialAttributeValue;

        damageText.ElementColorTarget.color = computeDamage.Element == Element.Neutral ? Color.red : computeDamage.Element.GetAssociatedColor();
        (delta > 0 ? damageText.OnHeal : damageText.OnDamage)?.Invoke(Math.Abs(delta).ToString());
        _damageTextCache.Enqueue((Time.time + damageText.Lifetime, damageText));
    }

    void OnSwappedSelectedUnit()
    {
        UpdateTacticsButtonColor();

        if (UnitSelected == null)
            return;

        foreach (var (mod, display) in ModifierDisplays)
        {
            display.RemoveDisplay();
        }
        ModifierDisplays.Clear();

        foreach (var modifier in UnitSelected.Profile.Modifiers)
        {
            if (ModifierDisplays.TryGetValue(modifier, out _) || modifier.DisplayPrefab == null)
                continue;

            var display = Instantiate(modifier.DisplayPrefab, ModifierContainer);
            ModifierDisplays[modifier] = display;
            display.OnDisplayed(UnitSelected, this, modifier);
        }
    }

    void Update()
    {
        if (SwitchCharacter.action.WasPerformedThisFrameUnique())
            SwitchToNextHero(SwitchCharacter.action.ReadValue<float>() >= 0 ? 1 : -1);

        if (UnitSelected == null && BattleManagement.PartyLineup.Count != 0)
        {
            UnitSelected = BattleManagement.PartyLineup[0];
            OnSwappedSelectedUnit();
        }

        if (UnitSelected == null)
            return;

        if (SwitchSpecialInput.action.WasPerformedThisFrameUnique())
            UnitSelected.Profile.SwitchSpecialHandler?.OnSwitch(UnitSelected);

        { // MODIFIERS
            foreach (var modifier in UnitSelected.Profile.Modifiers)
            {
                if (ModifierDisplays.TryGetValue(modifier, out _) || modifier.DisplayPrefab == null)
                    continue;

                var display = Instantiate(modifier.DisplayPrefab, ModifierContainer);
                ModifierDisplays[modifier] = display;
                display.OnDisplayed(UnitSelected, this, modifier);
                display.OnNewModifier();
            }

            List<IModifier> modifiersToRemove = null;
            foreach (var (mod, display) in ModifierDisplays)
            {
                foreach (var modifier in UnitSelected.Profile.Modifiers)
                {
                    if (ReferenceEquals(modifier, mod))
                        goto FoundMatch;
                }

                try
                {
                    display.RemoveDisplay();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                modifiersToRemove ??= new();
                modifiersToRemove.Add(mod);

                FoundMatch:{}
            }

            if (modifiersToRemove is not null)
            {
                foreach (var modifier in modifiersToRemove)
                {
                    ModifierDisplays.Remove(modifier);
                }
            }
        }

        if (_waitingForScheduledUnit != null && (BattleManagement.Orders.TryGetValue(_waitingForScheduledUnit, out _) || BattleManagement.Processing.TryGetValue(_waitingForScheduledUnit, out _)))
        {
            if (CancelInput.action.WasPerformedThisFrameUnique())
            {
                BattleManagement.Orders.Remove(_waitingForScheduledUnit);
                BattleManagement.Interrupt(_waitingForScheduledUnit);
                _waitingForScheduledUnit = null;
                // Put the battle on pause, the player likely wants to schedule something
                CancelFullOrder();
                ResetNavigation();
                BattleManagement.Blocked |= BlockBattleFlags.PreparingOrders;
                Discard.gameObject.SetActive(true);
            }
        }
        else if (_waitingForScheduledUnit)
        {
            ResetNavigation();
            _waitingForScheduledUnit = null;
        }

        Skills.interactable = UnitSelected.Profile.Skills.Count > 0;
        Items.interactable = UnitSelected.Profile.Inventory.Items().FirstOrDefault(x => x.item is Consumable).item is Consumable;

        if (_order.Actions.Length != 0 && _runningUIOperation == null && CancelInput.action.WasPerformedThisFrameUnique())
            _order.Actions.BackingArray = _order.Actions.AsSpan()[..^1].ToArray();

        if (BattleManagement.Processing.TryGetValue(UnitSelected, out var progress))
        {
            var span = progress.chosenTactic.Actions.AsSpan()[progress.actionI..];
            UpdatePreview(progress.chosenTactic, span, PreviewType.Execution);
        }
        else if (_order.Actions.Length != 0) // Order being composed by the player right now
        {
            UpdatePreview(_order, _order.Actions, PreviewType.Order);
        }
        else if (BattleManagement.Orders.TryGetValue(UnitSelected, out var order)) // Order scheduled
        {
            UpdatePreview(order, order.Actions, PreviewType.Order);
        }
        else if (BattleManagement.TacticsDisabled.Contains(UnitSelected))
        {
            UpdatePreview(null, default, PreviewType.Order);
        }
        else // No orders, find the first tactics that can run
        {
            Tactics tacticsPreviewed = null;
            using (BattleManagement.Units.TemporaryCopy(out var unitsCopy))
            {
                foreach (var tactic in UnitSelected.Profile.Tactics)
                {
                    if (tactic != null && tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, new TargetCollection(unitsCopy), UnitSelected.Context, out _, accountForCost:false))
                    {
                        tacticsPreviewed = tactic;
                        break;
                    }
                }
            }
            ReadOnlySpan<IAction> actions = tacticsPreviewed != null ? tacticsPreviewed.Actions.AsSpan() : default;
            UpdatePreview(tacticsPreviewed, actions, PreviewType.Tactics);
        }

        foreach (var unit in BattleManagement.Units)
        {
            if (!ProcessedUnits.Add(unit))
                continue;

            if (unit.Profile is HeroExtension hero)
            {
                HeroData.Add(hero);
            }
            else
            {
                EnemyData.Add(unit.Profile);

                var renderer = unit.transform.GetComponentInChildren<Renderer>();
                var rendererTransform = renderer.transform;
                var enemyUI = Instantiate(EnemyUIPrefab);
                var rectTransform = enemyUI.GetComponent<RectTransform>();
                enemyUI.transform.position = new Vector3(rendererTransform.position.x, renderer.bounds.max.y + rectTransform.sizeDelta.y, rendererTransform.position.z);
                rectTransform.SetParent(rendererTransform, true);
                enemyUI.name = $"{unit.gameObject.name} UI";

                EnemyPrefabUIData data = enemyUI.GetComponent<EnemyPrefabUIData>();
                data.identity.text = unit.Profile.Name;
                data.healthBar.fillAmount = unit.Profile.EffectiveStats.HP;

                EnemyUIData.Add(data);
            }
        }

        SelectedUI.nameLabel.text = UnitSelected.Profile.Name;
        SelectedUI.characterIcon.sprite = UnitSelected.Profile.Portrait;
        SelectedUI.atbBar.fillAmount = UnitSelected.Profile.ActionsCharged / UnitSelected.Profile.ActionChargeMax;
        SelectedUI.healthBar.fillAmount = (float)UnitSelected.Profile.CurrentHP / UnitSelected.Profile.EffectiveStats.HP;
        SelectedUI.health.text = UnitSelected.Profile.CurrentHP.ToString();
        int j = 0;
        for (int i = 0; i < HeroData.Count; i++)
        {
            if (HeroData[i] != UnitSelected.Profile)
            {
                HeroUIData[j].gameObject.SetActive(true);
                HeroUIData[j].characterIcon.sprite = HeroData[i].Portrait;
                HeroUIData[j].atbBar.fillAmount = HeroData[i].ActionsCharged / HeroData[i].ActionChargeMax;
                HeroUIData[j].healthBar.fillAmount = (float)HeroData[i].CurrentHP / HeroData[i].EffectiveStats.HP;
                HeroUIData[j].health.text = HeroData[i].CurrentHP.ToString();
                j++;
            }
        }
        for (int i = 0; i < EnemyData.Count; i++)
        {
            EnemyUIData[i].healthBar.fillAmount = (float)EnemyData[i].CurrentHP / EnemyData[i].EffectiveStats.HP;
            EnemyUIData[i].health.text = EnemyData[i].CurrentHP.ToString();
        }
    }

    IEnumerable PresentAttackUI()
    {
        if (_order.Actions.Length == 0 || _order.Actions.BackingArray[^1] != null)
            Array.Resize(ref _order.Actions.BackingArray, _order.Actions.BackingArray.Length + 1);

        if (UnitSelected == null)
            yield break;

        _order.Actions.BackingArray[^1] = UnitSelected.Profile.BasicAttack;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
            {
                if (yields is CancelRequested)
                {
                    _order.Actions.BackingArray = _order.Actions.BackingArray.AsSpan()[..^1].ToArray();
                    yield break;
                }

                yield return yields;
            }
        }
    }

    IEnumerable PresentSkillsUI()
    {
        AGAIN:
        SubActionSelectionContainer.gameObject.SetActive(true);

        Skill selectedSkill = null;
        bool cancel = false;
        var objects = new List<RectTransform>();
        try
        {
            if (UnitSelected == null)
                yield break;

            bool first = true;
            var previousCursor = _lastActionCursor;
            foreach (var skill in UnitSelected.Profile.Skills)
            {
                var button = CreateButton(skill.name, SkillTemplate, SubActionSelectionContainer, objects, () => selectedSkill = skill, _ =>
                {
                    _lastActionCursor = skill;
                    TooltipUI.OnPresentNewTooltip?.Invoke(skill.Description);
                });

                if (ReferenceEquals(previousCursor, skill))
                    button.Select();

                if (_order.Actions.CostTotal() + skill.ATBCost > UnitSelected.Profile.ActionChargeMax)
                {
                    button.interactable = false;
                }
                else if (first)
                {
                    button.Select();
                    first = false;
                }
            }

            CreateButton("<<-", SkillTemplate, SubActionSelectionContainer, objects, () => cancel = true);

            while (selectedSkill == null && cancel == false)
            {
                if (CancelInput.action.WasPerformedThisFrameUnique())
                    yield break;

                yield return null;
            }

            if (cancel)
                yield break;
        }
        finally
        {
            TooltipUI.OnHideTooltip?.Invoke();
            foreach (var o in objects)
                Destroy(o.gameObject);

            SubActionSelectionContainer.gameObject.SetActive(false);
        }

        if (_order.Actions.Length == 0 || _order.Actions.BackingArray[^1] != null)
            Array.Resize(ref _order.Actions.BackingArray, _order.Actions.BackingArray.Length + 1);

        _order.Actions.BackingArray[^1] = selectedSkill;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
            {
                if (yields is CancelRequested)
                {
                    _order.Actions.BackingArray = _order.Actions.BackingArray.AsSpan()[..^1].ToArray();
                    goto AGAIN;
                }

                yield return yields;
            }
        }

        if (_order.Condition != null && _order.Actions.CostTotal() < UnitSelected.Profile.ActionChargeMax && _order.Condition != null)
            goto AGAIN;
    }

    IEnumerable PresentItemUI()
    {
        AGAIN:
        SubActionSelectionContainer.gameObject.SetActive(true);

        Consumable selectedConsumable = null;
        var cancel = false;
        var objects = new List<RectTransform>();
        try
        {
            if (UnitSelected == null)
                yield break;

            bool first = true;
            var previousCursor = _lastActionCursor;
            foreach ((BaseItem item, uint count) in UnitSelected.Profile.Inventory.Items())
            {
                if (item is not Consumable consumable)
                    continue;

                var button = CreateButton(($"{consumable.name} (x{count})"), SkillTemplate, SubActionSelectionContainer, objects, () => selectedConsumable = consumable, _ =>
                {
                    _lastActionCursor = consumable;
                    TooltipUI.OnPresentNewTooltip?.Invoke(consumable.Description);
                });

                if (ReferenceEquals(previousCursor, consumable))
                    button.Select();

                if (_order.Actions.CostTotal() + consumable.ATBCost > UnitSelected.Profile.ActionChargeMax)
                {
                    button.interactable = false;
                }
                else if (first)
                {
                    button.Select();
                    first = false;
                }
            }

            CreateButton("<<-", SkillTemplate, SubActionSelectionContainer, objects, () => cancel = true);

            while (selectedConsumable == null && cancel == false)
            {
                if (CancelInput.action.WasPerformedThisFrameUnique())
                    yield break;

                yield return null;
            }

            if (cancel)
                yield break;
        }
        finally
        {
            TooltipUI.OnHideTooltip?.Invoke();
            foreach (var o in objects)
                Destroy(o.gameObject);

            SubActionSelectionContainer.gameObject.SetActive(false);
        }

        if (_order.Actions.Length == 0 || _order.Actions.BackingArray[^1] != null)
            Array.Resize(ref _order.Actions.BackingArray, _order.Actions.BackingArray.Length + 1);

        _order.Actions.BackingArray[^1] = selectedConsumable;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
            {
                if (yields is CancelRequested)
                {
                    _order.Actions.BackingArray = _order.Actions.BackingArray.AsSpan()[..^1].ToArray();
                    goto AGAIN;
                }

                yield return yields;
            }
        }

        if (_order.Condition != null && _order.Actions.CostTotal() < UnitSelected.Profile.ActionChargeMax && _order.Condition != null)
            goto AGAIN;
    }

    static Button CreateButton(string name, RectTransform SkillTemplate, RectTransform SelectionContainer, ICollection<RectTransform> objects, UnityAction OnClick, [CanBeNull] UnityAction<BaseEventData> OnHoverOrSelected = null)
    {
        var uiElem = Instantiate(SkillTemplate, SelectionContainer, false);
        uiElem.gameObject.SetActive(true);
        objects.Add(uiElem);
        if (uiElem.GetComponentInChildren<Text>() is { } text && text != null)
            text.text = name;
        if (uiElem.GetComponentInChildren<TMP_Text>() is { } tmp_text && tmp_text != null)
            tmp_text.text = name;


        var button = uiElem.GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        if (OnHoverOrSelected is not null)
        {
            var onHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onHover.callback.AddListener(OnHoverOrSelected);
            var onSelect = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            onSelect.callback.AddListener(OnHoverOrSelected);

            if (button.gameObject.TryGetComponent(out EventTrigger trigger) == false)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            trigger.triggers.Add(onHover);
            trigger.triggers.Add(onSelect);
        }

        return button;
    }

    void TacticsPressed()
    {
        if (BattleManagement.TacticsDisabled.Remove(UnitSelected) == false)
            BattleManagement.TacticsDisabled.Add(UnitSelected);
        UpdateTacticsButtonColor();
    }

    void UpdateTacticsButtonColor()
    {
        var cols = Tactics.colors;
        cols.normalColor = BattleManagement.TacticsDisabled.Contains(UnitSelected) ? _disabledTacticsColor : _initialTacticsColor;
        Tactics.colors = cols;
    }

    IEnumerable PresentTargetSelectionUI()
    {
        SubActionSelectionContainer.gameObject.SetActive(true);

        bool accepted = false;
        UnityAction listener = () => accepted = true;
        if (MultiSelection)
        {
            AcceptSelection.gameObject.SetActive(true); // Disabled for now, may come back if selection targeting comes back
            AcceptSelection.onClick.AddListener(listener);
        }

        IUIElementSelection<HashSet<BattleCharacterController>> selector = null;
        try
        {
            if (_groupSelection.ValidAndPrepared(UnitSelected, _order.Actions, BattleManagement.Units))
            {
                selector = _groupSelection;
            }
            else // Fallback to generic handler if no groups could be found
            {
                _targetSelection.Prepare(UnitSelected, _order.Actions, BattleManagement.Units);
                selector = _targetSelection;
            }

            HashSet<BattleCharacterController> selection;
            do
            {
                selector.UpdateRenderingAndSelection();
                if (MultiSelection)
                    AcceptSelection.interactable = selector.HasAnythingSelected();
                else
                    accepted = selector.HasAnythingSelected();

                if (accepted && selector.TryGetSelected(out selection))
                    break; // Succeeded

                accepted = false; // Reset click

                if (CancelInput.action.WasPerformedThisFrameUnique())
                    while (true)
                        yield return new CancelRequested(); // Infinite loop to ensure we do not advance out of a cancel

                yield return null;
            } while (true);

            _order.Condition = ScriptableObject.CreateInstance<ActionCondition>();
            _order.Condition.TargetFilter = new SpecificTargetsCondition { Targets = selection.ToHashSet() };
        }
        finally
        {
            TooltipUI.OnHideTooltip?.Invoke();

            selector?.Close();

            AcceptSelection.onClick.RemoveListener(listener);
            AcceptSelection.gameObject.SetActive(false);
            SubActionSelectionContainer.gameObject.SetActive(false);
        }
    }

    void SwitchToNextHero(int dir)
    {
        int partyCount = BattleManagement.PartyLineup.Count;
        int indexOfOldSelection = BattleManagement.PartyLineup.IndexOf(UnitSelected);
        // Search for the next unit forward or backwards in the list
        for (int k = Mod(indexOfOldSelection + dir, partyCount); k != indexOfOldSelection; k = Mod(k + dir, partyCount))
        {
            if (BattleManagement.PartyLineup[k].Profile.CurrentHP == 0)
                continue;

            UnitSelected = BattleManagement.PartyLineup[k];
            OnSwappedSelectedUnit();
            break;
        }

        static int Mod(int x, int y)
        {
            // Make sure the result is positive when x is negative
            // i.e.: -1%10 == -1, we want == 9 instead
            return x < 0 ? y + (x % y) : x % y;
        }
    }

    void ScheduleOrder(Tactics tactics)
    {
        _order = new();
        ResetNavigation();
        if (UnitSelected != null)
        {
            BattleManagement.Orders[UnitSelected] = tactics;
            _waitingForScheduledUnit = UnitSelected;
            HideNavigation();
        }

        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
    }

    void CancelFullOrder()
    {
        _order = new();
        ResetNavigation();
        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
    }

    void ResetNavigation()
    {
        if (_runningUIOperation is {} val)
        {
            StopCoroutine(val.coroutine);
            val.disposable.Dispose();
            _runningUIOperation = null;
        }

        HideNavigation();

        ActionSelectionContainer.gameObject.SetActive(true);

        if (_order.Actions.Length > 0 && _order.Condition != null)
        {
            Schedule.gameObject.SetActive(true);
            Discard.gameObject.SetActive(true);
        }
    }

    void HideNavigation()
    {
        Schedule.gameObject.SetActive(false);
        Discard.gameObject.SetActive(false);
        ItemTemplate.gameObject.SetActive(false);
        SkillTemplate.gameObject.SetActive(false);
        Targets.AlliesTargetTemplate.gameObject.SetActive(false);
        Targets.HostileTargetTemplate.gameObject.SetActive(false);
        SubActionSelectionContainer.gameObject.SetActive(false);
        AcceptSelection.gameObject.SetActive(false);
        ActionSelectionContainer.gameObject.SetActive(false);
    }

    bool TryOrderWizard(IEnumerable inner)
    {
        if (_runningUIOperation != null)
            return false;

        var enumerable = BattleUICoroutine().GetEnumerator();
        _runningUIOperation = (null, (IDisposable)enumerable);
        var coroutine = StartCoroutine(enumerable);
        // The coroutine may complete in its entirety when calling StartCoroutine when it never yields,
        // make sure that we keep it as finished if that's the case
        if (_runningUIOperation != null)
            _runningUIOperation = (coroutine, (IDisposable)enumerable);

        return true;

        IEnumerable BattleUICoroutine()
        {
            BattleManagement.Blocked |= BlockBattleFlags.PreparingOrders;
            try
            {
                ActionSelectionContainer.gameObject.SetActive(false);

                foreach (var yield in inner)
                    yield return yield;
            }
            finally
            {
                _runningUIOperation = null;

                ResetNavigation();

                if (_order.Actions.Length == 0) // Order was cancelled
                    CancelFullOrder();

                if (_order.Actions.CostTotal() >= 4 && _order.Condition != null)
                    ScheduleOrder(_order);
            }
        }
    }


    public void UpdatePreview([CanBeNull] Tactics tactics, ReadOnlySpan<IAction> actionsSubset, PreviewType type)
    {
        using var _temp = BattleManagement.Units.TemporaryCopy(out var _unitsCopy);
        if (tactics != null && tactics.Condition != null && tactics.Condition.CanExecute(tactics.Actions, new TargetCollection(_unitsCopy), UnitSelected!.Context, out var selection, accountForCost:false))
        {
            int i = 0;
            foreach (var controller in selection)
            {
                if (i >= _targetCursors.Count)
                    _targetCursors.Add(Instantiate(TargetCursorTemplate));

                var position = controller.transform.position;
                foreach (var renderer in controller.PooledGetInChildren<Renderer>())
                    position.y = Mathf.Max(renderer.bounds.max.y, position.y);

                _targetCursors[i++].transform.SetPositionAndRotation(position, Camera.main!.transform.rotation);
            }

            for (; i < _targetCursors.Count; i++)
                _targetCursors[i].SetActive(false);
        }
        else
        {
            foreach (var cursor in _targetCursors)
                cursor.SetActive(false);
        }

        var current = _existingPreviews.AsSpan();
        int minLength = Math.Min(current.Length, actionsSubset.Length);
        int leftMatches = 0;
        int rightMatches = 0;
        for (int i = 0; i < minLength; i++)
        {
            if (current[i].action == actionsSubset[i])
                leftMatches++;
            else
                break;
        }

        for (int i = 1; i <= minLength; i++)
        {
            if (current[^i].action == actionsSubset[^i])
                rightMatches++;
            else
                break;
        }

        System.Range matchRange = rightMatches > leftMatches ? ^rightMatches.. : ..leftMatches;
        System.Range nonmatchRange = rightMatches > leftMatches ? ..^rightMatches : leftMatches..;

        var toKeep = current[matchRange];
        var toDiscard = current[nonmatchRange];
        var toAdd = actionsSubset[nonmatchRange];

        if (toDiscard.Length == 0 && toAdd.Length == 0)
        {
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i].type != type)
                {
                    current[i].type = type;
                    CallPreviewTypeMethod(type, current[i].ui);
                }
            }
            return;
        }

        foreach (var (action, ui, _) in toDiscard)
            ui.Discarded?.Invoke();

        if (rightMatches > leftMatches)
        {
            for (int i = 0; i < toKeep.Length; i++)
            {
                var (action, ui, _) = toKeep[i];
                int newPosition = toAdd.Length + i;
                ui.Moved?.Invoke();
            }
        }

        _existingPreviews = new (IAction action, UIActionPreview ui, PreviewType type)[toKeep.Length + toAdd.Length];
        var newAsSpan = _existingPreviews.AsSpan();
        toKeep.CopyTo(newAsSpan[matchRange]);
        var newAsSpanAddRange = newAsSpan[nonmatchRange];
        for (int i = 0; i < toAdd.Length; i++)
        {
            var newElement = Instantiate(ActionPreviewTemplate.gameObject, ActionPreviewTemplate.transform.parent);
            newElement.SetActive(true);
            var ui = newElement.GetComponent<UIActionPreview>();
            var rect = (RectTransform)ui.transform;
            var size = rect.sizeDelta;
            size.x *= toAdd[i].ActionCost;
            rect.sizeDelta = size;
            ui.Created?.Invoke(toAdd[i].Name);
            CallPreviewTypeMethod(type, ui);
            newAsSpanAddRange[i] = (toAdd[i], ui, type);
        }

        if (toAdd.Length != 0)
        {
            for (int i = 0; i < _existingPreviews.Length; i++)
                _existingPreviews[i].ui.transform.SetSiblingIndex(i);
        }

        if (_previewType != type)
        {
            _previewType = type;
            for (int i = 0; i < _existingPreviews.Length; i++)
                CallPreviewTypeMethod(type, _existingPreviews[i].ui);
        }

        static void CallPreviewTypeMethod(PreviewType type, UIActionPreview ui)
        {
            var invoker = type switch
            {
                PreviewType.Tactics => ui.IsTactics,
                PreviewType.Order => ui.IsOrder,
                PreviewType.Execution => ui.IsExecution,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            invoker?.Invoke();
        }
    }


    public enum PreviewType
    {
        Tactics,
        Order,
        Execution
    }

    [Serializable]
    public class TargetList
    {
        [ValidateInput(nameof(HasToggle), "Must have a toggle")]
        public RectTransform HostileTargetTemplate;
        [ValidateInput(nameof(HasToggle), "Must have a toggle")]
        public RectTransform AlliesTargetTemplate;

        bool HasToggle(RectTransform val, ref string errorMessage)
        {
            return val != null && val.GetComponentInChildren<Toggle>();
        }
    }

    class TargetSelection : UIElementSelection<BattleCharacterController, HashSet<BattleCharacterController>>
    {
        [ItemCanBeNull] readonly HashSet<BattleCharacterController> _lastSelection = new();
        [ItemCanBeNull] readonly HashSet<BattleCharacterController> _selection = new();
        BattleCharacterController _unitSelected;
        BattleCharacterController _lastCursor;
        IActionCollection _actions;
        List<BattleCharacterController> _units;

        public TargetSelection(BattleUIOperation battleUIParam) : base(battleUIParam) { }

        public void Prepare(BattleCharacterController unitSelected, IActionCollection actions, List<BattleCharacterController> units)
        {
            _selection.Clear();
            _unitSelected = unitSelected;
            _actions = actions;
            _units = units;

            Clear();
            UpdateRenderingAndSelection();

            bool foundCursor = false;
            foreach (var (unit, (parent, toggle)) in Toggles)
            {
                if (unit == _lastCursor)
                {
                    toggle.Select();
                    foundCursor = true;
                }
            }

            if (MultiSelection)
                foreach (var unit in _lastSelection)
                    if (unit != null && Toggles.TryGetValue(unit, out var toggleData))
                        toggleData.toggle.isOn = true;

            if (foundCursor == false)
            {
                foreach (var (unit, (parent, toggle)) in Toggles)
                {
                    if (toggle.isOn)
                    {
                        toggle.Select();
                        foundCursor = true;
                        break;
                    }
                }
            }

            if (foundCursor == false)
            {
                foreach (var controller in units)
                {
                    if (Toggles.TryGetValue(controller, out var toggleData))
                    {
                        toggleData.toggle.Select();
                        break;
                    }
                }
            }
        }

        public override void Close()
        {
            _lastSelection.Clear();
            foreach (var selected in _selection)
                _lastSelection.Add(selected);

            _selection.Clear();
            base.Close();
        }

        public override bool HasAnythingSelected()
        {
            return _selection.Count > 0;
        }

        public override bool TryGetSelected(out HashSet<BattleCharacterController> values)
        {
            values = new(_selection);
            return _selection.Count > 0;
        }

        protected override IEnumerable<BattleCharacterController> GetItems()
        {
            return _units.Where(x => IsFiltered(x, _actions, _unitSelected.Context, _units) == false).OrderByDescending(x => x.IsHostileTo(_unitSelected));
        }

        protected override void OnHoverOrSelected(BattleCharacterController unit)
        {
            _lastCursor = unit;
            TooltipUI.OnPresentNewTooltip?.Invoke($"Level {unit.Profile.Level}\n{unit.Profile.EffectiveStats.ToStringOneStatPerLine()}");
        }

        protected override void OnRemoved(BattleCharacterController unit, bool fromClearAction)
        {
            _selection.Remove(unit);

            Filter(_actions, _unitSelected.Context, _selection);

            foreach (var (otherUnit, (_, toggle)) in Toggles)
                toggle.SetIsOnWithoutNotify(_selection.Contains(otherUnit));
        }

        protected override void OnNew(BattleCharacterController unit, out string label, out bool isOn, out bool inHostileList)
        {
            isOn = _selection.Contains(unit);
            label = unit.Profile.Name;
            inHostileList = unit.IsHostileTo(_unitSelected);
        }

        protected override void OnToggled(BattleCharacterController unit, bool isOn)
        {
            if (isOn)
                _selection.Add(unit);
            else
                _selection.Remove(unit);

            if (Filtered(_actions, _unitSelected.Context, _selection, out var newSelection) == false)
                return;

            // Some of the units were filtered out:
            if (isOn) // Tried to add this unit ?
            {
                if (newSelection.Contains(unit)) // The unit bound to this toggle was added in, some other unit was removed
                {
                    ReplaceSelection(_selection, newSelection, Toggles);
                }
                else // This unit was filtered out
                {
                    // Try again with just this unit selected
                    if (Filtered(_actions, _unitSelected.Context, new() { unit }, out var newSelectionWithOnlyIt) == false)
                        ReplaceSelection(_selection, newSelectionWithOnlyIt, Toggles);
                    // Else, don't add it, this unit is likely not compatible with this action
                }
            }
            else // Tried to remove this unit ? But it removed more units than that one
            {
                // Roll the deselection back
                _selection.Add(unit);
                Toggles[unit].toggle.SetIsOnWithoutNotify(true);
            }

            static void ReplaceSelection(HashSet<BattleCharacterController> write, HashSet<BattleCharacterController> read, IReadOnlyDictionary<BattleCharacterController, (RectTransform parent, Toggle toggle)> toggles)
            {
                // Remove those that are not part of this new selection, and update their toggles
                foreach (var unit in write)
                {
                    if (read.Contains(unit) == false)
                        toggles[unit].toggle.SetIsOnWithoutNotify(false);
                }
                // Copy new into current
                write.Clear();
                foreach (var unit in read)
                    write.Add(unit);
            }
        }

        static bool Filtered(IActionCollection actions, EvaluationContext context, HashSet<BattleCharacterController> input, out HashSet<BattleCharacterController> output)
        {
            var collection = new TargetCollection(input.ToList());
            var filtered = collection;
            foreach (var action in actions)
                action.TargetFilter?.Filter(ref filtered, context);

            if (collection != filtered)
            {
                output = filtered.ToHashSet();
                return true;
            }
            else
            {
                output = input;
                return false;
            }
        }

        static bool Filter(IActionCollection actions, EvaluationContext context, HashSet<BattleCharacterController> input)
        {
            var collection = new TargetCollection(input.ToList());
            var filtered = collection;
            foreach (var action in actions)
                action.TargetFilter?.Filter(ref filtered, context);

            if (collection != filtered)
            {
                input.Clear();
                foreach (var unit in filtered)
                    input.Add(unit);
                return true;
            }

            return false;
        }

        bool IsFiltered(BattleCharacterController x, IActionCollection actions, EvaluationContext context, List<BattleCharacterController> units)
        {
            var alone = new TargetCollection(units);
            alone.Empty();
            alone.SetAt(units.IndexOf(x));

            foreach (var action in actions)
                action.TargetFilter?.Filter(ref alone, context);

            return alone.IsEmpty;
        }
    }

    class GroupSelection : UIElementSelection<GroupSelection.TargetGroupNonAlloc, HashSet<BattleCharacterController>>
    {
        readonly HashSet<TargetGroupNonAlloc> _lastSelection = new();
        readonly HashSet<TargetGroupNonAlloc> _selectedTargetGroups = new();
        readonly TargetGroupNonAlloc[] _targetGroups;
        TargetGroupNonAlloc _lastCursor;
        BattleCharacterController _unitSelected;
        IActionCollection _actions;
        List<BattleCharacterController> _units;

        public GroupSelection(BattleUIOperation battleUIParam) : base(battleUIParam)
        {
            _targetGroups = new[]
            {
                new TargetGroupNonAlloc { GenericName = "Hostiles", Filter = x => x.IsHostileTo(_unitSelected!.Context.Controller), ConsideredHostile = true },
                new TargetGroupNonAlloc { GenericName = "Allies", Filter = x => x.IsHostileTo(_unitSelected!.Context.Controller) == false, ConsideredHostile = false }
            };
        }

        public bool ValidAndPrepared(BattleCharacterController unitSelectedParam, IActionCollection actions, List<BattleCharacterController> units)
        {
            _unitSelected = unitSelectedParam;
            _actions = actions;
            _units = units;

            bool valid = false;
            foreach (var groupNonAlloc in _targetGroups)
            {
                if (EvaluateGroupFilter(groupNonAlloc, _units, _actions, unitSelectedParam.Context, out _, out _))
                {
                    valid = true;
                    break;
                }
            }

            if (valid == false)
                return false;

            _selectedTargetGroups.Clear();
            Clear();
            UpdateRenderingAndSelection();

            bool foundCursor = false;
            foreach (var (otherGroup, (_, otherToggle)) in Toggles)
            {
                if (otherGroup.Equals(_lastCursor))
                {
                    otherToggle.Select();
                    foundCursor = true;
                }

                if (MultiSelection && _lastSelection.Contains(otherGroup))
                    otherToggle.isOn = true;
            }

            if (foundCursor == false)
            {
                foreach (var (otherGroup, (_, otherToggle)) in Toggles)
                {
                    if (otherToggle.isOn)
                    {
                        otherToggle.Select();
                        break;
                    }
                }
            }

            return true;
        }

        public override void Close()
        {
            _lastSelection.Clear();
            foreach (var selected in _selectedTargetGroups)
                _lastSelection.Add(selected);

            _selectedTargetGroups.Clear();
            base.Close();
        }

        public override bool HasAnythingSelected()
        {
            return _selectedTargetGroups.Count != 0;
        }

        public override bool TryGetSelected([MaybeNullWhen(false)]out HashSet<BattleCharacterController> units)
        {
            if (_selectedTargetGroups.Count == 0)
            {
                units = null;
                return false;
            }

            units = new();
            foreach (var group in _selectedTargetGroups)
            {
                EvaluateGroupFilter(group, _units, _actions, _unitSelected.Context, out _, out var selection);
                foreach (var unit in selection)
                    units.Add(unit);
            }

            return units.Count != 0; // Unlikely but just in case
        }

        protected override IEnumerable<TargetGroupNonAlloc> GetItems()
        {
            return _targetGroups.Where(x => EvaluateGroupFilter(x, _units, _actions, _unitSelected.Context, out _, out _));
        }

        protected override void OnHoverOrSelected(TargetGroupNonAlloc group)
        {
            _lastCursor = group;
            EvaluateGroupFilter(group, _units, _actions, _unitSelected.Context, out _, out var selection);
            TooltipUI.OnPresentNewTooltip?.Invoke($"Targets: {string.Join(", ", selection.Select(x => x.Profile.Name))}");
        }

        protected override void OnRemoved(TargetGroupNonAlloc group, bool fromClearAction)
        {
            _selectedTargetGroups.Remove(group);
        }

        protected override void OnNew(TargetGroupNonAlloc obj, out string label, out bool isOn, out bool inHostileList)
        {
            label = obj.GenericName;
            isOn = _selectedTargetGroups.Contains(obj);
            inHostileList = obj.ConsideredHostile;
        }

        protected override void OnToggled(TargetGroupNonAlloc group, bool isOn)
        {
            if (isOn)
            {
                foreach (var (otherGroup, (_, otherToggle)) in Toggles)
                {
                    _selectedTargetGroups.Remove(otherGroup);
                    if (otherGroup.Equals(group) == false)
                        otherToggle.SetIsOnWithoutNotify(false);
                }

                _selectedTargetGroups.Add(group);
            }
            else
            {
                _selectedTargetGroups.Remove(group);
            }
        }

        bool EvaluateGroupFilter(TargetGroupNonAlloc group, List<BattleCharacterController> allUnits, IActionCollection actions, EvaluationContext context, out string specificName, out TargetCollection selection)
        {
            int initiallyValidCount = 0;
            selection = new(allUnits);
            using var e = selection.GetEnumerator();
            for (; e.MoveNext();)
            {
                if (group.Filter(e.Current) == false)
                    selection.RemoveAt(e.CurrentIndex);
                else
                    initiallyValidCount++;
            }

            foreach (var action in actions)
                action.TargetFilter?.Filter(ref selection, context);

            var count = selection.CountSlow();
            if (count == 0 || count == 1 && initiallyValidCount != 1)
            {
                specificName = group.GenericName;
                return false;
            }
            else if (count == 1)
            {
                specificName = "";
                foreach (var character in selection)
                {
                    specificName = character.name;
                    break;
                }
                return true;
            }
            else
            {
                specificName = group.GenericName;
                return true;
            }
        }

        public struct TargetGroupNonAlloc : IEquatable<TargetGroupNonAlloc>
        {
            public string GenericName;
            public bool ConsideredHostile;
            public Func<BattleCharacterController, bool> Filter;

            public bool Equals(TargetGroupNonAlloc other) => GenericName == other.GenericName && ConsideredHostile == other.ConsideredHostile && Equals(Filter, other.Filter);

            public override bool Equals(object obj) => obj is TargetGroupNonAlloc other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(GenericName, ConsideredHostile, Filter);
        }
    }

    abstract class UIElementSelection<T, T2> : IUIElementSelection<T2>
    {
        readonly BattleUIOperation battleUI;
        readonly List<T> _temp = new();

        protected readonly Dictionary<T, (RectTransform parent, Toggle toggle)> Toggles = new();
        protected BattleTooltipUI TooltipUI => battleUI.TooltipUI;

        TargetList _targetList => battleUI.Targets;

        protected UIElementSelection(BattleUIOperation battleUIParam)
        {
            battleUI = battleUIParam;
        }

        public abstract bool HasAnythingSelected();
        public abstract bool TryGetSelected(out T2 values);
        protected abstract IEnumerable<T> GetItems();
        protected abstract void OnHoverOrSelected(T obj);
        protected abstract void OnRemoved(T obj, bool fromClearAction);
        protected abstract void OnNew(T obj, out string label, out bool isOn, out bool inHostileList);
        protected abstract void OnToggled(T obj, bool isOn);


        public void UpdateRenderingAndSelection()
        {
            _temp.Clear();
            foreach (var item in GetItems())
            {
                _temp.Add(item);
                if (Toggles.TryGetValue(item, out _))
                    continue;

                OnNew(item, out var label, out var isOn, out var inHostileList);

                RectTransform uiElem;
                var template = inHostileList ? _targetList.HostileTargetTemplate : _targetList.AlliesTargetTemplate;
                uiElem = Instantiate(template, template.transform.parent, false);
                uiElem.gameObject.SetActive(true);
                if (uiElem.GetComponentInChildren<Text>() is { } text && text != null)
                    text.text = label;
                if (uiElem.GetComponentInChildren<TMP_Text>() is { } tmp_text && tmp_text != null)
                    tmp_text.text = label;

                var toggle = uiElem.GetComponent<Toggle>();

                var onHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                onHover.callback.AddListener(evt => OnHoverOrSelected(item));
                var onSelect = new EventTrigger.Entry { eventID = EventTriggerType.Select };
                onSelect.callback.AddListener(evt => OnHoverOrSelected(item));

                if (toggle.gameObject.TryGetComponent(out EventTrigger trigger) == false)
                    trigger = toggle.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();
                trigger.triggers.Add(onHover);
                trigger.triggers.Add(onSelect);

                toggle.SetIsOnWithoutNotify(isOn);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(b => OnToggled(item, b));
                Toggles.Add(item, (uiElem, toggle));
                template.transform.parent.gameObject.SetActive(true);
            }

            foreach (var (t, rect) in Toggles) // Mark all items that are no longer in the list and put them in temp
            {
                if (_temp.Remove(t))
                    continue; // In both lists, we can continue

                _temp.Add(t); // Not in existing toggles, add it to temp to mark it for deletion
            }

            foreach (T item in _temp)
            {
                Destroy(Toggles[item].parent.gameObject);
                Toggles.Remove(item);
            }

            foreach (T item in _temp)
                OnRemoved(item, false);

            _temp.Clear();
        }

        protected void Clear()
        {
            var cpy = Toggles.ToArray();
            foreach (var (t, (parent, toggle)) in Toggles)
                Destroy(parent.gameObject);

            Toggles.Clear();

            foreach (var v in cpy)
                OnRemoved(v.Key, true);
        }

        public virtual void Close() => Clear();
    }

    interface IUIElementSelection<T>
    {
        bool HasAnythingSelected();
        bool TryGetSelected(out T values);
        void UpdateRenderingAndSelection();
        void Close();
    }

    class CancelRequested{ }
}