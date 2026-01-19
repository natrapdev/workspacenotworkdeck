using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class StateData : Node
{
    private readonly Dictionary<string, float> _staminaCosts = new()
    {
        {"walk", 0.012f},
        {"jump", 33f},
        {"attack", 0.8f}
    };

    private readonly Dictionary<string, float> _fatigueCosts = new()
    {
        {"walk", 0.000012f},
        {"jump", 0.0033f},
        {"attack", 0.0005f}
    };

    public void AssignStateStatistics(Dictionary<string, CharacterState> states)
    {
        foreach (string stateName in states.Keys.ToArray())
        {
            states[stateName].staminaCost = _staminaCosts[stateName];
            states[stateName].fatigueCost = _fatigueCosts[stateName];
        }
    }
}
