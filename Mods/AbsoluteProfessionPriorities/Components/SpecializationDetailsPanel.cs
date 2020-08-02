using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WitchyMods.AbsoluteProfessionPriorities {
    /// <summary>
    /// Contains the priority buttons and text of a specialization
    /// </summary>
    public class SpecializationDetailsPanel : MonoBehaviour {
        /// <summary>
        /// The toggle control of the specialization
        /// </summary>
        public OrderableToggle SpecializationToggle;
        public Button SubExpandButton;
        public Button SubCollapseButton;
        public GameObject SubSpecializationsContainer;
        public OrderableToggle SubSpecializationToggleTemplate;
        public Toggle AutoPriorityToggle;

#if !MODKIT
        private HumanAI human;
        private string SpecializationName;
        private bool canAutoManage = true;
        public Specialization Specialization;

        private SpecializationPanel ParentPanel = null;

        private Dictionary<string, OrderableToggle> SubSpecToggles = new Dictionary<string, OrderableToggle>();

        private bool inInit = false;
#endif

#if !MODKIT

        public void Init(SpecializationPanel parent, SpecializationDescriptor spec, int maxValue) {
            this.ParentPanel = parent;
            this.SpecializationName = spec.Name;

            canAutoManage = spec.CanAutoManageSubSpecializations;
            this.AutoPriorityToggle.gameObject.SetActive(spec.CanAutoManageSubSpecializations);

            this.SpecializationToggle.MaxValue = maxValue;
            this.SpecializationToggle.ToggleText.text = spec.GetDisplayName();

            if (spec.SubSpecializations.Count == 0) {
                SubExpandButton.gameObject.SetActive(false);
                SubCollapseButton.gameObject.SetActive(false);
                AutoPriorityToggle.gameObject.SetActive(false);
            } else {
                foreach (var sub in spec.SubSpecializations.Values) {
                    OrderableToggle subToggle = GameObject.Instantiate(this.SubSpecializationToggleTemplate.gameObject).GetComponent<OrderableToggle>();
                    subToggle.transform.SetParent(this.SubSpecializationsContainer.transform);
                    subToggle.name = sub.Name;
                    subToggle.ToggleText.text = sub.GetDisplayName();
                    subToggle.MaxValue = spec.SubSpecializations.Count - 1;
                    subToggle.gameObject.SetActive(true);

                    subToggle.UpButton.onClick.AddListener(() => SubSpecializationToggle_ButtonUpClicked(subToggle));
                    subToggle.DownButton.onClick.AddListener(() => SubSpecializationToggle_ButtonDownClicked(subToggle));
                    subToggle.Toggle.onValueChanged.AddListener((x) => SubSpecializationToggle_ToggleValueChanged(subToggle));
                    subToggle.OrderInput.onValueChanged.AddListener((x) => SubSpecializationToggle_InputValueChanged(subToggle));

                    this.SubSpecToggles.Add(sub.Name, subToggle);
                }

                OrderSubSpecializations();
            }

            this.SpecializationToggle.OrderInput.onValueChanged.AddListener((x) => SpecializationToggle_InputValueChanged());
            this.SpecializationToggle.Toggle.onValueChanged.AddListener((x) => SpecializationToggle_ToggleValueChanged());

            this.SubCollapseButton.onClick.AddListener(SpecializationToggle_CollapseButtonClicked);
            this.SubExpandButton.onClick.AddListener(SpecializationToggle_ExpandButtonClicked);

            this.AutoPriorityToggle.onValueChanged.AddListener(this.AutoManageToggle_ValueChanged);
        }

        public void InitForHuman(HumanAI human) {
            inInit = true;
            this.human = human;
            this.Specialization = AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[human.GetID()][this.ParentPanel.ProfessionType][this.SpecializationName];

            this.SpecializationToggle.Value = this.Specialization.Priority;
            this.SpecializationToggle.Toggle.isOn = this.Specialization.Active;

            foreach (var sub in this.Specialization.SubSpecializations.Values) {
                this.SubSpecToggles[sub.Name].Toggle.isOn = sub.Active;
                this.SubSpecToggles[sub.Name].Value = sub.Priority;
            }

            this.OrderSubSpecializations();
            this.UpdateOrderButtonsState();

            this.AutoPriorityToggle.isOn = this.Specialization.AutoManageSubSpecializations;
            inInit = false;
        }

        private void AutoManageToggle_ValueChanged(bool isOn) {
            if (inInit) return;

            this.Specialization.AutoManageSubSpecializations = this.AutoPriorityToggle.isOn;
            this.UpdateOrderButtonsState();
        }

        private void SpecializationToggle_InputValueChanged() {
            if (inInit) return;
            this.Specialization.Priority = Convert.ToInt32(this.SpecializationToggle.OrderInput.text);
        }

        private void SpecializationToggle_ToggleValueChanged() {
            if (inInit) return;

            this.Specialization.Active = this.SpecializationToggle.Toggle.isOn;
            ParentPanel.Human.professionManager.SetSpecializationActive(this.Specialization.Name, ParentPanel.ProfessionType, this.SpecializationToggle.Toggle.isOn, ParentPanel.Human);
        }

        private void SpecializationToggle_ExpandButtonClicked() {
            this.SubExpandButton.gameObject.SetActive(false);
            this.SubCollapseButton.gameObject.SetActive(true);
            this.SubSpecializationsContainer.SetActive(true);
        }

        private void SpecializationToggle_CollapseButtonClicked() {
            this.SubExpandButton.gameObject.SetActive(true);
            this.SubCollapseButton.gameObject.SetActive(false);
            this.SubSpecializationsContainer.SetActive(false);
        }

        private void SubSpecializationToggle_InputValueChanged(OrderableToggle toggle) {
            if (inInit) return;

            this.Specialization.SubSpecializations[toggle.name].Priority = toggle.Value;
            OrderSubSpecializations();
        }

        private void SubSpecializationToggle_ToggleValueChanged(OrderableToggle toggle) {
            if (inInit) return;

            this.Specialization.SubSpecializations[toggle.name].Active = toggle.Toggle.isOn;

            if (!toggle.Toggle.isOn) {
                DebugLogger.Log($"Profession:{this.Specialization.Profession}");
                foreach (var interaction in ProfessionManager.workInteractions[this.Specialization.Profession]) {
                    DebugLogger.Log($"\tinteraction:{interaction}");
                }
                this.human.AbortInteractionsWhere(x =>
                ProfessionManager.workInteractions[this.Specialization.Profession].Contains(x.interaction));
            }
        }

        private void SubSpecializationToggle_ButtonUpClicked(OrderableToggle toggle) {
            if (Input.GetButton("Shift")) {
                toggle.Value--;
            } else {
                int index = toggle.transform.GetSiblingIndex();
                OrderableToggle previousToggle = this.SubSpecializationsContainer.transform.GetChild(index - 1).GetComponent<OrderableToggle>();

                bool mustPush = toggle.Value == previousToggle.Value;

                toggle.Value--;
                previousToggle.GetComponentInChildren<OrderableToggle>().Value++;

                if (mustPush) {
                    for (int i = index; i < this.SubSpecializationsContainer.transform.childCount; i++) {
                        this.SubSpecializationsContainer.transform.GetChild(i).GetComponent<SpecializationDetailsPanel>().SpecializationToggle.Value++;
                    }
                }
            }

            this.OrderSubSpecializations();
        }

        private void SubSpecializationToggle_ButtonDownClicked(OrderableToggle toggle) {
            if (Input.GetButton("Shift")) {
                toggle.Value++;
            } else {
                int index = toggle.transform.GetSiblingIndex();
                OrderableToggle nextToggle = this.SubSpecializationsContainer.transform.GetChild(index - 1).GetComponent<OrderableToggle>();

                bool mustPush = toggle.Value == nextToggle.Value;

                toggle.Value++;
                nextToggle.GetComponentInChildren<OrderableToggle>().Value--;

                if (mustPush) {
                    for (int i = 0; i <= index; i++) {
                        this.SubSpecializationsContainer.transform.GetChild(i).GetComponent<SpecializationDetailsPanel>().SpecializationToggle.Value--;
                    }
                }
            }

            this.OrderSubSpecializations();
        }

        private void OrderSubSpecializations() {
            int index = 0;
            foreach(var subToggle in this.SubSpecToggles.Values.OrderBy(x => x.Value)) {
                subToggle.transform.SetSiblingIndex(index);
                index++;
            }
        }

        private void UpdateOrderButtonsState() {
            foreach (var subToggle in this.SubSpecToggles.Values) {
                subToggle.transform.Find("ButtonContainer").gameObject.SetActive(!canAutoManage || !this.Specialization.AutoManageSubSpecializations);
            }
        }
#endif
    }
}
