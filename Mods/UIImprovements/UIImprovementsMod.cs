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
        [NonSerialized]
        public Furniture SelectedFurniture = null;

        [NonSerialized]
        public EquipmentOverview EquipmentOverview = null;

        [NonSerialized]
        public static UIImprovementsMod Instance = null;

        public override void Load()
        {
            Harmony harmony = new Harmony("UIImprovements");
            harmony.PatchAll();
        }

        public override void Start()
        {
            Instance = this;
        }

        public override void Update()
        {
        }
    }
}
