using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conditions;
using Effects;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleUIOperation : MonoBehaviour
{
    public BattleStateMachine BattleManagement;

    [Header("Selected Character")]
    [CanBeNull] public BattleCharacterController UnitSelected; // This is who is being referenced in the Command Panel
    public HeroPrefabUIData SelectedUI;

    [Header("UI Info")]
    public GameObject EnemyUIPrefab;

    public List<HeroExtension> HeroData;
    public List<HeroPrefabUIData> HeroUIData;

    public List<CharacterTemplate> EnemyData;
    public List<EnemyPrefabUIData> EnemyUIData;

    [Header("Player Main Buttons")]
    [Required] public Button Attack;
    [Required] public Button Skills;
    [Required] public Button Items;
    [Required] public Button Tactics;
    [Required] public Button Schedule;
    [Required] public Button Discard;

    [Header("Item/Target/Skill Selection")]
    [Required] public RectTransform SelectionContainer;
    [Required] public Button AcceptSelection;

    [ValidateInput(nameof(HasButton), "Must have a button")]
    public RectTransform ItemTemplate;
    [ValidateInput(nameof(HasButton), "Must have a button")]
    public RectTransform SkillTemplate;
    [ValidateInput(nameof(HasToggle), "Must have a toggle")]
    public RectTransform TargetTemplate;

    [Header("Previews")]
    [Required] public UIActionPreview ActionPreviewTemplate;
    [Required] public GameObject TargetCursorTemplate;

    [Required] public DamageText DamageTextTemplate;

    public SerializableHashSet<BattleCharacterController> ProcessedUnits = new();

    PlayerControls _playerControls;
    (Coroutine coroutine, IDisposable disposable)? _runningUIOperation;
    bool _listenerBound;
    Color _initialTacticsColor, _disabledTacticsColor;

    List<GameObject> _targetCursors = new();
    Queue<(float availableAfter, DamageText component)> _damageTextCache = new();
    Tactics _order = new();

    (IAction action, UIActionPreview ui)[] _existingPreviews = Array.Empty<(IAction, UIActionPreview)>();
    PreviewType _previewType;

    bool HasButton(RectTransform val, ref string errorMessage)
    {
        return val != null &&  val.GetComponentInChildren<Button>();
    }

    bool HasToggle(RectTransform val, ref string errorMessage)
    {
        return val != null && val.GetComponentInChildren<Toggle>();
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
        }

        _playerControls = new();
        _playerControls.Enable();
        _playerControls.BattleMap.HeroSwitch.performed += SwitchToNextHero;
        ActionPreviewTemplate.gameObject.SetActive(false);
        AttributeAdd.OnApplied += DamageHandler;
    }

    void OnDisable()
    {
        _playerControls.Disable();
        _playerControls.BattleMap.HeroSwitch.performed -= SwitchToNextHero;
        AttributeAdd.OnApplied -= DamageHandler;
    }

    void DamageHandler(BattleCharacterController target, Attribute attribute, int delta)
    {
        if (attribute != Attribute.Health)
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
        (delta > 0 ? damageText.OnHeal : damageText.OnDamage)?.Invoke(Math.Abs(delta).ToString());
        _damageTextCache.Enqueue((Time.time + damageText.Lifetime, damageText));
    }

    void Update()
    {
        if (UnitSelected == null && BattleManagement.PartyLineup.Count != 0)
            UnitSelected = BattleManagement.PartyLineup[0];

        if (UnitSelected == null)
            return;

        Skills.interactable = UnitSelected.Profile.Skills.Count > 0;
        Items.interactable = UnitSelected.Profile.Inventory.Items().FirstOrDefault(x => x.item is Consumable).item is Consumable;

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
        else // No orders, find the first tactics that can run
        {
            Tactics tacticsPreviewed = null;
            using (BattleManagement.Units.TemporaryCopy(out var unitsCopy))
            {
                foreach (var tactic in UnitSelected.Profile.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, new TargetCollection(unitsCopy), UnitSelected.Context, out _, accountForCost:false))
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

        _order.Actions.BackingArray[^1] = UnitSelected.Profile.BasicAttack;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }
    }

    IEnumerable PresentSkillsUI()
    {
        AGAIN:
        SelectionContainer.gameObject.SetActive(true);

        Skill selectedSkill = null;
        bool cancel = false;
        var objects = new List<RectTransform>();
        try
        {
            bool first = true;
            foreach (var skill in UnitSelected.Profile.Skills)
            {
                var button = CreateButton(skill.name, SkillTemplate, SelectionContainer, objects);
                button.onClick.AddListener(() => selectedSkill = skill);

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

            var back = CreateButton("<<-", SkillTemplate, SelectionContainer, objects);
            back.onClick.AddListener(() => cancel = true);

            while (selectedSkill == null && cancel == false)
            {
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }

            if (cancel)
                yield break;
        }
        finally
        {
            foreach (var o in objects)
                Destroy(o.gameObject);
        }

        if (_order.Actions.Length == 0 || _order.Actions.BackingArray[^1] != null)
            Array.Resize(ref _order.Actions.BackingArray, _order.Actions.BackingArray.Length + 1);

        _order.Actions.BackingArray[^1] = selectedSkill;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }

        if (_order.Condition != null && _order.Actions.CostTotal() < UnitSelected.Profile.ActionChargeMax && _order.Condition != null)
            goto AGAIN;
    }

    IEnumerable PresentItemUI()
    {
        AGAIN:
        SelectionContainer.gameObject.SetActive(true);

        Consumable selectedConsumable = null;
        var cancel = false;
        var objects = new List<RectTransform>();
        try
        {
            bool first = true;
            foreach ((BaseItem item, uint count) in UnitSelected.Profile.Inventory.Items())
            {
                if (item is not Consumable consumable)
                    continue;

                var button = CreateButton(($"{consumable.name} (x{count})"), SkillTemplate, SelectionContainer, objects);
                button.onClick.AddListener(() => selectedConsumable = consumable);

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

            var back = CreateButton("<<-", SkillTemplate, SelectionContainer, objects);
            back.onClick.AddListener(() => cancel = true);

            while (selectedConsumable == null && cancel == false)
            {
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }

            if (cancel)
                yield break;
        }
        finally
        {
            foreach (var o in objects)
                Destroy(o.gameObject);
        }

        if (_order.Actions.Length == 0 || _order.Actions.BackingArray[^1] != null)
            Array.Resize(ref _order.Actions.BackingArray, _order.Actions.BackingArray.Length + 1);

        _order.Actions.BackingArray[^1] = selectedConsumable;

        if (_order.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }

        if (_order.Condition != null && _order.Actions.CostTotal() < UnitSelected.Profile.ActionChargeMax && _order.Condition != null)
            goto AGAIN;
    }

    static Button CreateButton(string name, RectTransform SkillTemplate, RectTransform SelectionContainer, ICollection<RectTransform> objects)
    {
        var uiElem = Instantiate(SkillTemplate, SelectionContainer, false);
        uiElem.gameObject.SetActive(true);
        objects.Add(uiElem);
        if (uiElem.GetComponentInChildren<Text>() is Text text && text != null)
            text.text = name;
        if (uiElem.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
            tmp_text.text = name;

        var button = uiElem.GetComponent<Button>();
        return button;
    }

    void TacticsPressed()
    {
        if (BattleManagement.TacticsDisabled.Contains(UnitSelected))
            BattleManagement.TacticsDisabled.Remove(UnitSelected);
        else
            BattleManagement.TacticsDisabled.Add(UnitSelected);
        UpdateTacticsButtonColor();
        Attack.Select();
    }

    void UpdateTacticsButtonColor()
    {
        var cols = Tactics.colors;
        cols.normalColor = BattleManagement.TacticsDisabled.Contains(UnitSelected) ? _disabledTacticsColor : _initialTacticsColor;
        Tactics.colors = cols;
    }

    IEnumerable PresentTargetSelectionUI()
    {
        SelectionContainer.gameObject.SetActive(true);

        HashSet<BattleCharacterController> selection = new();
        { // Start with everything selected, remove any target that doesn't pass the action target condition
            foreach (var unit in BattleManagement.Units)
                selection.Add(unit);

            Filter(_order.Actions, UnitSelected.Context, selection, out selection);
        }

        AcceptSelection.gameObject.SetActive(true);

        bool submitted = false;

        var objects = new List<RectTransform>();
        UnityAction listener = () => submitted = true;
        bool successfull = false;
        try
        {
            AcceptSelection.onClick.AddListener(listener);

            _order.Condition = ScriptableObject.CreateInstance<ActionCondition>();
            _order.Condition.TargetFilter = new SpecificTargetsCondition { Targets = selection };
            bool first = true;

            if (FindTargetGroups(_order.Actions, UnitSelected.Context, BattleManagement.Units, out var groups)) // Have [Hostiles] [Allies] instead of [Hostile1] [Hostile2] [Hostile3]...
            {
                selection.Clear();
                foreach (var unit in groups[0].units)
                    selection.Add(unit);

                var toggles = new List<Toggle>();
                foreach ((string name, List<BattleCharacterController> units) in groups)
                {
                    var toggle = CreateToggle(name, toggles.Count == 0, ref first);
                    toggle.onValueChanged.AddListener(ToggleGroup);
                    toggles.Add(toggle);

                    void ToggleGroup(bool isOn)
                    {
                        if (isOn == false)
                            return;

                        foreach (var otherToggle in toggles)
                        {
                            if (otherToggle == toggle)
                                continue;
                            otherToggle.isOn = false;
                        }

                        selection.Clear();
                        foreach (var unit in units)
                            selection.Add(unit);
                    }
                }
            }
            else // Fallback to generic handler if no groups could be found
            {
                var ignoreListenerEvent = new HashSet<BattleCharacterController>();
                var toggles = new Dictionary<BattleCharacterController, Toggle>();
                foreach (var unitForThisToggle in BattleManagement.Units.OrderByDescending(x => x.IsHostileTo(UnitSelected)))
                {
                    var toggle = CreateToggle(unitForThisToggle.Profile.Name, selection.Contains(unitForThisToggle), ref first);
                    toggle.onValueChanged.AddListener(UpdateSelection);
                    toggles.Add(unitForThisToggle, toggle);

                    void UpdateSelection(bool addUnitIn)
                    {
                        if (ignoreListenerEvent.Remove(unitForThisToggle))
                            return;

                        if (addUnitIn)
                            selection.Add(unitForThisToggle);
                        else
                            selection.Remove(unitForThisToggle);

                        Filter(_order.Actions, UnitSelected.Context, selection, out var newSelection);

                        if (selection.Count == newSelection.Count)
                            return;

                        // Some of the units were filtered out:
                        if (addUnitIn) // Tried to add this unit ?
                        {
                            if (newSelection.Contains(unitForThisToggle)) // The unit bound to this toggle was added in, some other unit was removed
                            {
                                ReplaceSelection(selection, newSelection, toggles, ignoreListenerEvent);
                            }
                            else // This unit was filtered out
                            {
                                // Try again with just this unit selected
                                Filter(_order.Actions, UnitSelected.Context, new() { unitForThisToggle }, out var newSelectionWithOnlyIt);
                                if (newSelectionWithOnlyIt.Count == 1)
                                    ReplaceSelection(selection, newSelectionWithOnlyIt, toggles, ignoreListenerEvent);
                                // Else, don't add it, this unit is likely not compatible with this action
                            }
                        }
                        else // Tried to remove this unit ? But it removed more units than that one
                        {
                            // Roll the deselection back
                            selection.Add(unitForThisToggle);
                            ignoreListenerEvent.Add(unitForThisToggle);
                            toggle.isOn = true;
                        }
                    }
                }
            }

            while (submitted == false || selection.Count == 0)
            {
                AcceptSelection.interactable = selection.Count > 0;
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }

            successfull = true;
        }
        finally
        {
            if (successfull == false)
                _order.Condition = null;

            foreach (var obj in objects)
                Destroy(obj.gameObject);

            AcceptSelection.onClick.RemoveListener(listener);
            AcceptSelection.gameObject.SetActive(false);
        }

        Toggle CreateToggle(string label, bool defaultState, ref bool first)
        {
            var uiElem = Instantiate(TargetTemplate, SelectionContainer, false);
            uiElem.gameObject.SetActive(true);
            objects.Add(uiElem);
            if (uiElem.GetComponentInChildren<Text>() is Text text && text != null)
                text.text = label;
            if (uiElem.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
                tmp_text.text = label;

            var toggle = uiElem.GetComponent<Toggle>();
            toggle.isOn = defaultState;

            if (first)
                toggle.Select();
            first = false;
            return toggle;
        }


        static void ReplaceSelection(HashSet<BattleCharacterController> write, HashSet<BattleCharacterController> read, Dictionary<BattleCharacterController, Toggle> toggles, HashSet<BattleCharacterController> validationUpdates)
        {
            // Remove those that are not part of this new selection, and update their toggles
            foreach (var unit in write)
            {
                if (read.Contains(unit) == false)
                {
                    validationUpdates.Add(unit);
                    toggles[unit].isOn = false;
                }
            }
            // Copy new into current
            write.Clear();
            foreach (var unit in read)
                write.Add(unit);
        }

        static void Filter(IActionCollection actions, EvaluationContext context, HashSet<BattleCharacterController> input, out HashSet<BattleCharacterController> output)
        {
            output = new();
            foreach (var template in input)
                output.Add(template);

            foreach (var action in actions)
            {
                if (action.TargetFilter == null)
                    continue;

                TargetCollection collection = new(output.ToList());
                action.TargetFilter.Filter(ref collection, context);
                if (collection.CountSlow() != output.Count)
                {
                    output.Clear();
                    foreach (var target in collection)
                        output.Add(target);
                }
            }
        }

        static bool FindTargetGroups(IActionCollection actions, EvaluationContext context, List<BattleCharacterController> units, out List<(string name, List<BattleCharacterController> units)> groups)
        {
            groups = new()
            {
                ("Hostiles", units.Where(x => x.IsHostileTo(context.Controller)).ToList()),
                ("Allies", units.Where(x => x.IsHostileTo(context.Controller) == false).ToList())
            };
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                var newTargets = new TargetCollection(groups[i].units);
                foreach (var action in actions)
                    action.TargetFilter?.Filter(ref newTargets, context);

                var count = newTargets.CountSlow();
                if (count == 0 || count == 1 && groups[i].units.Count != 1)
                {
                    groups.RemoveAt(i);
                }
                else if (count == 1)
                {
                    var target = newTargets.First();
                    groups[i] = (target.name, new(){ target });
                }
                else
                {
                    groups[i] = (groups[i].name, newTargets.ToList());
                }
            }

            return groups.Count > 0;
        }
    }

    void SwitchToNextHero(InputAction.CallbackContext context)
    {
        float dir = context.ReadValue<float>();
        int sign = dir < 0 ? -1 : 1;

        int partyCount = BattleManagement.PartyLineup.Count;
        int indexOfOldSelection = BattleManagement.PartyLineup.IndexOf(UnitSelected);
        // Search for the next unit forward or backwards in the list
        for (int k = Mod(indexOfOldSelection + sign, partyCount); k != indexOfOldSelection; k = Mod(k + sign, partyCount))
        {
            if (BattleManagement.PartyLineup[k].Profile.CurrentHP > 0)
            {
                UnitSelected = BattleManagement.PartyLineup[k];
                UpdateTacticsButtonColor();
                break;
            }
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
        BattleManagement.Orders[UnitSelected] = tactics;
        _order = new();
        ResetNavigation();
        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
        Attack.Select();
    }

    void CancelFullOrder()
    {
        _order = new();
        ResetNavigation();
        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
        Attack.Select();
    }

    void ResetNavigation()
    {
        if (_runningUIOperation is {} val)
        {
            StopCoroutine(val.coroutine);
            val.disposable.Dispose();
            _runningUIOperation = null;
        }

        Attack.gameObject.SetActive(true);
        Skills.gameObject.SetActive(true);
        Items.gameObject.SetActive(true);
        Tactics.gameObject.SetActive(true);

        Schedule.gameObject.SetActive(false);
        Discard.gameObject.SetActive(false);
        ItemTemplate.gameObject.SetActive(false);
        SkillTemplate.gameObject.SetActive(false);
        TargetTemplate.gameObject.SetActive(false);
        SelectionContainer.gameObject.SetActive(false);
        AcceptSelection.gameObject.SetActive(false);

        if (_order.Actions.Length > 0 && _order.Condition != null)
        {
            Schedule.gameObject.SetActive(true);
            Discard.gameObject.SetActive(true);
        }
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
                Attack.gameObject.SetActive(false);
                Skills.gameObject.SetActive(false);
                Items.gameObject.SetActive(false);
                Tactics.gameObject.SetActive(false);

                foreach (var yield in inner)
                    yield return yield;
            }
            finally
            {
                _runningUIOperation = null;

                ResetNavigation();
                Schedule.Select();

                if (_order.Actions.Length == 0) // Order was cancelled, remove blocker
                    BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
                if (_order.Actions.CostTotal() >= 4 && _order.Condition != null)
                    ScheduleOrder(_order);
            }
        }
    }


    public void UpdatePreview([CanBeNull] Tactics tactics, ReadOnlySpan<IAction> actionsSubset, PreviewType type)
    {
        using var _temp = BattleManagement.Units.TemporaryCopy(out var _unitsCopy);
        if (tactics != null && tactics.Condition != null && tactics.Condition.CanExecute(tactics.Actions, new TargetCollection(_unitsCopy), UnitSelected.Context, out var selection, accountForCost:false))
        {
            int i = 0;
            foreach (var controller in selection)
            {
                if (i >= _targetCursors.Count)
                    _targetCursors.Add(Instantiate(TargetCursorTemplate));

                var position = controller.transform.position;
                foreach (var renderer in controller.PooledGetInChildren<Renderer>())
                    position.y = Mathf.Max(renderer.bounds.max.y, position.y);

                _targetCursors[i++].transform.SetPositionAndRotation(position, Camera.main.transform.rotation);
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
            return;

        foreach (var (action, ui) in toDiscard)
            ui.Discarded?.Invoke();

        if (rightMatches > leftMatches)
        {
            for (int i = 0; i < toKeep.Length; i++)
            {
                var (action, ui) = toKeep[i];
                int newPosition = toAdd.Length + i;
                ui.Moved?.Invoke();
            }
        }

        _existingPreviews = new (IAction action, UIActionPreview ui)[toKeep.Length + toAdd.Length];
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
            newAsSpanAddRange[i] = (toAdd[i], ui);
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

    class SpecificTargetsCondition : SimplifiedCondition
    {
        public HashSet<BattleCharacterController> Targets = new();
        public override string UIDisplayText => "Specific Targets";

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){ }

        protected override bool Filter(BattleCharacterController target, EvaluationContext context) => Targets.Contains(target);
    }
}