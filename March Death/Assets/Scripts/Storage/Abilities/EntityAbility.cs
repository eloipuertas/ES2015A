using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Storage
{
    [Flags]
    public enum TooltipFlag
    {
        HIDE                = 1,
        SHOW                = 2,
        DISPLAY_COST        = 4,
        DISPLAY_TARGET      = 8
    };

    public class EntityAbility
    {
        public string ability = "";
        public string name = "";
        public string tooltip = "";

        public TooltipFlag tooltipFlags;

        public KeyCode keyBinding;

        public EntityType targetType;
        public Races targetRace;
        public BuildingTypes targetBuilding;
        public UnitTypes targetUnit;

        public void SetupTooltip(Info info)
        {
            if ((tooltipFlags & TooltipFlag.HIDE) == TooltipFlag.HIDE)
            {
                return;
            }

            if ((tooltipFlags & TooltipFlag.DISPLAY_COST) == TooltipFlag.DISPLAY_COST)
            {
                tooltip += "\n\nCost:\n" +
                    "Wood: [[resources.wood]]\n" +
                    "Metal: [[resources.metal]]\n" +
                    "Food: [[resources.food]]";
            }

            if ((tooltipFlags & TooltipFlag.DISPLAY_TARGET) == TooltipFlag.DISPLAY_TARGET)
            {
                string attrs = "";

                switch (targetType)
                {
                    case EntityType.UNIT:
                        UnitInfo unitTarget = info.of(targetRace, targetUnit);

                        attrs = "\n\nAttributes:\n" +
                            "WeaponAbility: [[attributes.weaponAbility]]\n" +
                            "ProjectileAbility: [[attributes.projectileAbility]]\n" +
                            "Strength: [[attributes.strength]]\n" +
                            "Resistance: [[attributes.resistance]]\n" +
                            "Wounds: [[attributes.wounds]]\n" +
                            "Attack Rate: [[attributes.attackRate]]\n" +
                            "Movement Rate: [[attributes.movementRate]]";

                        attrs = attrs.FormatWith(unitTarget, @"\[\[", @"\]\]");
                        tooltip = "Recruit " + unitTarget.name + " " + tooltip + attrs;
                        break;

                    case EntityType.BUILDING:
                        BuildingInfo buildingTarget = info.of(targetRace, targetBuilding);

                        int i = 0;
                        foreach (EntityAbility ability in buildingTarget.abilities)
                        {
                            if (i > 0)
                            {
                                attrs += ", ";
                            }

                            if ((ability.tooltipFlags & TooltipFlag.HIDE) == TooltipFlag.HIDE)
                            {
                                continue;
                            }

                            attrs += ability.name;
                            ++i;
                        }

                        if (i > 0)
                        {
                            attrs = "\n\nCan recruit:\n" + attrs;
                        }

                        tooltip = "Create " + buildingTarget.name + " " + tooltip + attrs + "\nKey : " + keyBinding.ToString();
                        break;
                }
            }
        }

        // Method to get keybindings
        public void SetupKeyCode()
        {
            switch (name)
            {
                case "Create Farm":
                    keyBinding = KeyCode.F;
                    break;
                case "Create Mine":
                    keyBinding = KeyCode.M;
                    break;
                case "Create Sawmill":
                    keyBinding = KeyCode.S;
                    break;
                case "Create Archery":
                    keyBinding = KeyCode.A;
                    break;
                case "Create Stable":
                    keyBinding = KeyCode.C;
                    break;
                case "Create Watchtower":
                    keyBinding = KeyCode.T;
                    break;
                case "Create Wall":
                    keyBinding = KeyCode.W;
                    break;
                case "Create Cavalry":
                    keyBinding = KeyCode.C;
                    break;
                case "Create Thrown":
                    keyBinding = KeyCode.T;
                    break;
                case "Create LightArmor":
                    keyBinding = KeyCode.L;
                    break;
                case "Create HeavyArmor":
                    keyBinding = KeyCode.H;
                    break;
                case "Create Barrack":
                    keyBinding = KeyCode.B;
                    break;
                case "Create WallCorner":
                    keyBinding = KeyCode.Q;
                    break;
                case "Create Civil":
                    keyBinding = KeyCode.C;
                    break;
                case "Sell":
                    keyBinding = KeyCode.S;
                    break;
                default:
                    keyBinding =  KeyCode.RightShift;
                    break;
            }
        }
    }
}
