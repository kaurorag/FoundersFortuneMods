using FFModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [Serializable]
    public class InteractionRestrictionEmptyGrave : InteractionRestriction {
        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            if (!(interactable is HumanInteractable)) { return true; }

            HumanInteractable humanInteractable = (HumanInteractable)interactable;
            if (humanInteractable.GetHuman() == null) { return true; }
            if (humanInteractable.GetHuman().alive) { return true; }

            var filledInfo = AccessTools.Field(typeof(GraveModule), "filled");

            if (!WorldScripts.Instance.furnitureFactory.GetModules<GraveModule>().Any(x =>
                !((bool)filledInfo.GetValue(x)) && x.parent.IsBuilt())) {
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class InteractionRestrictionEmptyGraveCooldown : InteractionRestriction {
        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            return AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown.HasValue;
        }
    }

    [Serializable]
    public class InteractionRestrictionFactionType : InteractionRestriction {
        public FactionType factionType;
        public bool mustBeThisType = true;

        public InteractionRestrictionFactionType(FactionType factionType, bool mustBeThisType = true) {
            this.factionType = factionType;
            this.mustBeThisType = mustBeThisType;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "Doesn't belong to the correct faction";
            return (interactable.GetFaction().GetFactionType() == factionType) != mustBeThisType;
        }
    }

    public enum ConstructableType {
        All = 0,
        Fields = 1,
        Furniture = 2,
        Structures = 3
    }


    [Serializable]
    public class InteractionRestrictionConstructableType : InteractionRestriction {
        public ConstructableType Type;

        public InteractionRestrictionConstructableType(ConstructableType type) => this.Type = type;

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";

            if (interaction != Interaction.Construct) return true;

            switch (this.Type) {
                case ConstructableType.Structures: return !IsStructure(interactable);
                case ConstructableType.Fields: return !IsField(interactable);
                case ConstructableType.Furniture: return IsStructure(interactable) || IsField(interactable);
            }

            return false;
        }

        private bool IsStructure(Interactable interactable) {
            if (interactable is FloorInteractable) return true;
            if (interactable is WallInteractable) return true;
            if (interactable is FurnitureInteractable) {
                Furniture furniture = interactable.GetPrimaryHolder<Furniture>();

                WallPartModule wallPartModule = furniture.GetModule<WallPartModule>();

                if (wallPartModule != null && wallPartModule.IsDoorOrWindow()) return true;
                if (furniture.HasModule<StairsModule>()) return true;
                if (furniture.HasModule<PillarModule>()) return true;
            }
            return false;
        }

        private bool IsField(Interactable interactable) {
            if (!(interactable is FurnitureInteractable)) return false;

            Furniture furniture = ((FurnitureInteractable)interactable).GetFurniture();

            return furniture.HasModule<SoilModule>();
        }
    }

    [System.Serializable]
    public class InteractionRestrictionWoodStockpile : InteractionRestriction {

        public InteractionRestrictionWoodStockpile() { }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";

            if (interactable == null || !interactable.IsValid() || !(interactable is FurnitureInteractable)) return true;
            ResourceModule rm = interactable.GetPrimaryHolder<Furniture>().GetModule<ResourceModule>();
            if (rm == null) return true;

            if (rm.GetResource() != Resource.Wood) return true;
            if (rm.resourcesPerRound == 0) return false;

            Resource aggResource;
            bool stockpileFull = !FurnitureFactory.StockpilesHaveSpace(Resource.Wood, null, out aggResource);
            if (stockpileFull) {
                reason = Helper.SubstituteCode(("stockpileIsFull"), "resource", GameState.GetDisplayNameOfResource(aggResource));
            } else {
                reason = "";
            }
            return stockpileFull;
        }
    }

    [Serializable]
    public class InteractionRestrictionResourcePerRound : InteractionRestriction {
        public int[] AllowedAmounts;
        public int[] ForbiddenAmmounts;

        public InteractionRestrictionResourcePerRound(int[] allowedAmounts = null, int[] forbiddenAmounts= null) {
            this.AllowedAmounts = allowedAmounts == null ? new int[] { } : allowedAmounts;
            this.ForbiddenAmmounts = forbiddenAmounts == null ? new int[] { } : forbiddenAmounts;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";

            if (interaction != Interaction.GatherResource) return true;
            if (interactable == null || !interactable.IsValid() || !(interactable is FurnitureInteractable)) return true;

            ResourceModule rm = interactable.GetPrimaryHolder<Furniture>().GetModule<ResourceModule>();
            if (rm == null) return true;

            if (rm.isDepleted) return true;
            if (this.AllowedAmounts.Length != 0 && !this.AllowedAmounts.Contains(rm.resourcesPerRound)) return true;
            if (this.ForbiddenAmmounts.Length != 0 && this.ForbiddenAmmounts.Contains(rm.resourcesPerRound)) return true;

            return false;
        }
    }

    [Serializable]
    public class InteractionRestrictionGrowingSpotFilter : InteractionRestriction {
        public string[] RequiredSkillsFilter;

        public InteractionRestrictionGrowingSpotFilter(params string[] filter) {
            this.RequiredSkillsFilter = filter;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            if (interactable == null || !interactable.IsValid() || !(interactable is FurnitureInteractable)) return true;
            Furniture furniture = interactable.GetPrimaryHolder<Furniture>();
            GrowingSpotModule module = furniture.GetModule<GrowingSpotModule>();
            if (module == null) return true;
            return !this.RequiredSkillsFilter.Contains(module.requiredSkill);
        }
    }

    [Serializable]
    public class InteractionRestrictionCareForTrees : InteractionRestriction {
        public string RequiredSkill;
        public float AllowedTime;

        public InteractionRestrictionCareForTrees(string requiredSkill, float time) {
            this.RequiredSkill = requiredSkill;
            this.AllowedTime = time;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            if (interactable == null || !interactable.IsValid() || !(interactable is FurnitureInteractable)) return true;
            Furniture furniture = interactable.GetPrimaryHolder<Furniture>();
            GrowingSpotModule module = furniture.GetModule<GrowingSpotModule>();
            if (module == null) return true;

            if (!actor.professionManager.HasSkill(this.RequiredSkill)) return true;
            if(module.GetFieldValue<int>("currentPhase") >= 0) {
                float timeDiff = this.AllowedTime - Time.time;
                return timeDiff > 0;
            }
            return false;
        }
    }

    [Serializable]
    public class InteractionRestrictionNameKey : InteractionRestriction {
        public string[] InteractioNameKey;

        public InteractionRestrictionNameKey(params string[] interactionNameKey) {
            this.InteractioNameKey = interactionNameKey;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            if (interactable == null || !interactable.IsValid() || !(interactable is FurnitureInteractable)) return true;
            Furniture furniture = interactable.GetPrimaryHolder<Furniture>();
            ProductionModule module = furniture.GetModule<ProductionModule>();
            if (module == null) return true;
            return !this.InteractioNameKey.Contains(module.interactionNameKey);
        }
    }
}
