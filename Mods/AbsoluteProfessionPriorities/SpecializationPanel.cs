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

            //Get the instance of the mod and inits the human
            AbsoluteProfessionPrioritiesMod.Instance.InitHuman(this.Human.GetID());

            UpdateDetails();
        }

        public void UpdateDetails()
        {
            if (!AbsoluteProfessionPrioritiesMod.Instance.specializationPriorities[this.Human.GetID()].ContainsKey(this.ProfessionType)) return;

            //For each specialization, update the detail panel
            List<String> orderedSpecs = AbsoluteProfessionPrioritiesMod.Instance.specializationPriorities[this.Human.GetID()][this.ProfessionType];

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
