﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conditions;
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
    public HeroExtension UnitSelected; // This is who is being referenced in the Command Panel
    public HeroPrefabUIData SelectedUI;

    [Header("UI Info")]
    public GameObject enemyUIPrefab;

    public List<HeroExtension> heroData;
    public List<HeroPrefabUIData> heroUIData;

    public List<CharacterTemplate> enemyData;
    public List<EnemyPrefabUIData> enemyUIData;

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

    [Required]
    [ValidateInput(nameof(HasButton), "Must have a button")]
    public RectTransform ItemTemplate;
    [Required]
    [ValidateInput(nameof(HasButton), "Must have a button")]
    public RectTransform SkillTemplate;
    [Required]
    [ValidateInput(nameof(HasToggle), "Must have a toggle")]
    public RectTransform TargetTemplate;

    [Header("Actions display")]
    [Required]
    public RectTransform ActionContainer;
    [Required]
    [ValidateInput(nameof(HasTextComponent), "Must have a text component (TMP or Unity)")]
    public RectTransform ActionTemplate;

    public SerializableHashSet<CharacterTemplate> ProcessedUnits = new();

    Coroutine _runningUIOperation;
    bool _listenerBound;
    PlayerControls _playerControls;
    Color _initialTacticsColor, _disabledTacticsColor;

    List<CharacterTemplate> _unitsCopy = new();
    Tactics _tempTactics = new();
    List<IAction> _currentActionHints = new();
    List<RectTransform> _currentActionHintsUI = new();

    bool HasButton(RectTransform val, ref string errorMessage)
    {
        return val != null &&  val.GetComponentInChildren<Button>();
    }

    bool HasToggle(RectTransform val, ref string errorMessage)
    {
        return val != null && val.GetComponentInChildren<Toggle>();
    }

    bool HasTextComponent(RectTransform val, ref string errorMessage)
    {
        return val != null && (val.GetComponentInChildren<Text>() || val.GetComponentInChildren<TMP_Text>());
    }

    void OnEnable()
    {
        ActionTemplate.gameObject.SetActive(false);
        ResetNavigation();
        if (_listenerBound == false)
        {
            _listenerBound = true;
            _initialTacticsColor = Tactics.colors.normalColor;
            _disabledTacticsColor = Tactics.colors.disabledColor;
            UpdateTacticsButtonColor();
            Attack.onClick.AddListener(() => TryOrderWizard(PresentAttackUI().GetEnumerator()));
            Skills.onClick.AddListener(() => TryOrderWizard(PresentSkillsUI().GetEnumerator()));
            Items.onClick.AddListener(() => TryOrderWizard(PresentItemUI().GetEnumerator()));
            Tactics.onClick.AddListener(TacticsPressed);

            Schedule.onClick.AddListener(() => ScheduleOrder(_tempTactics));
            Discard.onClick.AddListener(CancelFullOrder);
        }

        _playerControls = new();
        _playerControls.Enable();
        _playerControls.BattleMap.HeroSwitch.performed += SwitchToNextHero;
        UnitSelected = BattleManagement.PartyLineup[0];
    }

    void OnDisable()
    {
        ResetNavigation();
        _playerControls.Disable();
        _playerControls.BattleMap.HeroSwitch.performed -= SwitchToNextHero;
    }

    void Update()
    {
        Skills.interactable = UnitSelected.Skills.Count > 0;
        Items.interactable = UnitSelected.Inventory.Items().FirstOrDefault(x => x.item is Consumable).item is Consumable;

        Tactics tacticsPreviewed = _tempTactics;
        if (_tempTactics.Actions.Length == 0) // Show preview of tactics chosen by the unit if no orders are provided
        {
            _unitsCopy.AddRange(BattleManagement.Units);
            foreach (var tactic in UnitSelected.Tactics)
            {
                if (tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, new TargetCollection(_unitsCopy), UnitSelected.Context, out _, accountForCost:false))
                {
                    tacticsPreviewed = tactic;
                    break;
                }
            }
            _unitsCopy.Clear();
        }

        if (tacticsPreviewed != null && _currentActionHints.SequenceEqual(tacticsPreviewed.Actions.BackingArray) == false)
        {
            bool overriden = _tempTactics.Actions.Length != 0;
            _currentActionHints.Clear();
            _currentActionHints.AddRange(tacticsPreviewed.Actions.BackingArray);

            foreach (var rectTransform in _currentActionHintsUI)
                Destroy(rectTransform.gameObject);
            _currentActionHintsUI.Clear();
            foreach (var action in _currentActionHints)
            {
                var actionUI = Instantiate(ActionTemplate, ActionContainer, false);
                actionUI.gameObject.SetActive(true);
                actionUI.sizeDelta = new Vector2(actionUI.sizeDelta.x * action.ATBCost, actionUI.sizeDelta.y);
                if (actionUI.GetComponentInChildren<Text>() is Text text && text != null)
                    text.text = action.Name;
                if (actionUI.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
                    tmp_text.text = action.Name;
                if (overriden == false && actionUI.GetComponentInChildren<Image>() is Image image)
                    image.color *= 0.5f;

                _currentActionHintsUI.Add(actionUI);
            }
        }

        foreach (var unit in BattleManagement.Units)
        {
            if (!ProcessedUnits.Add(unit))
                continue;

            if (unit is HeroExtension hero)
            {
                heroData.Add(hero);
            }
            else
            {
                enemyData.Add(unit);

                var renderer = unit.transform.GetComponentInChildren<Renderer>();
                var rendererTransform = renderer.transform;
                var enemyUI = Instantiate(enemyUIPrefab);
                var rectTransform = enemyUI.GetComponent<RectTransform>();
                enemyUI.transform.position = new Vector3(rendererTransform.position.x, renderer.bounds.max.y + rectTransform.sizeDelta.y, rendererTransform.position.z);
                rectTransform.SetParent(rendererTransform, true);
                enemyUI.name = $"{unit.gameObject.name} UI";

                EnemyPrefabUIData data = enemyUI.GetComponent<EnemyPrefabUIData>();
                data.identity.text = unit.name;
                data.healthBar.fillAmount = unit.EffectiveStats.HP;

                enemyUIData.Add(data);
            }
        }

        SelectedUI.characterIcon.sprite = UnitSelected.Portrait;
        SelectedUI.atbBar.fillAmount = UnitSelected.ActionsCharged / UnitSelected.ActionChargeMax;
        SelectedUI.healthBar.fillAmount = (float)UnitSelected.CurrentHP / UnitSelected.EffectiveStats.HP;
        SelectedUI.health.text = UnitSelected.CurrentHP.ToString();
        int j = 0;
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i] != UnitSelected)
            {
                heroUIData[j].gameObject.SetActive(true);
                heroUIData[j].characterIcon.sprite = heroData[i].Portrait;
                heroUIData[j].atbBar.fillAmount = heroData[i].ActionsCharged / heroData[i].ActionChargeMax;
                heroUIData[j].healthBar.fillAmount = (float)heroData[i].CurrentHP / heroData[i].EffectiveStats.HP;
                heroUIData[j].health.text = heroData[i].CurrentHP.ToString();
                j++;
            }
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyUIData[i].healthBar.fillAmount = (float)enemyData[i].CurrentHP / enemyData[i].EffectiveStats.HP;
            enemyUIData[i].health.text = enemyData[i].CurrentHP.ToString();
        }
    }

    IEnumerable PresentAttackUI()
    {
        if (_tempTactics.Actions.Length == 0 || _tempTactics.Actions.BackingArray[^1] != null)
            Array.Resize(ref _tempTactics.Actions.BackingArray, _tempTactics.Actions.BackingArray.Length + 1);

        _tempTactics.Actions.BackingArray[^1] = UnitSelected.BasicAttack;

        if (_tempTactics.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }
    }

    IEnumerable PresentSkillsUI()
    {
        SelectionContainer.gameObject.SetActive(true);

        Skill selectedSkill = null;
        var objects = new List<RectTransform>();
        try
        {
            bool first = true;
            foreach (var skill in UnitSelected.Skills)
            {
                var uiElem = Instantiate(SkillTemplate, SelectionContainer, false);
                uiElem.gameObject.SetActive(true);
                objects.Add(uiElem);
                if (uiElem.GetComponentInChildren<Text>() is Text text && text != null)
                    text.text = skill.name;
                if (uiElem.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
                    tmp_text.text = skill.name;

                var button = uiElem.GetComponent<Button>();
                button.onClick.AddListener(() => selectedSkill = skill);

                if (_tempTactics.Actions.CostTotal() + skill.ATBCost > UnitSelected.ActionChargeMax)
                {
                    button.interactable = false;
                }
                else if (first)
                {
                    button.Select();
                    first = false;
                }
            }

            while (selectedSkill == null)
            {
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }
        }
        finally
        {
            foreach (var o in objects)
                Destroy(o.gameObject);
        }

        if (_tempTactics.Actions.Length == 0 || _tempTactics.Actions.BackingArray[^1] != null)
            Array.Resize(ref _tempTactics.Actions.BackingArray, _tempTactics.Actions.BackingArray.Length + 1);

        _tempTactics.Actions.BackingArray[^1] = selectedSkill;

        if (_tempTactics.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }
    }

    IEnumerable PresentItemUI()
    {
        SelectionContainer.gameObject.SetActive(true);

        Consumable selectedSkill = null;
        var objects = new List<RectTransform>();
        try
        {
            bool first = true;
            foreach ((BaseItem item, uint count) in UnitSelected.Inventory.Items())
            {
                if (item is not Consumable consumable)
                    continue;

                var uiElem = Instantiate(SkillTemplate, SelectionContainer, false);
                uiElem.gameObject.SetActive(true);
                objects.Add(uiElem);
                if (uiElem.GetComponentInChildren<Text>() is Text text && text != null)
                    text.text = consumable.name;
                if (uiElem.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
                    tmp_text.text = consumable.name;

                var button = uiElem.GetComponent<Button>();
                button.onClick.AddListener(() => selectedSkill = consumable);

                if (_tempTactics.Actions.CostTotal() + consumable.ATBCost > UnitSelected.ActionChargeMax)
                {
                    button.interactable = false;
                }
                else if (first)
                {
                    button.Select();
                    first = false;
                }
            }

            while (selectedSkill == null)
            {
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }
        }
        finally
        {
            foreach (var o in objects)
                Destroy(o.gameObject);
        }

        if (_tempTactics.Actions.Length == 0 || _tempTactics.Actions.BackingArray[^1] != null)
            Array.Resize(ref _tempTactics.Actions.BackingArray, _tempTactics.Actions.BackingArray.Length + 1);

        _tempTactics.Actions.BackingArray[^1] = selectedSkill;

        if (_tempTactics.Condition == null)
        {
            foreach (var yields in PresentTargetSelectionUI())
                yield return yields;
        }
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

        HashSet<CharacterTemplate> selection = new();
        { // Start with everything selected, remove any target that doesn't pass the action target condition
            foreach (var unit in BattleManagement.Units)
                selection.Add(unit);

            Filter(_tempTactics.Actions, UnitSelected.Context, selection, out selection);
        }

        AcceptSelection.gameObject.SetActive(true);

        bool submitted = false;

        var objects = new List<RectTransform>();
        UnityAction listener = () => submitted = true;
        try
        {
            AcceptSelection.onClick.AddListener(listener);

            var toggles = new Dictionary<CharacterTemplate, Toggle>();
            var ignoreListenerEvent = new HashSet<CharacterTemplate>();
            bool first = true;
            foreach (var unitForThisToggle in BattleManagement.Units)
            {
                var uiElem = Instantiate(TargetTemplate, SelectionContainer, false);
                uiElem.gameObject.SetActive(true);
                objects.Add(uiElem);
                if (uiElem.GetComponentInChildren<Text>() is Text text && text != null)
                    text.text = unitForThisToggle.name;
                if (uiElem.GetComponentInChildren<TMP_Text>() is TMP_Text tmp_text && tmp_text != null)
                    tmp_text.text = unitForThisToggle.name;

                var toggle = uiElem.GetComponent<Toggle>();
                toggle.isOn = selection.Contains(unitForThisToggle);
                toggle.onValueChanged.AddListener(UpdateSelection);
                toggles.Add(unitForThisToggle, toggle);

                if (first)
                    toggle.Select();
                first = false;

                void UpdateSelection(bool addUnitIn)
                {
                    if (ignoreListenerEvent.Remove(unitForThisToggle))
                        return;

                    if (addUnitIn)
                        selection.Add(unitForThisToggle);
                    else
                        selection.Remove(unitForThisToggle);

                    Filter(_tempTactics.Actions, UnitSelected.Context, selection, out var newSelection);

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
                            Filter(_tempTactics.Actions, UnitSelected.Context, new(){ unitForThisToggle }, out var newSelectionWithOnlyIt);
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

            while (submitted == false || selection.Count == 0)
            {
                AcceptSelection.interactable = selection.Count > 0;
                if (Input.GetKey(KeyCode.Escape))
                    yield break;
                yield return null;
            }

            _tempTactics.Condition = ScriptableObject.CreateInstance<ActionCondition>();
            _tempTactics.Condition.TargetFilter = new SpecificTargetsCondition { Targets = selection };
        }
        finally
        {
            foreach (var obj in objects)
                Destroy(obj.gameObject);

            AcceptSelection.onClick.RemoveListener(listener);
            AcceptSelection.gameObject.SetActive(false);
        }


        static void ReplaceSelection(HashSet<CharacterTemplate> write, HashSet<CharacterTemplate> read, Dictionary<CharacterTemplate, Toggle> toggles, HashSet<CharacterTemplate> validationUpdates)
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

        static void Filter(IActionCollection actions, EvaluationContext context, HashSet<CharacterTemplate> input, out HashSet<CharacterTemplate> output)
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
            if (BattleManagement.PartyLineup[k].CurrentHP > 0)
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
        BattleManagement.SetOrderFor(UnitSelected, tactics);
        _tempTactics = new();
        ResetNavigation();
        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
        Attack.Select();
    }

    void CancelFullOrder()
    {
        _tempTactics = new();
        ResetNavigation();
        BattleManagement.Blocked &= ~BlockBattleFlags.PreparingOrders;
        Attack.Select();
    }

    void ResetNavigation()
    {
        if (_runningUIOperation != null)
        {
            StopCoroutine(_runningUIOperation);
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

        if (_tempTactics.Actions.Length > 0 && _tempTactics.Condition != null)
        {
            Schedule.gameObject.SetActive(true);
            Discard.gameObject.SetActive(true);
        }
    }

    bool TryOrderWizard(IEnumerator inner)
    {
        if (_runningUIOperation != null)
            return false;

        _runningUIOperation = StartCoroutine(BattleUICoroutine(inner));

        return true;

        IEnumerator BattleUICoroutine(IEnumerator inner)
        {
            BattleManagement.Blocked |= BlockBattleFlags.PreparingOrders;
            try
            {
                Attack.gameObject.SetActive(false);
                Skills.gameObject.SetActive(false);
                Items.gameObject.SetActive(false);
                Tactics.gameObject.SetActive(false);

                while (inner.MoveNext())
                    yield return inner.Current;
            }
            finally
            {
                _runningUIOperation = null;

                ResetNavigation();
                Schedule.Select();

                if (_tempTactics.Actions.CostTotal() >= 4 && _tempTactics.Condition != null)
                    ScheduleOrder(_tempTactics);
            }
        }
    }

    class SpecificTargetsCondition : SimplifiedCondition
    {
        public HashSet<CharacterTemplate> Targets = new();
        public override string UIDisplayText => "Specific Targets";

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){ }

        protected override bool Filter(CharacterTemplate target, EvaluationContext context) => Targets.Contains(target);
    }
}