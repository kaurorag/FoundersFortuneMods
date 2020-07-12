using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    [Serializable]
    public class AbsoluteProfessionPrioritiesMod : Mod
    {
        //[humanId][ProfessionType][specializations] (ordered by priority where 0=most important]
        public Dictionary<long, Dictionary<ProfessionType, List<String>>> specializationPriorities = new Dictionary<long, Dictionary<ProfessionType, List<string>>>();

        public override void Load()
        {
            Harmony harmony = new Harmony("AbsoluteProfessionPriorities");
            harmony.PatchAll();
        }

        public override void Start()
        {
            //If it's null, then we either loaded a new game or a game that hadn't had the mod yet
            if (specializationPriorities == null)
            {
                specializationPriorities = new Dictionary<long, Dictionary<ProfessionType, List<string>>>();
            }

            //Get all of the IDs of the current colonists
            HumanManager humanManager = WorldScripts.Instance.humanManager;
            List<long> humanIds = humanManager.GetHumans().Where(x => x.faction.GetFactionType() == FactionType.Colony).Select(x => x.GetID()).ToList();

            //Remove all of the saved IDs that are not in the colony anymore
            foreach (var removedId in specializationPriorities.Keys.Where(x => !humanIds.Contains(x)).ToArray())
            {
                specializationPriorities.Remove(removedId);
            }

            //Init all of the IDs
            foreach(var id in humanIds)
            {
                InitHuman(id);
            }
        }

        public void InitHuman(long id)
        {
            HumanManager humanManager = WorldScripts.Instance.humanManager;

            //If we don't have this id in the dictionnary, add it
            if (!specializationPriorities.ContainsKey(id))
                specializationPriorities.Add(id, new Dictionary<ProfessionType, List<string>>());

            //Get the human corresponding to this ID
            HumanAI colonist = humanManager.GetHumans().First(x => x.GetID() == id);

            //For each profession except NoJob and Soldier
            foreach (var profession in colonist.professionManager.professions.Values.Where(
                x => x.type != ProfessionType.NoJob && x.type != ProfessionType.Soldier))
            {
                //If we don't have the profession in the dictionary, add it
                if (!specializationPriorities[id].ContainsKey(profession.type))
                    specializationPriorities[id].Add(profession.type, new List<string>());

                List<string> colSpecs = profession.specializations.Keys.ToList();

                //Remove all of the specializations that do not exist anymore
                specializationPriorities[id][profession.type].RemoveAll(x => !colSpecs.Contains(x));

                //Add all of the specializations that are missing
                specializationPriorities[id][profession.type].AddRange(colSpecs.Where(x => !specializationPriorities[id][profession.type].Contains(x)));
            }
        }

        public override void Update()
        {
        }
    }
}
