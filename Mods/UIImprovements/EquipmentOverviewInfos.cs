using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.UIImprovements
{
    public class EquipmentOverviewInfos : MonoBehaviour
    {
        public static EquipmentOverviewInfos Instance { get; private set; }

        public Furniture SelectedFurniture = null;
        public EquipmentOverview EquipmentOverview = null;

        private void Awake()
        {
            Instance = this;
        }
    }
}
