using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {
    public class YieldMicroInteractionHelper {
        private YieldMicroInteraction y = null;

        public const Interaction TendToFieldsInteraction = (Interaction)(-1);

        public YieldMicroInteractionHelper(YieldMicroInteraction instance) {
            y = instance;
        }

        //Members
        public HumanAI human {
            get { return AccessTools.Field(typeof(YieldMicroInteraction), "human").GetValue(y) as HumanAI; }
        }

        public Interactable InteractionTarget {
            get { return (Interactable)AccessTools.Property(typeof(YieldMicroInteraction), "InteractionTarget").GetValue(y); }
        }

        public InteractionInfo InteractionInfo {
            get { return (InteractionInfo)AccessTools.Property(typeof(YieldMicroInteraction), "InteractionInfo").GetValue(y); }
        }

        public Interaction Interaction {
            get { return (Interaction)AccessTools.Property(typeof(YieldMicroInteraction), "Interaction").GetValue(y); }
        }

        public YieldMicroInteraction subTask {
            get { return (YieldMicroInteraction)AccessTools.Property(typeof(YieldMicroInteraction), "subTask").GetValue(y); }
            set { AccessTools.Property(typeof(YieldMicroInteraction), "subTask").SetValue(y, value); }
        }

        public bool continueOnFail {
            get { return (bool)AccessTools.Field(y.GetType(), "continueOnFail").GetValue(y); }
            set { AccessTools.Field(y.GetType(), "continueOnFail").SetValue(y, value); }
        }

        //Methods
        public IEnumerable<YieldResult> WorkOnFurniture() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "WorkOnFurniture").Invoke(y, new object[] { });
        }

        public YieldResult Interact(Interaction interaction) {
            if (InteractionTarget.Interact(interaction, human, InteractionInfo.issuedByAI))
                return YieldResult.Continue;
            else
                return YieldResult.Failed;
        }

        public IEnumerable<YieldResult> Wait(float seconds, Ref<float> progress = null) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "Wait").Invoke(y, new object[] { seconds, progress });
        }

        public IEnumerable<YieldResult> Walk(Interactable target, float distance, bool refuseOnFail = true, Vector3 offset = default(Vector3)) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "Walk").Invoke(y, new object[] { target, distance, refuseOnFail, offset });
        }

        public IEnumerable<YieldResult> ContinueInteraction(IEnumerable<YieldResult> interactionEnumberable, float distance, System.Func<bool> continueCondition = null, bool continueOnFail = true) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "ContinueInteraction").Invoke(y, new object[] {
                interactionEnumberable,
                distance,
                continueCondition,
                continueOnFail
            });
        }

        public void Continue(float distance) {
            AccessTools.Method(y.GetType(), "Continue").Invoke(y, new object[] { distance });
        }

        public IEnumerable<YieldResult> Retry(System.Func<bool, IEnumerable<YieldResult>> actionToRetry, int attempts) {
            return (IEnumerable<YieldResult>)AccessTools.Method(y.GetType(), "Retry").Invoke(y, new object[] { actionToRetry, attempts });
        }

        public IEnumerable<YieldResult> StartWorkOnFurniture() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "StartWorkOnFurniture").Invoke(y, new object[] { });
        }

        public IEnumerable<YieldResult> WorkOnFurnitureStep(float duration) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "WorkOnFurnitureStep").Invoke(y, new object[] { duration });
        }

        public void Carry(HumanAI target, Ragdoll ragdoll) {
            AccessTools.Method(y.GetType(), "Carry").Invoke(y, new object[] { target, ragdoll });
        }

        public YieldResult LockInteraction() {
            return (YieldResult)AccessTools.Method(typeof(YieldMicroInteraction), "LockInteraction").Invoke(y, new object[] { });
        }

        public YieldResult Interact() {
            return (YieldResult)AccessTools.Method(typeof(YieldMicroInteraction), "Interact").Invoke(y, new object[] { });
        }

        public YieldResult ObjectInteract(Interactable objectToInteractWith) {
            return (YieldResult)AccessTools.Method(y.GetType(), "ObjectInteract").Invoke(y, new object[] { objectToInteractWith });
        }

        public IEnumerable<YieldResult> Butcher() {
            return AccessTools.Method(y.GetType(), "Butcher").Invoke(y, new object[] { }) as IEnumerable<YieldResult>;
        }

        public IEnumerable<YieldResult> Milk() {
            return AccessTools.Method(y.GetType(), "Milk").Invoke(y, new object[] { }) as IEnumerable<YieldResult>;
        }

        public IEnumerable<YieldResult> Shear() {
            return AccessTools.Method(y.GetType(), "Shear").Invoke(y, new object[] { }) as IEnumerable<YieldResult>;
        }

        public IEnumerable<YieldResult> Tame() {
            return AccessTools.Method(y.GetType(), "Tame").Invoke(y, new object[] { }) as IEnumerable<YieldResult>;
        }

        public void TurnToTransform() {
            AccessTools.Method(typeof(YieldMicroInteraction), "TurnToTransform").Invoke(y, new object[] { });
        }

        public void StopCurrentSubtask() {
            AccessTools.Method(typeof(YieldMicroInteraction), "StopCurrentSubtask").Invoke(y, new object[] { });
        }

        //Custom
        public void New_Continue() {
            var info = WorkInteractionControllerPatch.GetTendToFieldsInteraction(human);
            human.AbortInteractionAt(0, false);
            if (info != null) {
                human.SetCurrentTask(info);
            }
        }

        public IEnumerable<YieldResult> New_TendToFields() {
            Furniture furniture = InteractionTarget.GetPrimaryHolder<Furniture>();
            SoilModule soilModule = furniture.GetModule<SoilModule>();

            foreach (var interaction in soilModule.GetInteractions(human, InteractionInfo.issuedByAI, false)) {
                if (interaction.interaction == Interaction.RemoveInfestedPlants ||
                    interaction.interaction == Interaction.RemovePlant ||
                    interaction.interaction == TendToFieldsInteraction)
                    continue;

                if (interaction.interaction == Interaction.WaterPlant) {
                    if (!soilModule.GetSeasons().Contains(SeasonManager.GetCurrentSeason())) continue;

                    //InteractionRestrictionCanTakeResourceNearby cheat.  we don't check the distance
                    if (!WorldScripts.Instance.furnitureFactory.GetModules<InventoryReplenishmentModule>().Any(x => x.GetResource() == Resource.Water))
                        continue;

                    //InteractionRestrictionEquipmentEffect cheat.  we don't check the distance
                    if (!human.equipmentSet.GetEffects().Any(x => x.first == EquipmentEffect.WateringPlants))
                        continue;

                } else if (!WorkInteractionControllerPatch.CheckInteraction(InteractionTarget, interaction, human))
                    continue;

                subTask = new YieldMicroInteraction(new InteractionInfo(
                    interaction.interaction, InteractionTarget, interaction.restrictions, InteractionInfo.issuedByAI, InteractionInfo.priority, isContinuationOrSubtask: true),
                    human);

                while (subTask != null) { // WHILEPROTECTED
                    subTask = subTask.Handle();
                    yield return YieldResult.WaitFrame;
                }
                StopCurrentSubtask();
            }

            yield return YieldResult.Completed;
        }

        public IEnumerable<YieldResult> New_WaterPlantCustom() {
            if (human.inventory.GetCount(Resource.Water) == 0) {
                List<InteractionRestriction> restrictions = new List<InteractionRestriction>() { new InteractionRestrictionResource(Resource.Water) };
                Interactable well = WorkInteractionControllerPatch.SearchForInteractableWithInteraction(human, Interaction.TakeIntoInventory, -1f, restrictions);

                subTask = new YieldMicroInteraction(new InteractionInfo(Interaction.TakeIntoInventory, well, restrictions, InteractionInfo.issuedByAI, InteractionInfo.priority, false, false, true), human);
                while (subTask != null) { // WHILEPROTECTED
                    subTask = subTask.Handle();
                    yield return YieldResult.WaitFrame;
                }
                StopCurrentSubtask();
                if (human.inventory.GetCount(Resource.Water) == 0) { yield return YieldResult.Failed; }
            }

            foreach (var x in Walk(InteractionTarget, 0.1f)) { yield return x; }
            TurnToTransform();
            yield return LockInteraction();
            human.SetAnimationParameter("isWatering", true);
            foreach (var x in Wait(5 * human.GetWorkTimeFactor())) { yield return x; }
            yield return Interact();
            human.inventory.Remove(Resource.Water, 1);
            yield return YieldResult.Completed;
        }

        public IEnumerable<YieldResult> New_Butcher() {
            foreach (var x in LockAnimal()) yield return x;
            foreach (var x in Butcher()) yield return x;
        }

        public IEnumerable<YieldResult> New_Milk() {
            foreach (var x in LockAnimal()) yield return x;
            foreach (var x in Milk()) yield return x;
        }

        public IEnumerable<YieldResult> New_Shear() {
            foreach (var x in LockAnimal()) yield return x;
            foreach (var x in Shear()) yield return x;
        }

        public IEnumerable<YieldResult> New_Tame() {
            foreach (var x in LockAnimal()) yield return x;
            foreach (var x in Tame()) yield return x;
        }

        public IEnumerable<YieldResult> StartGetInteractionEnumerable(Interaction interaction) {

            switch (interaction) {
                case TendToFieldsInteraction: return ContinueInteraction(New_TendToFields(), 20f);
                case Interaction.ClearStumps:
                case Interaction.GatherResource: if (InteractionInfo.isContinuationOrSubtask) return WorkOnFurniture(); else return null;
                case Interaction.WaterPlant: return New_WaterPlantCustom();
                case Interaction.Tame: return New_Tame();
                case Interaction.Butcher: return New_Butcher();
                case Interaction.Shear: return New_Shear();
                case Interaction.Milk:  return New_Milk();
            }

            return null;
        }

        private IEnumerable<YieldResult> LockAnimal() {
            HumanAI partner = InteractionTarget.GetPrimaryHolder<HumanAI>();
            if (partner.animal != null)
                yield return LockInteraction();
        }

        public YieldMicroInteraction New_Handle() {
            YieldResult result;
            YieldMicroInteraction resultHandle = y.Handle(out result) as YieldMicroInteraction;

            if(result == YieldResult.Failed && this.Interaction == Interaction.Bury && 
                this.InteractionInfo.restrictions != null &&
                this.InteractionInfo.restrictions.OfType<InteractionRestrictionFaction>().Any()) {
                AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown = 5;
            }

            return resultHandle;
        }
    }
}
