using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [Serializable]
    public class AbsoluteProfessionPrioritiesMod : Mod {
        //[humanId][ProfessionType][specializations] (ordered by priority where 0=most important]
        public Dictionary<long, Dictionary<ProfessionType, List<String>>> specializationPriorities = new Dictionary<long, Dictionary<ProfessionType, List<string>>>();

        [NonSerialized]
        private static Dictionary<ProfessionType, List<String>> defaultPriorities = new Dictionary<ProfessionType, List<string>>();

        [NonSerialized]
        public static AbsoluteProfessionPrioritiesMod Instance;

        public float? buryColonistFailCooldown;


        public override void Load() {
                Harmony harmony = new Harmony("AbsoluteProfessionPriorities");
                harmony.PatchAll();
        }

        public override void Start() {
            Instance = this;

            //Prepare the default dictionary
            foreach (var spec in ModHandler.mods.professionSpecializations.Values) {
                foreach (ProfessionType profession in spec.professionNames.Select(x => Enum.Parse(typeof(ProfessionType), x, true))) {
                    if (!defaultPriorities.ContainsKey(profession))
                        defaultPriorities.Add(profession, new List<string>());

                    defaultPriorities[profession].Add(spec.name);
                }
            }

            if (!defaultPriorities.ContainsKey(ProfessionType.Builder))
                defaultPriorities.Add(ProfessionType.Builder, new List<string>());

            //If it's null, then we either loaded a new game or a game that hadn't had the mod yet
            if (specializationPriorities == null) {
                specializationPriorities = new Dictionary<long, Dictionary<ProfessionType, List<string>>>();
            }

            //Get all of the IDs of the current colonists
            HumanManager humanManager = WorldScripts.Instance.humanManager;
            List<long> humanIds = humanManager.GetHumans().Where(x => x.faction.GetFactionType() == FactionType.Colony && x.humanType == HumanType.Colonist).Select(x => x.GetID()).ToList();

            //Remove all of the saved IDs that are not in the colony anymore
            foreach (var removedId in specializationPriorities.Keys.Where(x => !humanIds.Contains(x)).ToArray()) {
                specializationPriorities.Remove(removedId);
            }

            //Init all of the IDs
            foreach (var id in humanIds) {
                InitHuman(id);
            }
        }

        public void InitHuman(long id) {
            HumanManager humanManager = WorldScripts.Instance.humanManager;

            //If we don't have this id in the dictionnary, add it
            if (!specializationPriorities.ContainsKey(id)) {
                specializationPriorities.Add(id, new Dictionary<ProfessionType, List<string>>());
            }

            foreach (var profession in defaultPriorities.Keys) {
                if (!specializationPriorities[id].ContainsKey(profession)) {
                    specializationPriorities[id].Add(profession, new List<string>());
                }

                //Remove all specializations that don't exist anymore
                specializationPriorities[id][profession].RemoveAll(x => !defaultPriorities[profession].Contains(x));

                //Add the missing ones
                foreach (var spec in defaultPriorities[profession]) {
                    if (!specializationPriorities[id][profession].Contains(spec))
                        specializationPriorities[id][profession].Add(spec);
                }
            }
        }

        public override void Update() {
            if (buryColonistFailCooldown.HasValue) {
                buryColonistFailCooldown -= Time.deltaTime;
                if (buryColonistFailCooldown < 0) buryColonistFailCooldown = null;
            }
        }
    }
}
