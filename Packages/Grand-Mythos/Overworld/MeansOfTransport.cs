using System;
using UnityEngine.AI;
using UnityEngine.Events;

[Serializable]
public class MeansOfTransport
{
    public string PromptLabel = "";
    public NavFlags NavFlags;
    public UnityEvent? OnActivate, OnDeactivate;
}

[System.Flags]
public enum NavFlags : int
{
    All = NavMesh.AllAreas,
    Walkable = 1 << 0,
    NotWalkable = 1 << 1,
    Jump = 1 << 2,
    Sea = 1 << 3,
    Air = 1 << 4,
    Crouch = 1 << 5,
}