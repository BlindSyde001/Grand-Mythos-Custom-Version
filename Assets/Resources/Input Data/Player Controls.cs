//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Resources/Input Data/Player Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Player Controls"",
    ""maps"": [
        {
            ""name"": ""Overworld Map"",
            ""id"": ""e1aceec9-fbdb-4312-a608-f4eaece73d9f"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""f88d1f6b-48e7-4916-8835-da6bcbe760f1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""1152e6e8-5dbf-4671-aa51-ceaabcb1c5be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OpenMenu"",
                    ""type"": ""Button"",
                    ""id"": ""9b5ed792-86ec-4433-9a11-73c0dbc3aac7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WSAD"",
                    ""id"": ""499aca77-16ae-4901-a9c7-a1142f195f42"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""2eb0d1d0-0350-4795-b1f5-e8e73c644789"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""63924652-df58-4655-911f-20f00ea40754"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""6eff89a3-a93b-436c-aacb-bd1533e1f64c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""5c08304f-a429-49aa-aff2-432f45013f87"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrow Keys"",
                    ""id"": ""adecd1ad-dcb2-4f58-a27d-84dae3c58946"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""291b5c62-783b-45b5-9092-9b88a0e943a0"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""e136bf9d-65ec-4de3-9466-5c9c65a73f1b"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""f1d83629-cb3d-467d-a5ef-889dcd4ed85b"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""c9bb574b-48c1-482a-9025-292baaeace46"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""8ab38407-6a8d-4257-b395-1fa856686f56"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68b3ef7b-f4d1-474f-a8c1-a796ec994c33"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""OpenMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu Map"",
            ""id"": ""ecbcd905-2db8-4b61-bf14-fad53bada67c"",
            ""actions"": [
                {
                    ""name"": ""CloseMenu"",
                    ""type"": ""Button"",
                    ""id"": ""b8522af9-72c9-4bea-8add-ccec21a6d4dc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CloseCurrentMenuCategory"",
                    ""type"": ""Button"",
                    ""id"": ""402fb325-9e75-49d2-ac96-cfcbdb832d75"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""174833e3-d0d1-4225-8f66-702bdfd48dc2"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""CloseMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b7161edc-68e1-46bf-b810-fb8de460f20e"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""CloseCurrentMenuCategory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Title Map"",
            ""id"": ""e75cffd3-ef1f-4bda-bdeb-1ad167a377df"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""64e96c88-a777-4f51-96b1-f7ddd2342c33"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3ae4ee05-bc17-44f2-9101-5fab1f3edc6b"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Battle Map"",
            ""id"": ""416f6706-9cf7-473b-9aa7-d462612bb75f"",
            ""actions"": [
                {
                    ""name"": ""HeroSwitch"",
                    ""type"": ""Button"",
                    ""id"": ""fad21261-8d1e-4889-8159-3f0dccb63eaf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""1873ee7f-7b22-4007-aaf0-a52894efd963"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HeroSwitch"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""0b980e67-0ef4-4f22-a2ea-c29d649d1edd"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""HeroSwitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""9354074e-d732-4bda-a4c3-0b9238614370"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC Controls"",
                    ""action"": ""HeroSwitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Cutscene Map"",
            ""id"": ""378c8a2a-4da2-48e3-8d8b-6efbcad66d3e"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""dfc744b8-eea8-45fe-ade8-00098529060a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b4251d27-88a3-40a6-80d8-4fbd00e3d0b9"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""PC Controls"",
            ""bindingGroup"": ""PC Controls"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Overworld Map
        m_OverworldMap = asset.FindActionMap("Overworld Map", throwIfNotFound: true);
        m_OverworldMap_Move = m_OverworldMap.FindAction("Move", throwIfNotFound: true);
        m_OverworldMap_Interact = m_OverworldMap.FindAction("Interact", throwIfNotFound: true);
        m_OverworldMap_OpenMenu = m_OverworldMap.FindAction("OpenMenu", throwIfNotFound: true);
        // Menu Map
        m_MenuMap = asset.FindActionMap("Menu Map", throwIfNotFound: true);
        m_MenuMap_CloseMenu = m_MenuMap.FindAction("CloseMenu", throwIfNotFound: true);
        m_MenuMap_CloseCurrentMenuCategory = m_MenuMap.FindAction("CloseCurrentMenuCategory", throwIfNotFound: true);
        // Title Map
        m_TitleMap = asset.FindActionMap("Title Map", throwIfNotFound: true);
        m_TitleMap_Newaction = m_TitleMap.FindAction("New action", throwIfNotFound: true);
        // Battle Map
        m_BattleMap = asset.FindActionMap("Battle Map", throwIfNotFound: true);
        m_BattleMap_HeroSwitch = m_BattleMap.FindAction("HeroSwitch", throwIfNotFound: true);
        // Cutscene Map
        m_CutsceneMap = asset.FindActionMap("Cutscene Map", throwIfNotFound: true);
        m_CutsceneMap_Newaction = m_CutsceneMap.FindAction("New action", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Overworld Map
    private readonly InputActionMap m_OverworldMap;
    private List<IOverworldMapActions> m_OverworldMapActionsCallbackInterfaces = new List<IOverworldMapActions>();
    private readonly InputAction m_OverworldMap_Move;
    private readonly InputAction m_OverworldMap_Interact;
    private readonly InputAction m_OverworldMap_OpenMenu;
    public struct OverworldMapActions
    {
        private @PlayerControls m_Wrapper;
        public OverworldMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_OverworldMap_Move;
        public InputAction @Interact => m_Wrapper.m_OverworldMap_Interact;
        public InputAction @OpenMenu => m_Wrapper.m_OverworldMap_OpenMenu;
        public InputActionMap Get() { return m_Wrapper.m_OverworldMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(OverworldMapActions set) { return set.Get(); }
        public void AddCallbacks(IOverworldMapActions instance)
        {
            if (instance == null || m_Wrapper.m_OverworldMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_OverworldMapActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Interact.started += instance.OnInteract;
            @Interact.performed += instance.OnInteract;
            @Interact.canceled += instance.OnInteract;
            @OpenMenu.started += instance.OnOpenMenu;
            @OpenMenu.performed += instance.OnOpenMenu;
            @OpenMenu.canceled += instance.OnOpenMenu;
        }

        private void UnregisterCallbacks(IOverworldMapActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Interact.started -= instance.OnInteract;
            @Interact.performed -= instance.OnInteract;
            @Interact.canceled -= instance.OnInteract;
            @OpenMenu.started -= instance.OnOpenMenu;
            @OpenMenu.performed -= instance.OnOpenMenu;
            @OpenMenu.canceled -= instance.OnOpenMenu;
        }

        public void RemoveCallbacks(IOverworldMapActions instance)
        {
            if (m_Wrapper.m_OverworldMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IOverworldMapActions instance)
        {
            foreach (var item in m_Wrapper.m_OverworldMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_OverworldMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public OverworldMapActions @OverworldMap => new OverworldMapActions(this);

    // Menu Map
    private readonly InputActionMap m_MenuMap;
    private List<IMenuMapActions> m_MenuMapActionsCallbackInterfaces = new List<IMenuMapActions>();
    private readonly InputAction m_MenuMap_CloseMenu;
    private readonly InputAction m_MenuMap_CloseCurrentMenuCategory;
    public struct MenuMapActions
    {
        private @PlayerControls m_Wrapper;
        public MenuMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CloseMenu => m_Wrapper.m_MenuMap_CloseMenu;
        public InputAction @CloseCurrentMenuCategory => m_Wrapper.m_MenuMap_CloseCurrentMenuCategory;
        public InputActionMap Get() { return m_Wrapper.m_MenuMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuMapActions set) { return set.Get(); }
        public void AddCallbacks(IMenuMapActions instance)
        {
            if (instance == null || m_Wrapper.m_MenuMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_MenuMapActionsCallbackInterfaces.Add(instance);
            @CloseMenu.started += instance.OnCloseMenu;
            @CloseMenu.performed += instance.OnCloseMenu;
            @CloseMenu.canceled += instance.OnCloseMenu;
            @CloseCurrentMenuCategory.started += instance.OnCloseCurrentMenuCategory;
            @CloseCurrentMenuCategory.performed += instance.OnCloseCurrentMenuCategory;
            @CloseCurrentMenuCategory.canceled += instance.OnCloseCurrentMenuCategory;
        }

        private void UnregisterCallbacks(IMenuMapActions instance)
        {
            @CloseMenu.started -= instance.OnCloseMenu;
            @CloseMenu.performed -= instance.OnCloseMenu;
            @CloseMenu.canceled -= instance.OnCloseMenu;
            @CloseCurrentMenuCategory.started -= instance.OnCloseCurrentMenuCategory;
            @CloseCurrentMenuCategory.performed -= instance.OnCloseCurrentMenuCategory;
            @CloseCurrentMenuCategory.canceled -= instance.OnCloseCurrentMenuCategory;
        }

        public void RemoveCallbacks(IMenuMapActions instance)
        {
            if (m_Wrapper.m_MenuMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IMenuMapActions instance)
        {
            foreach (var item in m_Wrapper.m_MenuMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_MenuMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public MenuMapActions @MenuMap => new MenuMapActions(this);

    // Title Map
    private readonly InputActionMap m_TitleMap;
    private List<ITitleMapActions> m_TitleMapActionsCallbackInterfaces = new List<ITitleMapActions>();
    private readonly InputAction m_TitleMap_Newaction;
    public struct TitleMapActions
    {
        private @PlayerControls m_Wrapper;
        public TitleMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_TitleMap_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_TitleMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TitleMapActions set) { return set.Get(); }
        public void AddCallbacks(ITitleMapActions instance)
        {
            if (instance == null || m_Wrapper.m_TitleMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_TitleMapActionsCallbackInterfaces.Add(instance);
            @Newaction.started += instance.OnNewaction;
            @Newaction.performed += instance.OnNewaction;
            @Newaction.canceled += instance.OnNewaction;
        }

        private void UnregisterCallbacks(ITitleMapActions instance)
        {
            @Newaction.started -= instance.OnNewaction;
            @Newaction.performed -= instance.OnNewaction;
            @Newaction.canceled -= instance.OnNewaction;
        }

        public void RemoveCallbacks(ITitleMapActions instance)
        {
            if (m_Wrapper.m_TitleMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ITitleMapActions instance)
        {
            foreach (var item in m_Wrapper.m_TitleMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_TitleMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public TitleMapActions @TitleMap => new TitleMapActions(this);

    // Battle Map
    private readonly InputActionMap m_BattleMap;
    private List<IBattleMapActions> m_BattleMapActionsCallbackInterfaces = new List<IBattleMapActions>();
    private readonly InputAction m_BattleMap_HeroSwitch;
    public struct BattleMapActions
    {
        private @PlayerControls m_Wrapper;
        public BattleMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @HeroSwitch => m_Wrapper.m_BattleMap_HeroSwitch;
        public InputActionMap Get() { return m_Wrapper.m_BattleMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BattleMapActions set) { return set.Get(); }
        public void AddCallbacks(IBattleMapActions instance)
        {
            if (instance == null || m_Wrapper.m_BattleMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_BattleMapActionsCallbackInterfaces.Add(instance);
            @HeroSwitch.started += instance.OnHeroSwitch;
            @HeroSwitch.performed += instance.OnHeroSwitch;
            @HeroSwitch.canceled += instance.OnHeroSwitch;
        }

        private void UnregisterCallbacks(IBattleMapActions instance)
        {
            @HeroSwitch.started -= instance.OnHeroSwitch;
            @HeroSwitch.performed -= instance.OnHeroSwitch;
            @HeroSwitch.canceled -= instance.OnHeroSwitch;
        }

        public void RemoveCallbacks(IBattleMapActions instance)
        {
            if (m_Wrapper.m_BattleMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IBattleMapActions instance)
        {
            foreach (var item in m_Wrapper.m_BattleMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_BattleMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public BattleMapActions @BattleMap => new BattleMapActions(this);

    // Cutscene Map
    private readonly InputActionMap m_CutsceneMap;
    private List<ICutsceneMapActions> m_CutsceneMapActionsCallbackInterfaces = new List<ICutsceneMapActions>();
    private readonly InputAction m_CutsceneMap_Newaction;
    public struct CutsceneMapActions
    {
        private @PlayerControls m_Wrapper;
        public CutsceneMapActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_CutsceneMap_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_CutsceneMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CutsceneMapActions set) { return set.Get(); }
        public void AddCallbacks(ICutsceneMapActions instance)
        {
            if (instance == null || m_Wrapper.m_CutsceneMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_CutsceneMapActionsCallbackInterfaces.Add(instance);
            @Newaction.started += instance.OnNewaction;
            @Newaction.performed += instance.OnNewaction;
            @Newaction.canceled += instance.OnNewaction;
        }

        private void UnregisterCallbacks(ICutsceneMapActions instance)
        {
            @Newaction.started -= instance.OnNewaction;
            @Newaction.performed -= instance.OnNewaction;
            @Newaction.canceled -= instance.OnNewaction;
        }

        public void RemoveCallbacks(ICutsceneMapActions instance)
        {
            if (m_Wrapper.m_CutsceneMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ICutsceneMapActions instance)
        {
            foreach (var item in m_Wrapper.m_CutsceneMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_CutsceneMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public CutsceneMapActions @CutsceneMap => new CutsceneMapActions(this);
    private int m_PCControlsSchemeIndex = -1;
    public InputControlScheme PCControlsScheme
    {
        get
        {
            if (m_PCControlsSchemeIndex == -1) m_PCControlsSchemeIndex = asset.FindControlSchemeIndex("PC Controls");
            return asset.controlSchemes[m_PCControlsSchemeIndex];
        }
    }
    public interface IOverworldMapActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnOpenMenu(InputAction.CallbackContext context);
    }
    public interface IMenuMapActions
    {
        void OnCloseMenu(InputAction.CallbackContext context);
        void OnCloseCurrentMenuCategory(InputAction.CallbackContext context);
    }
    public interface ITitleMapActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
    public interface IBattleMapActions
    {
        void OnHeroSwitch(InputAction.CallbackContext context);
    }
    public interface ICutsceneMapActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
}