using System;
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
        public string name = "";
        public string tooltip = "";

        public TooltipFlag tooltipFlags;

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

                        tooltip = "Create " + buildingTarget.name + " " + tooltip + attrs;
                        break;
                }
            }
        }
    }
}
