using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FFModUtils;
using HarmonyLib;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class YieldMicroInteractionHelper {

        private YieldMicroInteraction y = null;

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

        public IEnumerable<YieldResult> Construct() {
            return AccessTools.Method(y.GetType(), "Construct").Invoke(y, new object[] { }) as IEnumerable<YieldResult>;
        }

        public void TurnToTransform() {
            AccessTools.Method(typeof(YieldMicroInteraction), "TurnToTransform").Invoke(y, new object[] { });
        }

        public void StopCurrentSubtask() {
            AccessTools.Method(typeof(YieldMicroInteraction), "StopCurrentSubtask").Invoke(y, new object[] { });
        }

        //Custom
        public IEnumerable<YieldResult> StartGetInteractionEnumerable(Interaction interaction) {
            if (ShouldLockTarget(interaction)) {
                yield return New_LockInteraction(true);
            }
   
            //Only interactions created by this mod have a InteractionMaxDistanceData so we know it's ours
            if (this.InteractionInfo.data != null && this.InteractionInfo.data is InteractionMaxDistanceData) {
                float distance = ((InteractionMaxDistanceData)this.InteractionInfo.data).MaxDistance;

                switch (interaction) {
                    case Specialization.TendToFieldsInteraction:
                        foreach (var x in ContinueInteraction(New_TendToFields(), distance)) yield return x; break;
                    case Specialization.SowAndCareForTrees:
                        foreach (var x in ContinueInteraction(New_SowAndCareForTrees(), distance)) yield return x; break;
                    case Interaction.Sow:
                        foreach (var x in New_Sow()) yield return x; break;
                    case Specialization.ChopTrees:
                    case Interaction.GatherResource:
                    case Interaction.ClearStumps:
                        foreach (var x in DoOnceForSubTask(WorkOnFurniture(), distance)) yield return x; break;
                    case Interaction.WaterPlant:
                        foreach (var x in New_WaterPlantCustom()) yield return x; break;
                }
            }
        }

        private IEnumerable<YieldResult> DoOnceForSubTask(IEnumerable<YieldResult> task, float distance) {
            if (InteractionInfo.isContinuationOrSubtask)
                foreach (var x in task) yield return x;
            else
                foreach (var x in ContinueInteraction(task, distance)) yield return x;
        }

        private bool ShouldLockTarget(Interaction interaction) {
            switch (interaction) {
                case Interaction.EatAtChair:
                case Interaction.DrinkBeerAtChair:
                case Interaction.TryFluTreatment:
                case Interaction.SplintArm:
                case Interaction.SplintLeg:
                case Interaction.BandageWounds:
                case Interaction.Sow:
                case Interaction.Construct:
                case Interaction.GatherResource:
                case Interaction.LearnCrafting:
                case Interaction.LearnFarming:
                case Interaction.LearnFighting:
                case Interaction.LearnForesting:
                case Interaction.LearnLifeSkills:
                case Interaction.LearnMedicine:
                case Interaction.LearnMining:
                case Interaction.LearnResearching:
                case Interaction.WaterPlant:
                case Interaction.CarryToBed:
                case Interaction.Bury:
                case Interaction.Sit:
                case Interaction.Sleep:
                case Interaction.SleepUntilHealthy:
                case Interaction.LockUp:
                case Interaction.Remove:
                case Interaction.Deconstruct:
                case Interaction.Produce:
                case Interaction.ProcessResource:
                case Interaction.RemovePlant:
                case Interaction.RemoveDeadPlants:
                case Interaction.WorkOnMasterpiece:
                case Interaction.RemoveInfestedPlants:
                case Interaction.ClearStumps:
                case Interaction.GiveFood:
                case Interaction.GiveHealingPotion:
                case Interaction.Care:
                case Interaction.Tame:
                case Interaction.Shear:
                case Interaction.Butcher:
                case Interaction.Milk:
                    return true;
            }

            return false;
        }

        public YieldResult New_LockInteraction(bool fromMod) {
            if (InteractionTarget.interactionLockID != 0 && Math.Abs(InteractionTarget.interactionLockID) != human.GetID() && InteractionTarget != human.GetInteractable()) {
                if (!fromMod && InteractionTarget.interactionLockID < 0)
                    return YieldResult.Continue;
                else
                    return YieldResult.Failed;
            }
            InteractionTarget.SetInteractionLockID(human.GetID());
            return YieldResult.Continue;
        }

        public IEnumerable<YieldResult> New_SowAndCareForTrees() {
            GrowingSpotModule module = this.InteractionTarget.GetPrimaryHolder<Furniture>().GetModule<GrowingSpotModule>();
            if(module.GetFieldValue<int>("currentPhase") < 0) {
                subTask = new YieldMicroInteraction(new InteractionInfo(Interaction.Sow,
                    this.InteractionTarget, this.InteractionInfo.restrictions, this.InteractionInfo.issuedByAI,
                    this.InteractionInfo.priority, this.InteractionInfo.urgent,
                    this.InteractionInfo.shouldBeFinished, true), this.human);
            } else {
                subTask = new YieldMicroInteraction(new InteractionInfo(Interaction.Care,
                    this.InteractionTarget, this.InteractionInfo.restrictions, this.InteractionInfo.issuedByAI,
                    this.InteractionInfo.priority, this.InteractionInfo.urgent,
                    this.InteractionInfo.shouldBeFinished, true), this.human);
            }

            subTask = subTask.Handle();
            yield return YieldResult.WaitFrame;
            StopCurrentSubtask();
            yield return YieldResult.Completed;
        }

        public IEnumerable<YieldResult> New_TendToFields() {
            Furniture furniture = InteractionTarget.GetPrimaryHolder<Furniture>();
            SoilModule soilModule = furniture.GetModule<SoilModule>();

            foreach (var interaction in soilModule.GetInteractions(human, InteractionInfo.issuedByAI, false)) {
                if (interaction.interaction == Interaction.RemoveInfestedPlants ||
                    interaction.interaction == Interaction.RemovePlant ||
                    interaction.interaction == Specialization.TendToFieldsInteraction)
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
                Interactable well = human.SearchForInteractableWithInteraction(Interaction.TakeIntoInventory, -1f, restrictions);

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

        private IEnumerable<YieldResult> New_Sow() {
            foreach (var x in Walk(InteractionTarget, 0.02f)) { yield return x; }
            yield return LockInteraction();
            TurnToTransform();
            human.SetAnimationParameter("isPlanting", true);
            float plantingEndTime = 2f; // is actually 3.8
            float plantingTime = (5 + plantingEndTime) * human.GetWorkTimeFactor() - plantingEndTime;
            foreach (var x in Wait(plantingTime)) { yield return x; }
            yield return Interact();
            yield return YieldResult.Completed;
        }

        public YieldMicroInteraction New_Handle() {
            YieldResult result;
            YieldMicroInteraction resultHandle = y.Handle(out result) as YieldMicroInteraction;

            if (result == YieldResult.Failed && this.Interaction == Interaction.Bury &&
                this.InteractionInfo.restrictions != null &&
                this.InteractionInfo.restrictions.OfType<InteractionRestrictionFaction>().Any()) {
                AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown = 5;
            }

            return resultHandle;
        }
    }
}
