using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;

namespace PoutOnYourOwn
{
    [Serializable]
    public class PoutOnYourOwnMod : Mod
    {
        public override void Load()
        {
            Harmony harmony = new Harmony("PoutOnYourOwn");
            harmony.PatchAll();

            UnityEngine.MonoBehaviour.print("PoutOnYourOwn loaded");
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }
    }

    [HarmonyPatch(typeof(IdleInteractionController), "GetRandomHumanIdleInteraction")]
    public  static class IdleInteractionControllerPatch
    {
        private static Interaction[] _AllowedInteractions = new Interaction[] { Interaction.Relax, Interaction.Sit, Interaction.Enjoy, Interaction.Dance };

        public static void Postfix(IdleInteractionController __instance, ref Interaction __result)
        {
            FieldInfo fInfo = typeof(IdleInteractionController).GetField("human", BindingFlags.NonPublic | BindingFlags.Instance);
            HumanAI human = (HumanAI)fInfo.GetValue(__instance);

            if((human.IsLazy() || human.uncontrollableReasons.Count > 0) && !_AllowedInteractions.Contains(__result))
                __result = _AllowedInteractions[new Random().Next(0, _AllowedInteractions.Length)];
        }
    }
}
