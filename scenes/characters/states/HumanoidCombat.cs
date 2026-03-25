using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidCombat : Node
{
    public HumanoidModel Humanoid { get; set; }

    public override void _Ready() => Humanoid = GetParent<HumanoidModel>();

    // private readonly Dictionary<string, int> _attackPriorities = new()
    // {
    //     {"prepare", 6},
    //     {"unsheathe1", 3},
    //     {"unsheathe2", 3},
    //     {"attack1", 4},
    //     {"attack2", 5},
    // };

    public InputPackage Contextualize(InputPackage input)
    {
        TranslateInputs(input);
        return input;
    }

    public void TranslateInputs(InputPackage input)
    {
        if (input.CombatActionNames.Count <= 0) return;

        string bestAction = input.CombatActionNames.Dequeue();
        string translatedName;

        if (bestAction.Equals("unsheathe1"))
        {
            translatedName = Humanoid.WeaponInventory.GetWeapon(1).Moves[bestAction];
        }
        else if (Humanoid.CurrentWeapon is not null)
        {
            translatedName = Humanoid.CurrentWeapon.Moves[bestAction];
        }
        else
        {
            return;
        }

        State combatState = Humanoid.StateContainer.GetStateByName(translatedName);

        input.Actions.Enqueue(
            combatState,
            combatState.Priority
        );
    }


}
