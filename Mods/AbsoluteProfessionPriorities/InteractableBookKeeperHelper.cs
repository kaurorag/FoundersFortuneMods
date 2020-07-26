using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {
    public class InteractableBookkeeperHelper {
        private InteractableBookkeeper i = null;

        public InteractableBookkeeperHelper(InteractableBookkeeper instance) {
            i = instance;
        }

        public Dictionary<Interaction, List<InteractableData>> interactableDict {
            get {
                return (Dictionary<Interaction, List<InteractableData>>)
              AccessTools.Field(typeof(InteractableBookkeeper), "interactableDict").GetValue(i);
            }
        }

        public Dictionary<InteractionWithFilter, List<InteractableData>> interactableFilterDict {
            get {
                return (Dictionary<InteractionWithFilter, List<InteractableData>>)
                AccessTools.Field(typeof(InteractableBookkeeper), "interactableFilterDict").GetValue(i);
            }
        }

        public IEnumerable<Interactable> GetNearbyInteractablesCustom(Interaction interaction, List<InteractionRestriction> interactionRestrictions, Vector3 point, float maxDistance, Faction actorFaction) {
            List<InteractionWithFilter> interactionsWithFilters = new List<InteractionWithFilter>();
            if (interactionRestrictions != null) {
                foreach (InteractionRestriction restriction in interactionRestrictions) {
                    interactionsWithFilters.AddRange(restriction.GetFilters().Select(x => new InteractionWithFilter(interaction, x)));
                }
            }

            if (interactionsWithFilters.Count == 0) {
                interactionsWithFilters.Add(new InteractionWithFilter(interaction));
            }

            List<InteractableData> filteredData = new List<InteractableData>();

            if (interactableDict.ContainsKey(interaction))
                filteredData.AddRange(interactableDict[interaction]);

            foreach (var intWithFilter in interactionsWithFilters) {
                if (interactableFilterDict.ContainsKey(intWithFilter))
                    filteredData.AddRange(interactableFilterDict[intWithFilter]);
            }

            foreach (var d in filteredData.Select(x => new { data = x, distance = GetDistance(x, point, actorFaction, maxDistance) })
                .Where(x => x.distance != Int32.MaxValue)
                .OrderBy(x => x.distance)) {
                yield return d.data.interactable;
            }
        }

        private int GetDistance(InteractableData data, Vector3 point, Faction actorFaction, float maxDistance) {
            Vector3 dataPosition = data.dynamic ? data.interactable.GetWorldPosition() : data.position;
            bool wrongFaction = data.ownerFaction != null && data.ownerFaction != actorFaction;
            float limitDistance = (wrongFaction && maxDistance != -1) ? maxDistance * 0.3f : maxDistance;
            float distance = Vector3.Distance(dataPosition, point);

            if (maxDistance == -1 || distance < limitDistance)
                return Mathf.FloorToInt(distance);

            return Int32.MaxValue;
        }

        public static IEnumerable<Interactable> SearchForInteractablesWithInteraction(HumanAI human, Interaction interaction, float distance, List<InteractionRestriction> restrictions = null, bool probablyReachablesOnly = false, bool objectInteraction = false, Vector3 positionOverride = default(Vector3)) {
            InteractableBookkeeperHelper helper = new InteractableBookkeeperHelper(InteractableBookkeeper.Instance);

            Vector3 position = (positionOverride == default(Vector3)) ? human.GetPosition() : positionOverride;
            foreach(var interactable in helper.GetNearbyInteractablesCustom(interaction, restrictions, position, distance, human.faction)) {
                if (probablyReachablesOnly && !human.humanNavigation.IsInteractableProbablyReachable(interactable.GetGameRepresentationID())) { continue; }
                if (!interactable.IsPossibleInteraction(interaction, human, true, objectInteraction)) {continue; }
                if (restrictions != null && restrictions.Any(x => x.IsRestrictedFast(interactable, interaction, human, true))) {continue; }

                yield return interactable;
            }
        }
    }


}
