using FFModUtils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {
    [HarmonyPatch]
    public static class MaxPriorityPatch {
        public static int MaxPriority = 4;

        [HarmonyPatch(typeof(ProfessionDetailPanel), "UpdatePriority")]
        [HarmonyPostfix]
        public static void UpdatePriority_Postfix(ProfessionDetailPanel __instance) {
            HumanAI human = __instance.GetFieldValue<HumanAI>("human");
            ProfessionType type = __instance.GetPropertyValue<ProfessionType>("type");
            Image priorityImage = __instance.GetFieldValue<Image>("priorityImage");

            Sprite starsFour = ModHandler.mods.sprites["starsFour"];

            int priority = human.professionManager.GetProfession(type).priority;

            if (priority == 4) {
                priorityImage.sprite = starsFour;
            }

            __instance.priorityHigherButton.SetIsPressable(priority < MaxPriority);
        }

        [HarmonyPatch(typeof(ProfessionDetailPanel), "GetColorByPriority")]
        [HarmonyPostfix]
        public static void GetColorByPriority_Postfix(int priority, ref Color __result) {
            if (priority == 4) {
                __result = new Color(0f, 0.35f, 0.18f);
            }
        }

        [HarmonyPatch(typeof(ProfessionDetailPanel), "HigherPriorityLoop")]
        [HarmonyPrefix]
        public static bool HigherPriorityLoop_Prefix(ProfessionDetailPanel __instance) {
            HumanAI human = __instance.GetFieldValue<HumanAI>("human");
            ProfessionType type = __instance.GetPropertyValue<ProfessionType>("type");

            int prio = human.professionManager.GetProfession(type).priority + 1;
            human.professionManager.SetPriority(type, prio, human);
            __instance.InvokeMethod("UpdatePriority");

            return false;
        }

        [HarmonyPatch(typeof(Profession), "SetPriorityDirectly")]
        [HarmonyPrefix]
        public static bool SetPriorityDirectly_Prefix(Profession __instance, int priority) {
            __instance.SetPropertyValue("priority", Mathf.Clamp(priority, 0, MaxPriority));
            return false;
        }

        [HarmonyPatch(typeof(ProfessionManager), "SetPriority")]
        [HarmonyPrefix]
        public static bool SetPriority_Prefix(ProfessionManager __instance, ProfessionType type, int priority, HumanAI human) {
            priority = Mathf.Clamp(priority, 0, MaxPriority);
            if (!__instance.CanHaveProfession(human)) { return false; }

            __instance.professions[type].SetPriorityDirectly(priority);

            // only one at max priority
            if (priority == MaxPriority && type != ProfessionType.Builder) {
                foreach (Profession profession in __instance.professions.Values) {
                    if (profession.type != type && profession.type != ProfessionType.Builder && profession.priority == MaxPriority) {
                        __instance.SetPriority(profession.type, MaxPriority - 1, human);
                    }
                }
            }

            // cancel work
            if (priority == 0) {
                human.AbortInteractionsWhere(x => ProfessionManager.workInteractions[type].Contains(x.interaction));
            }

            // tutorial
            WorldScripts.Instance.tutorialManager.HasAssignedProfession();
            return false;
        }

        [HarmonyPatch(typeof(ProfessionManager), "SetProfession")]
        [HarmonyPrefix]
        public static bool SetProfession_Prefix(ProfessionManager __instance, ProfessionType type, HumanAI human) {
            __instance.SetPriority(type, MaxPriority, human);
            return false;
        }

        [HarmonyPatch(typeof(ProfessionManager), "GetPrimaryProfession")]
        [HarmonyPrefix]
        public static bool GetPrimaryProfession_Prefix(ProfessionManager __instance, ref Profession __result) {
            int maxPrio = __instance.professions.Values.Where(x => x.type != ProfessionType.Builder).Max(x => x.priority);
            if (maxPrio == 0) {
                if (__instance.professions[ProfessionType.Builder].priority != 0)
                    __result = __instance.professions[ProfessionType.Builder];
                else
                    __result = __instance.professions[ProfessionType.NoJob];
            } else {
                Profession prof = __instance.professions.Values
                    .Where(x => x.priority == maxPrio && x.type != ProfessionType.Builder)
                    .MaxBy(x => x.level + x.GetExperienceProgress());
                __result = prof;
            }
            return false;
        }
    }
}
