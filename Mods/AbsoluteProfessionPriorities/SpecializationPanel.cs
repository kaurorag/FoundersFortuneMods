using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    /// <summary>
    /// Replaces the original panel
    /// </summary>
    public class SpecializationPanel : MonoBehaviour
    {
        public GameObject DetailsPanelTemplate;
        public Transform Content;

#if !MODKIT
        //public ProfessionDetailPanel ParentPanel { get; set; }
        public ProfessionType ProfessionType { get; set; }
        public HumanAI Human { get; set; }

        private List<string> _Specializations = null;
        private List<SpecializationDetailsPanel> _DetailPanels = new List<SpecializationDetailsPanel>();

        /// <summary>
        /// Initializes the panel
        /// </summary>
        /// <param name="pType">The profession type</param>
        /// <param name="specializations">Specializations of the profession</param>
        /// <param name="upButton">The template of the Up button</param>
        /// <param name="downButton">The template of the Down button</param>
        /// <param name="toggle">The template of the Toggle</param>
        public void Init(ProfessionType pType, List<ProfessionSpecializationDescription> specializations)
        {
            this.ProfessionType = pType;

            //Creates the list of specialization names
            _Specializations = specializations.Select(x=>x.name).ToList();

            //For each specialization, create a detail panel
            foreach (var spec in specializations)
            {
                GameObject specObj = GameObject.Instantiate(this.DetailsPanelTemplate, this.Content);
                SpecializationDetailsPanel panel = specObj.GetComponent<SpecializationDetailsPanel>();
                panel.ParentPanel = this;
                panel.Specialization = spec.name;
                panel.SpecializationText.text = spec.GetNameInGame();
                _DetailPanels.Add(panel);
            }
        }

        public void SetHuman(HumanAI human)
        {
            this.Human = human;
            UpdateDetails();
        }

        public void UpdateDetails()
        {
            //Get the instance of the mod and inits the human
            AbsoluteProfessionPrioritiesMod mod = ModHandler.mods.scriptMods.OfType<AbsoluteProfessionPrioritiesMod>().First();
            mod.InitHuman(this.Human.GetID());

            //For each specialization, update the detail panel
            List<String> orderedSpecs = mod.specializationPriorities[this.Human.GetID()][this.ProfessionType];

            for (int i = 0; i < orderedSpecs.Count; i++)
            {
                var detailsPanel = _DetailPanels.First(x => x.Specialization == orderedSpecs[i]);

                detailsPanel.gameObject.transform.SetSiblingIndex(i);
                detailsPanel.Toggle.SetValue(this.Human.professionManager.GetProfession(this.ProfessionType).HasSpecialization(orderedSpecs[i]));
                detailsPanel.SetUpButtonEnabled(i != 0);
                detailsPanel.SetDownButtonEnabled(i < orderedSpecs.Count - 1);
            }
        }
#endif
    }
}
