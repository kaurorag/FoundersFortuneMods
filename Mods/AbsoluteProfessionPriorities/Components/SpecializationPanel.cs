using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities {
    /// <summary>
    /// Replaces the original panel
    /// </summary>
    public class SpecializationPanel : MonoBehaviour {
        public GameObject DetailsPanelTemplate;
        public Transform Content;

#if !MODKIT
        //public ProfessionDetailPanel ParentPanel { get; set; }
        public ProfessionType ProfessionType { get; set; }
        public HumanAI Human { get; set; }

        private List<SpecializationDetailsPanel> DetailPanels = new List<SpecializationDetailsPanel>();

        /// <summary>
        /// Initializes the panel
        /// </summary>
        /// <param name="pType">The profession type</param>
        /// <param name="specializations">Specializations of the profession</param>
        /// <param name="upButton">The template of the Up button</param>
        /// <param name="downButton">The template of the Down button</param>
        /// <param name="toggle">The template of the Toggle</param>
        public void Init(ProfessionType pType, List<SpecializationDescriptor> specializations) {
            this.ProfessionType = pType;

            //For each specialization, create a detail panel
            foreach (var spec in specializations) {
                GameObject specObj = GameObject.Instantiate(this.DetailsPanelTemplate, this.Content);
                SpecializationDetailsPanel panel = specObj.GetComponent<SpecializationDetailsPanel>();
                panel.Init(this, spec, specializations.Count - 1);
                panel.SpecializationToggle.UpButton.onClick.AddListener(() => PriorityButtonUpClicked(panel));
                panel.SpecializationToggle.DownButton.onClick.AddListener(() => PriorityButtonDownClicked(panel));
                DetailPanels.Add(panel);
            }

            OrderDetailPanels();
        }

        private void PriorityButtonUpClicked(SpecializationDetailsPanel panel) {
            if (Input.GetButton("Shift")) {
                panel.SpecializationToggle.Value--;
            } else {
                int index = panel.transform.GetSiblingIndex();
                SpecializationDetailsPanel previousPanel = this.Content.GetChild(index - 1).GetComponent<SpecializationDetailsPanel>();

                bool mustPush = panel.SpecializationToggle.Value == previousPanel.SpecializationToggle.Value;

                panel.SpecializationToggle.Value--;
                previousPanel.GetComponentInChildren<OrderableToggle>().Value++;

                if (mustPush) {
                    for (int i = index; i < this.Content.childCount; i++) {
                        this.Content.GetChild(i).GetComponent<SpecializationDetailsPanel>().SpecializationToggle.Value++;
                    }
                }
            }

            this.OrderDetailPanels();
        }

        private void PriorityButtonDownClicked(SpecializationDetailsPanel panel) {
            if (Input.GetButton("Shift")) {
                panel.SpecializationToggle.Value++;
            } else {
                int index = panel.transform.GetSiblingIndex();
                SpecializationDetailsPanel nextPanel = this.Content.GetChild(index + 1).GetComponent<SpecializationDetailsPanel>();

                bool mustPush = panel.SpecializationToggle.Value == nextPanel.SpecializationToggle.Value;
                panel.SpecializationToggle.Value++;
                nextPanel.GetComponentInChildren<OrderableToggle>().Value--;

                if (mustPush) {
                    for (int i = 0; i <= index; i++) {
                        this.Content.GetChild(i).GetComponent<SpecializationDetailsPanel>().SpecializationToggle.Value--;
                    }
                }
            }

            this.OrderDetailPanels();
        }

        private void OrderDetailPanels() {
            int index = 0;
            foreach (var panel in this.DetailPanels.OrderBy(x => x.SpecializationToggle.Value)) {
                panel.transform.SetSiblingIndex(index);
                index++;
            }
        }

        public void SetHuman(HumanAI human) {
            this.Human = human;

            //Get the instance of the mod and inits the human
            AbsoluteProfessionPrioritiesMod.Instance.InitColonist(this.Human);

            if (!AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[this.Human.GetID()].ContainsKey(this.ProfessionType)) return;

            foreach (var panel in this.DetailPanels) {
                panel.InitForHuman(this.Human);
            }

            OrderDetailPanels();
        }
#endif
    }
}
