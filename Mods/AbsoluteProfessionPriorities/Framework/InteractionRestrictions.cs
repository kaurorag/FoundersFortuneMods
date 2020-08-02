using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
