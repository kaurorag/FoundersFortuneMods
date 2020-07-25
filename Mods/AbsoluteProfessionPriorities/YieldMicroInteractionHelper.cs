using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace WitchyMods.AbsoluteProfessionPriorities {
    public class YieldMicroInteractionHelper {
        private YieldMicroInteraction y = null;

        public static Interaction TendToFieldsInteraction = (Interaction)(-1);

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

        //Methods
        public IEnumerable<YieldResult> Sow() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "Sow").Invoke(y, new object[] { });
        }

        public IEnumerable<YieldResult> WaterPlant() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "WaterPlant").Invoke(y, new object[] { });
        }

        public IEnumerable<YieldResult> WorkOnFurniture() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "WorkOnFurniture").Invoke(y, new object[] { });
        }

        public YieldResult Interact(Interaction interaction) {
            if (InteractionTarget.Interact(interaction, human, InteractionInfo.issuedByAI))
                return YieldResult.Continue;
            else
                return YieldResult.Failed;
        }

        public IEnumerable<YieldResult> Wait(float seconds) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "Wait").Invoke(y, new object[] { seconds });
        }

        public IEnumerable<YieldResult> Walk(Interactable target, float distance) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "Walk").Invoke(y, new object[] { target, distance });
        }

        public IEnumerable<YieldResult> ContinueInteraction(IEnumerable<YieldResult> interactionEnumberable, float distance, System.Func<bool> continueCondition = null, bool continueOnFail = true) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "ContinueInteraction").Invoke(y, new object[] {
                interactionEnumberable,
                distance,
                continueCondition,
                continueOnFail
            });
        }

        public IEnumerable<YieldResult> StartWorkOnFurniture() {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "StartWorkOnFurniture").Invoke(y, new object[] { });
        }

        public IEnumerable<YieldResult> WorkOnFurnitureStep(float duration) {
            return (IEnumerable<YieldResult>)AccessTools.Method(typeof(YieldMicroInteraction), "WorkOnFurnitureStep").Invoke(y, new object[] { duration });
        }

        public YieldResult LockInteraction() {
            return (YieldResult)AccessTools.Method(typeof(YieldMicroInteraction), "LockInteraction").Invoke(y, new object[] { });
        }

        public void TurnToTransform() {
            AccessTools.Method(typeof(YieldMicroInteraction), "TurnToTransform").Invoke(y, new object[] { });
        }

        public void StopCurrentSubtask() {
            AccessTools.Method(typeof(YieldMicroInteraction), "StopCurrentSubtask").Invoke(y, new object[] { });
        }

        //Custom
        public void ContinueCustom() {
            var info = WorkInteractionControllerPatch.GetTendToFieldsInteraction(human);
            human.AbortInteractionAt(0, false);
            if (info != null) {
                human.SetCurrentTask(info);
            }
        }

        public IEnumerable<YieldResult> TendToFields() {
            Furniture furniture = InteractionTarget.GetPrimaryHolder<Furniture>();
            SoilModule soilModule = furniture.GetModule<SoilModule>();

            DebugLogger.Log("In TendToFields");

            foreach (var interaction in soilModule.GetInteractions(human, true, false)) {
                if (interaction.interaction == Interaction.RemoveInfestedPlants ||
                    interaction.interaction == Interaction.RemovePlant ||
                    interaction.interaction == TendToFieldsInteraction)
                    continue;

                DebugLogger.Log($"SoilModule has interaction: {interaction.interaction}");

                if (WorkInteractionControllerPatch.CheckInteraction(InteractionTarget, interaction, human)) {
                    subTask = new YieldMicroInteraction(new InteractionInfo(
                        interaction.interaction, InteractionTarget, interaction.restrictions, true, InteractionInfo.priority, isContinuationOrSubtask: true),
                        human);

                    DebugLogger.Log($"Setting subtask for {interaction.interaction}");

                    while (subTask != null) { // WHILEPROTECTED
                        subTask = subTask.Handle();
                        yield return YieldResult.WaitFrame;
                    }
                    StopCurrentSubtask();
                }
            }

            yield return YieldResult.Completed;
        }

        public IEnumerable<YieldResult> WorkOnFurnitureCustom() {
            foreach (var x in StartWorkOnFurniture()) { yield return x; }
            foreach (var x in WorkOnFurnitureStep(1f)) { yield return x; }
            DebugLogger.Log("Finished work on furniture");
            yield return YieldResult.Completed;
        }
    }
}
