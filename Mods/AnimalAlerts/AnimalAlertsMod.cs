using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using UnityEngine;

namespace WitchyMods.AnimalAlerts {
    [Serializable]
    public class AnimalAlertsMod : Mod {

        [NonSerialized]
        public static AnimalAlertsMod Instance = null;

        [NonSerialized]
        public readonly List<HumanAI> animalsMarkedForButchering = new List<HumanAI>();

        public override void Load() {
                Harmony harmony = new Harmony(this.GetType().FullName);
                harmony.PatchAll();
        }

        public override void Start() {
            Instance = this;
        }

        public override void Update() {

        }
    }

    public static class NotificationPrinter {
        public static void ShowBirthNotification(HumanAI child) {
            new NotificationConfig("witchy_AnimalAlerts_birth",
                String.Format(Localization.GetText("witchy_AnimalAlerts_birth"), child.GetFullName()),
                UISound.NotificationGeneric, child, null, null, -1, true).Show();
        }

        public static void ShowButcherNotification(HumanAI animal) {
            new NotificationConfig("witchy_AnimalAlerts_butchered",
                String.Format(Localization.GetText("witchy_AnimalAlerts_butchered"), animal.GetFullName()),
                UISound.NotificationGeneric, null, null, null, -1, true).Show();
        }

        public static void ShowDiedOldAgeNotification(HumanAI animal) {
            new NotificationConfig("witchy_AnimalAlerts_diedOldAge",
                String.Format(Localization.GetText("witchy_AnimalAlerts_diedOldAge"), animal.GetFullName()),
                UISound.NotificationGeneric, null, null, null, -1, true).Show();
        }
    }

    [HarmonyPatch(typeof(NotificationConfig))]
    public static class NotificationConfig_Patch {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPostfix]
        public static void ChangeText(NotificationConfig __instance) {
            if (__instance.subject != null && __instance.subject is HumanAI && ((HumanAI)__instance.subject).animal != null) {
                HumanAI animal = (HumanAI)__instance.subject;

                if (__instance.id.StartsWith("growingUp")) {
                    __instance.text = Localization.GetText("notificationTitle_willGrowUpTomorrow").Replace("%firstName%", animal.GetFullName());
                } else if (__instance.id.StartsWith("dyingOldAge")) {
                    __instance.text = Localization.GetText("notificationTitle_isApproachingEndOfLife").Replace("%firstName%", animal.GetFullName());
                }
            }
        }
    }

    [HarmonyPatch(typeof(AnimalSpeciesManager))]
    public static class AnimalSpeciesManager_Patch {

        private static MethodInfo spawnHumanMethod = AccessTools.Method(typeof(HumanManager), "SpawnHuman",
          new Type[] { typeof(HumanConfiguration), typeof(Vector3), typeof(Faction), typeof(bool), typeof(HumanAI.Serializable) });

        private static MethodInfo showBirthNotifMethod = AccessTools.Method(typeof(NotificationPrinter), "ShowBirthNotification");

        [HarmonyPatch("Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var instruction in instructions) {

                if (instruction.Calls(spawnHumanMethod)) {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, showBirthNotifMethod);
                } else
                    yield return instruction;
            }
        }
    }

    [HarmonyPatch]
    public static class BitcheringStart_Patch {
        [HarmonyPatch(typeof(YieldMicroInteraction), "Butcher")]
        [HarmonyPrefix]
        public static void Butcher_Prefix(YieldMicroInteraction __instance) {
            Interactable target = (Interactable)AccessTools.Property(typeof(YieldMicroInteraction), "InteractionTarget").GetValue(__instance);
            if (target is HumanInteractable && target.GetPrimaryHolder<HumanAI>().animal != null) {
                AnimalAlertsMod.Instance.animalsMarkedForButchering.Add(target.GetPrimaryHolder<HumanAI>());
            }
        }

        [HarmonyPatch(typeof(HumanAI), "Die")]
        [HarmonyPostfix]
        public static void Die_Postfix(HumanAI __instance, bool oldAge, bool force) {
            if (!__instance.alive && __instance.animal != null && __instance.faction is ColonyFaction) {
                if (oldAge) {
                    NotificationPrinter.ShowDiedOldAgeNotification(__instance);
                } else if (AnimalAlertsMod.Instance.animalsMarkedForButchering.Contains(__instance)) {
                    NotificationPrinter.ShowButcherNotification(__instance);
                    AnimalAlertsMod.Instance.animalsMarkedForButchering.Remove(__instance);
                }
            }
        }
    }

    [HarmonyPatch()]
    public static class ButcheringEnd_Patch {

        public static MethodBase TargetMethod() {
            return AccessTools.Method(typeof(YieldMicroInteraction), "Handle", new Type[] { typeof(YieldResult).MakeByRefType() });
        }

        public static void Postfix(YieldMicroInteraction __instance, ref YieldResult yieldResult) {
            if (__instance.Interaction == Interaction.Butcher && (yieldResult == YieldResult.Completed || yieldResult == YieldResult.Failed)) {
                Interactable target = (Interactable)AccessTools.Property(typeof(YieldMicroInteraction), "InteractionTarget").GetValue(__instance);
                AnimalAlertsMod.Instance.animalsMarkedForButchering.Remove(target.GetPrimaryHolder<HumanAI>());
            }
        }
    }
}
