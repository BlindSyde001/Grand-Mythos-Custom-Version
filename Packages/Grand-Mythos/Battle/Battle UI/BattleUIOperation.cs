using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Conditions;
using Effects;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BattleUIOperation : MonoBehaviour, IDisposableMenuProvider
{
    const bool MultiSelection = false;

    public BattleStateMachine BattleManagement;

    [Header("Selected Character")]
    [ReadOnly, CanBeNull] public BattleCharacterController UnitSelected;

    [Header("UI Info")]
    public List<HeroPrefabUIData> HeroUIData;
    public Gradient HealthLabelColorPercent = new(), ManaLabelColorPercent = new();
    [ReadOnly, SerializeField] public List<HeroExtension> HeroData;

    [Required] public GameObject EnemyUIPrefab;
    [ReadOnly, SerializeField] List<CharacterTemplate> EnemyData;
    [ReadOnly, SerializeField] List<EnemyPrefabUIData> EnemyUIData;

    [Required] public BattleTooltipUI TooltipUI;
    [Required] public DetailedInfoPanel DetailedInfoPanel;
    [Required] public InputActionReference DetailedInfoPanelOpen;

    [Header("Action Selection")]
    [Required] public RectTransform ActionSelectionContainer;
    [Required] public Button Attack;
    [Required] public Button Repeat;
    [Required] public Button Special;
    [Required] public Button Skills;
    [Required] public Button Items;

    [Header("Sub-Action Selection")]
    [Required] public InputActionReference CancelInput;
    [Required] public RectTransform SubActionSelectionContainer;
    [Required] public Button AcceptSelection;
    [ValidateInput(nameof(HasButton), "Must have a button")] public RectTransform ItemTemplate;
    [ValidateInput(nameof(HasButton), "Must have a button")] public RectTransform SkillTemplate;
    public TargetList Targets = new();

    [Header("Previews")]
    [Required] public GameObject TargetCursorTemplate;

    [Required] public DamageText DamageTextTemplate;

    public SerializableHashSet<BattleCharacterController> ProcessedUnits = new();

    (Coroutine coroutine, IDisposable disposable)? _runningUIOperation;
    bool _listenerBound;

    List<(GameObject cursor, BattleCharacterController? unit)> _targetCursors = new();
    Queue<(float availableAfter, DamageText component)> _damageTextCache = new();

    GroupSelection _groupSelection;
    TargetSelection _targetSelection;
    Dictionary<BattleCharacterController, IAction> _lastAction = new();

    Dictionary<(IModifier, CharacterTemplate), ModifierDisplay> _modifierDisplays = new();

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
        HideNavigation();
        if (_listenerBound == false)
        {
            _listenerBound = true;
            Attack.onClick.AddListener(() => TryOrderWizard(PresentAttackUI()));
            Repeat.onClick.AddListener(() => TryOrderWizard(PresentTargetSelectionUI(_lastAction[UnitSelected!])));
            Skills.onClick.AddListener(() => TryOrderWizard(PresentSkillsUI()));
            Items.onClick.AddListener(() => TryOrderWizard(PresentItemUI()));

            foreach (var ui in HeroUIData)
            {
                for (int i = ui.ModifierContainer.childCount - 1; i >= 0; i--)
                    Destroy(ui.ModifierContainer.transform.GetChild(i).gameObject);
            }
        }

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
        string text = computeDamage.Missed ? "Miss" : Math.Abs(delta).ToString();
        (delta > 0 ? damageText.OnHeal : damageText.OnDamage)?.Invoke(text);
        _damageTextCache.Enqueue((Time.time + damageText.Lifetime, damageText));
    }

    [MaybeNull] private IEnumerable _infoPanelRunning;

    void Update()
    {
        if (_infoPanelRunning is not null)
            return;

        if (DetailedInfoPanelOpen.action.WasPerformedThisFrameUnique())
        {
            _infoPanelRunning = DetailedInfoPanel.OpenAndAwaitClose(BattleManagement.Units.Select(x => x.Profile).ToArray());
            BattleManagement.Blocked |= BlockBattleFlags.DetailedInfoOpen;
            StartCoroutine(RunningPanelWatcher());

            IEnumerator RunningPanelWatcher()
            {
                try
                {
                    foreach (var yield in _infoPanelRunning)
                    {
                        yield return yield;
                    }
                }
                finally
                {
                    BattleManagement.Blocked &= ~BlockBattleFlags.DetailedInfoOpen;
                    _infoPanelRunning = null;
                }
            }
        }
        
        UpdateScene();
        UpdateSelected();
    }

    void UpdateScene()
    {
        foreach (var unit in BattleManagement.PartyLineup)
        {
            if (!ProcessedUnits.Add(unit))
                continue;

            if (unit.Profile is HeroExtension hero)
            {
                hero.Special?.OnBattleStart(unit);
                HeroData.Add(hero);
            }
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

        for (int i = 0; i < HeroData.Count; i++)
        {
            var ui = HeroUIData[i];
            var hero = HeroData[i];
            ui.gameObject.SetActive(true);
            ui.CharacterIcon.sprite = hero.Portrait;
            ui.ChargeBar.fillAmount = hero.ChargeTotal == 0 ? 0 : 1f - hero.ChargeLeft / hero.ChargeTotal;
            ui.AtbBar.fillAmount = 1f - hero.PauseLeft;
            //ui.HealthBar.fillAmount = (float)hero.CurrentHP / hero.EffectiveStats.HP;
            //ui.ManaBar.fillAmount = (float)hero.CurrentMP / hero.EffectiveStats.MP;
            ui.FlowBar.fillAmount = hero.CurrentFlow / 100f;
            ui.ManaLabel.text = hero.CurrentMP.ToString();
            ui.Health.text = hero.CurrentHP.ToString();
            ui.Health.color = HealthLabelColorPercent.Evaluate((float)hero.CurrentHP / hero.EffectiveStats.HP);
            ui.ManaLabel.color = HealthLabelColorPercent.Evaluate((float)hero.CurrentMP / hero.EffectiveStats.MP);
            ui.NameLabel.text = hero.Name;
        }

        for (int i = 0; i < EnemyData.Count; i++)
        {
            EnemyUIData[i].healthBar.fillAmount = (float)EnemyData[i].CurrentHP / EnemyData[i].EffectiveStats.HP;
            EnemyUIData[i].health.text = EnemyData[i].CurrentHP.ToString();
        }

        { // MODIFIERS
            for (int i = 0; i < HeroData.Count; i++)
            {
                var ui = HeroUIData[i];
                var unit = HeroData[i];
            
                foreach (var appliedModifier in unit.Modifiers)
                {
                    var modifier = appliedModifier.Modifier;
                    if (_modifierDisplays.TryGetValue((modifier, unit), out _) || modifier.DisplayPrefab == null)
                        continue;

                    var display = Instantiate(modifier.DisplayPrefab, ui.ModifierContainer);
                    _modifierDisplays[(modifier, unit)] = display;
                    display.OnDisplayed(unit, this, modifier);
                    display.OnNewModifier();
                }
            }

            List<(IModifier, CharacterTemplate)> modifiersToRemove = null;
            foreach (var ((mod, unit), display) in _modifierDisplays)
            {
                foreach (var modifier in unit.Modifiers)
                {
                    if (ReferenceEquals(modifier.Modifier, mod))
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
                modifiersToRemove.Add((mod, unit));

                FoundMatch:{}
            }

            if (modifiersToRemove is not null)
            {
                foreach (var modifier in modifiersToRemove)
                {
                    _modifierDisplays.Remove(modifier);
                }
            }
        }
    }

    private void UpdateSelected()
    {
        Repeat.interactable = UnitSelected != null && _lastAction.ContainsKey(UnitSelected);

        var characterReady = BattleManagement.Queue.FirstOrDefault(x => x.Profile.Team == BattleManagement.PlayerTeam && BattleManagement.Orders.ContainsKey(x) == false);
        if (characterReady != UnitSelected)
        {
            if (UnitSelected != null)
                HeroUIData[HeroData.IndexOf((HeroExtension)UnitSelected.Profile)].Highlight.gameObject.SetActive(false);
            
            UnitSelected = characterReady;
            ResetNavigation();
            if (UnitSelected is null)
                HideNavigation();

            foreach (var (mod, display) in _modifierDisplays)
                display.RemoveDisplay();
            _modifierDisplays.Clear();

            if (UnitSelected != null)
            {
                Special.interactable = UnitSelected.Profile.Special is not null;
                Special.GetComponentInChildren<TMP_Text>().text = UnitSelected.Profile.Special?.ButtonLabel ?? "Special";
                Special.onClick.RemoveAllListeners();
                Special.onClick.AddListener(() =>
                {
                    if (UnitSelected.Profile.Special is not null)
                        TryOrderWizard(UnitSelected.Profile.Special.OnButtonClicked(UnitSelected, this, PresentTargetSelectionUI));
                });
                HeroUIData[HeroData.IndexOf((HeroExtension)UnitSelected.Profile)].Highlight.gameObject.SetActive(true);
            }
        }
        
        if (characterReady)
            BattleManagement.Blocked |= BlockBattleFlags.PreparingOrders;
        else
            BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;

        if (UnitSelected == null)
            return;

        Skills.interactable = UnitSelected.Profile.Skills.Count > 0;
        Items.interactable = UnitSelected.Profile.Inventory.Items().FirstOrDefault(x => x.item is Consumable).item is Consumable;
    }

    IEnumerable PresentAttackUI()
    {
        if (UnitSelected == null)
            yield break;

        foreach (object o in PresentTargetSelectionUI(UnitSelected.Profile.BasicAttack))
            yield return o;
    }

    IEnumerable PresentSkillsUI()
    {
        if (UnitSelected == null)
            yield break;

        var menu = NewMenuOf<Skill>(nameof(PresentSkillsUI));
        foreach (var skill in UnitSelected.Profile.Skills)
        {
            var button = menu.NewButton(skill.name, skill, skill.Description);
            button.interactable = skill.ManaCost <= UnitSelected.Profile.CurrentMP;
        }

        var selectionTask = menu.SelectedItem();
        while (selectionTask.IsCompleted == false)
            yield return null;
            
        if (selectionTask.IsCanceled)
            yield break;

        foreach (object o in PresentTargetSelectionUI(selectionTask.Result))
            yield return o;
    }

    IEnumerable PresentItemUI()
    {
        if (UnitSelected == null)
            yield break;

        var menu = NewMenuOf<Consumable>(nameof(PresentItemUI));
        foreach ((BaseItem item, uint count) in UnitSelected.Profile.Inventory.Items())
        {
            if (item is not Consumable consumable)
                continue;

            var button = menu.NewButton($"{consumable.name} (x{count})", consumable, consumable.Description);
            button.interactable = consumable.ManaCost <= UnitSelected.Profile.CurrentMP;
        }

        var selectionTask = menu.SelectedItem();
        while (selectionTask.IsCompleted == false)
            yield return null;
            
        if (selectionTask.IsCanceled)
            yield break;

        foreach (object o in PresentTargetSelectionUI(selectionTask.Result))
            yield return o;
    }

    IEnumerable PresentTargetSelectionUI(IAction action)
    {
        if (UnitSelected == null)
            yield break;

        SubActionSelectionContainer.gameObject.SetActive(true);

        bool accepted = false;
        UnityAction listener = () => accepted = true;
        if (MultiSelection)
        {
            AcceptSelection.gameObject.SetActive(true); // Disabled for now, may come back if selection targeting comes back
            AcceptSelection.onClick.AddListener(listener);
        }

        IUIElementSelection<HashSet<BattleCharacterController>> selector = null;
        var tactic = new Tactics();
        try
        {
            if (_groupSelection.ValidAndPrepared(UnitSelected, action, BattleManagement.Units))
            {
                selector = _groupSelection;
            }
            else // Fallback to generic handler if no groups could be found
            {
                _targetSelection.Prepare(UnitSelected, action, BattleManagement.Units);
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
                {
                    yield break;
                }

                yield return null;
            } while (true);

            tactic.Condition = ScriptableObject.CreateInstance<ActionCondition>();
            tactic.Condition.TargetFilter = new SpecificTargetsCondition { Targets = selection.ToHashSet() };
            tactic.Action = action;
        }
        finally
        {
            TooltipUI.OnHideTooltip?.Invoke();

            selector?.Close();

            AcceptSelection.onClick.RemoveListener(listener);
            AcceptSelection.gameObject.SetActive(false);
            SubActionSelectionContainer.gameObject.SetActive(false);
        }

        ScheduleOrder(tactic);
    }

    void ScheduleOrder(Tactics tactics)
    {
        if (tactics.Action == null || tactics.Condition == null)
        {
            Debug.LogWarning("Tried to schedule an incomplete order");
            return;
        }

        ResetNavigation();
        if (UnitSelected != null)
        {
            _lastAction[UnitSelected] = tactics.Action;
            BattleManagement.Orders[UnitSelected] = tactics;
        }
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
    }

    void HideNavigation()
    {
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
            }
        }
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
        IAction _action;
        List<BattleCharacterController> _units;

        public TargetSelection(BattleUIOperation battleUIParam) : base(battleUIParam) { }

        public void Prepare(BattleCharacterController unitSelected, IAction action, List<BattleCharacterController> units)
        {
            _selection.Clear();
            _unitSelected = unitSelected;
            _action = action;
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
            return _units.Where(x => IsFiltered(x, _action, _unitSelected.Context, _units) == false).OrderByDescending(x => x.IsHostileTo(_unitSelected));
        }

        protected override void OnHoverOrSelected(BattleCharacterController unit)
        {
            _lastCursor = unit;
            TooltipUI.OnPresentNewTooltip?.Invoke($"Level {unit.Profile.Level}\n{unit.Profile.EffectiveStats.ToStringOneStatPerLine()}");
            UpdateSelectionArrow(Enumerable.Empty<BattleCharacterController>().Append(unit));
        }

        protected override void OnRemoved(BattleCharacterController unit, bool fromClearAction)
        {
            _selection.Remove(unit);

            Filter(_action, _unitSelected.Context, _selection);

            foreach (var (otherUnit, (_, toggle)) in Toggles)
                toggle.SetIsOnWithoutNotify(_selection.Contains(otherUnit));
        }

        protected override void OnNew(BattleCharacterController unit, out string label, out string[] subSelections, out bool isOn, out bool inHostileList)
        {
            isOn = _selection.Contains(unit);
            label = unit.Profile.Name;
            inHostileList = unit.IsHostileTo(_unitSelected);
            subSelections = Array.Empty<string>();
        }

        protected override void OnToggled(BattleCharacterController unit, bool isOn)
        {
            if (isOn)
                _selection.Add(unit);
            else
                _selection.Remove(unit);

            if (Filtered(_action, _unitSelected.Context, _selection, out var newSelection) == false)
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
                    if (Filtered(_action, _unitSelected.Context, new() { unit }, out var newSelectionWithOnlyIt) == false)
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

        static bool Filtered(IAction action, EvaluationContext context, HashSet<BattleCharacterController> input, out HashSet<BattleCharacterController> output)
        {
            var collection = new TargetCollection(input.ToList());
            var filtered = collection;
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

        static bool Filter(IAction action, EvaluationContext context, HashSet<BattleCharacterController> input)
        {
            var collection = new TargetCollection(input.ToList());
            var filtered = collection;
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

        bool IsFiltered(BattleCharacterController x, IAction action, EvaluationContext context, List<BattleCharacterController> units)
        {
            var alone = new TargetCollection(units);
            alone.Empty();
            alone.SetAt(units.IndexOf(x));

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
        IAction _action;
        List<BattleCharacterController> _units;

        public GroupSelection(BattleUIOperation battleUIParam) : base(battleUIParam)
        {
            _targetGroups = new[]
            {
                new TargetGroupNonAlloc { GenericName = "Hostiles", Filter = x => x.IsHostileTo(_unitSelected!.Context.Controller), ConsideredHostile = true },
                new TargetGroupNonAlloc { GenericName = "Allies", Filter = x => x.IsHostileTo(_unitSelected!.Context.Controller) == false, ConsideredHostile = false }
            };
        }

        public bool ValidAndPrepared(BattleCharacterController unitSelectedParam, IAction action, List<BattleCharacterController> units)
        {
            _unitSelected = unitSelectedParam;
            _action = action;
            _units = units;

            bool valid = false;
            foreach (var groupNonAlloc in _targetGroups)
            {
                if (EvaluateGroupFilter(groupNonAlloc, _units, _action, unitSelectedParam.Context, out _, out _))
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
                EvaluateGroupFilter(group, _units, _action, _unitSelected.Context, out _, out var selection);
                foreach (var unit in selection)
                    units.Add(unit);
            }

            return units.Count != 0; // Unlikely but just in case
        }

        protected override IEnumerable<TargetGroupNonAlloc> GetItems()
        {
            return _targetGroups.Where(x => EvaluateGroupFilter(x, _units, _action, _unitSelected.Context, out _, out _));
        }

        protected override void OnHoverOrSelected(TargetGroupNonAlloc group)
        {
            _lastCursor = group;
            EvaluateGroupFilter(group, _units, _action, _unitSelected.Context, out _, out var selection);
            TooltipUI.OnPresentNewTooltip?.Invoke($"Targets: {string.Join(", ", selection.Select(x => x.Profile.Name))}");
            UpdateSelectionArrow(selection);
        }

        protected override void OnRemoved(TargetGroupNonAlloc group, bool fromClearAction)
        {
            _selectedTargetGroups.Remove(group);
        }

        protected override void OnNew(TargetGroupNonAlloc obj, out string label, out string[] subSelections, out bool isOn, out bool inHostileList)
        {
            label = obj.GenericName;
            isOn = _selectedTargetGroups.Contains(obj);
            inHostileList = obj.ConsideredHostile;
            
            EvaluateGroupFilter(obj, _units, _action, _unitSelected.Context, out _, out var selection);
            subSelections = selection.Select(x => x.Profile.Name).ToArray();
            if (subSelections.Length > 0)
                label = "";
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

        bool EvaluateGroupFilter(TargetGroupNonAlloc group, List<BattleCharacterController> allUnits, IAction action, EvaluationContext context, out string specificName, out TargetCollection selection)
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
        readonly BattleUIOperation _battleUI;
        readonly List<T> _temp = new();

        protected readonly Dictionary<T, (RectTransform parent, Toggle toggle)> Toggles = new();
        protected BattleTooltipUI TooltipUI => _battleUI.TooltipUI;

        TargetList _targetList => _battleUI.Targets;

        protected UIElementSelection(BattleUIOperation battleUIParam)
        {
            _battleUI = battleUIParam;
        }

        public abstract bool HasAnythingSelected();
        public abstract bool TryGetSelected(out T2 values);
        protected abstract IEnumerable<T> GetItems();
        protected abstract void OnHoverOrSelected(T obj);
        protected abstract void OnRemoved(T obj, bool fromClearAction);
        protected abstract void OnNew(T obj, out string label, out string[] subSelections, out bool isOn, out bool inHostileList);
        protected abstract void OnToggled(T obj, bool isOn);

        protected void UpdateSelectionArrow(IEnumerable<BattleCharacterController> units)
        {
            int i = 0;
            var targetCursors = _battleUI._targetCursors;
            foreach (var controller in units)
            {
                if (i >= targetCursors.Count)
                    targetCursors.Add((Instantiate(_battleUI.TargetCursorTemplate), controller));
                else
                    targetCursors[i] = (targetCursors[i].cursor, controller);

                var position = controller.transform.position;
                foreach (var renderer in controller.PooledGetInChildren<Renderer>())
                    position.y = Mathf.Max(renderer.bounds.max.y, position.y);

                targetCursors[i].cursor.SetActive(true);
                targetCursors[i++].cursor.transform.SetPositionAndRotation(position, Camera.main!.transform.rotation);
            }

            for (; i < targetCursors.Count; i++)
                targetCursors[i].cursor.SetActive(false);
        }


        public void UpdateRenderingAndSelection()
        {
            _temp.Clear();
            foreach (var item in GetItems())
            {
                _temp.Add(item);
                if (Toggles.TryGetValue(item, out _))
                    continue;

                OnNew(item, out var label, out var subSelections, out var isOn, out var inHostileList);

                RectTransform uiElem;
                var template = inHostileList ? _targetList.HostileTargetTemplate : _targetList.AlliesTargetTemplate;
                uiElem = Instantiate(template, template.transform.parent, false);
                uiElem.gameObject.SetActive(true);
                if (uiElem.GetComponentInChildren<Text>() is { } text && text != null)
                    text.text = label;
                if (uiElem.GetComponentInChildren<TMP_Text>() is { } tmpText && tmpText != null)
                    tmpText.text = label;

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

                int index = 0;
                foreach (string s in subSelections)
                {
                    var uiElem2 = Instantiate(template, uiElem, false);
                    uiElem2.gameObject.SetActive(true);
                    uiElem2.anchorMin = new Vector2(0f, (float)index/subSelections.Length);
                    uiElem2.anchorMax = new Vector2(1f, (float)(index+1)/subSelections.Length);
                    uiElem2.offsetMin = new Vector2(5,5);
                    uiElem2.offsetMax = new Vector2(-5,-5);
                    if (uiElem2.GetComponentInChildren<Text>() is { } text2 && text2 != null)
                        text2.text = s;
                    if (uiElem2.GetComponentInChildren<TMP_Text>() is { } tmpText2 && tmpText2 != null)
                        tmpText2.text = s;
                    if (uiElem2.GetComponent<Image>() is { } img && img != null)
                        img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * 0.1f);
                    var toggle2 = uiElem2.GetComponent<Toggle>();
                    Destroy(toggle2.graphic);
                    Destroy(toggle2);
                    index++;
                }

                if (subSelections.Length > 0)
                    uiElem.offsetMin *= new Vector2(1, subSelections.Length + 1);
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
            foreach (var (cursor, unit) in _battleUI._targetCursors)
            {
                if (cursor.activeSelf == false)
                    return;

                var position = unit.transform.position;
                foreach (var renderer in unit.PooledGetInChildren<Renderer>())
                    position.y = Mathf.Max(renderer.bounds.max.y, position.y);
                cursor.transform.SetPositionAndRotation(position, Camera.main!.transform.rotation);
            }
        }

        protected void Clear()
        {
            var cpy = Toggles.ToArray();
            foreach (var (t, (parent, toggle)) in Toggles)
                Destroy(parent.gameObject);

            Toggles.Clear();

            foreach (var v in cpy)
                OnRemoved(v.Key, true);
            
            foreach (var obj in _battleUI._targetCursors)
                obj.cursor.SetActive(false);
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

    public IDisposableMenu<T> NewMenuOf<T>(string seed)
    {
        return new DisposableMenu<T> { UI = this, Seed = seed };
    }

    class DisposableMenu<T> : IDisposableMenu<T>
    {
        public BattleUIOperation UI;
        public string Seed;

        readonly List<RectTransform> _submenuItems = new();
        readonly TaskCompletionSource<T> _tcs = new();
        Coroutine _cancellation;


        static Dictionary<string, T> _lastSelected = new();

        public Button NewButton(string label, T item, string onHover = null, bool interactable = true)
        {
            var uiElem = Instantiate(UI.SkillTemplate, UI.SubActionSelectionContainer, false);
            uiElem.gameObject.SetActive(true);
            _submenuItems.Add(uiElem);
            if (uiElem.GetComponentInChildren<Text>() is { } text && text)
                text.text = label;
            if (uiElem.GetComponentInChildren<TMP_Text>() is { } tmpText && tmpText)
                tmpText.text = label;

            var button = uiElem.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Clear();
                _tcs.TrySetResult(item);
            });
            button.interactable = interactable;

            if (_lastSelected.TryGetValue(Seed, out var previouslySelected) && previouslySelected.Equals(item))
                button.Select();

            UI.SubActionSelectionContainer.gameObject.SetActive(true); // Enable afterward that way selection trackers can trigger and select this button

            UnityAction<BaseEventData> onHoverOrSelect = _ =>
            {
                if (onHover is not null)
                    UI.TooltipUI.OnPresentNewTooltip?.Invoke(onHover);
                _lastSelected[Seed] = item;
            };
            var onHover1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            var onSelect = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            onHover1.callback.AddListener(onHoverOrSelect);
            onSelect.callback.AddListener(onHoverOrSelect);

            if (button.gameObject.TryGetComponent(out EventTrigger trigger) == false)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            trigger.triggers.Add(onHover1);
            trigger.triggers.Add(onSelect);

            return button;
        }

        public Task<T> SelectedItem()
        {
            _cancellation = UI.StartCoroutine(AwaitCancellation());
            return _tcs.Task;

            IEnumerator AwaitCancellation()
            {
                do
                {
                    if (UI.CancelInput.action.WasPerformedThisFrameUnique())
                    {
                        Clear();
                        _tcs.SetCanceled();
                    }

                    yield return null;
                } while (true);
            }
        }

        void Clear()
        {
            UI.SubActionSelectionContainer.gameObject.SetActive(false);
            foreach (var element in _submenuItems)
                Destroy(element.gameObject);
            _submenuItems.Clear();
            UI.TooltipUI.OnHideTooltip?.Invoke();
            UI.StopCoroutine(_cancellation);
        }
    }
}