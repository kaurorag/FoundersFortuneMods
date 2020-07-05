using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.UIImprovements
{
    [Serializable()]
    public class UIImprovementsMod : Mod
    {
        public override void Load()
        {
            Harmony harmony = new Harmony("UIImprovements");
            harmony.PatchAll();
        }

        public override void Start()
        {
            var infos = WorldScripts.Instance.GetComponent<EquipmentOverviewInfos>();
            if (infos == null)
                WorldScripts.Instance.gameObject.AddComponent<EquipmentOverviewInfos>();
        }

        public override void Update()
        {
        }
    }
}
