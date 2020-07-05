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
        public ProfessionDetailPanel ParentPanel { get; set; }
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
        private void Init(ProfessionType pType, List<ProfessionSpecializationDescription> specializations, Button upButton, Button downButton, Toggle toggle)
        {
            this.ProfessionType = pType;

            //Creates the list of specialization names
            _Specializations = specializations.Select(x=>x.name).ToList();
            _DetailPanels.Clear();

            //For each specialization, create a detail panel
            foreach (var spec in specializations)
            {
                GameObject specContainer = new GameObject();
                specContainer.AddComponent<CanvasRenderer>();
                specContainer.AddComponent<RectTransform>();

                HorizontalLayoutGroup hGroup = specContainer.AddComponent<HorizontalLayoutGroup>();
                hGroup.childAlignment = TextAnchor.MiddleLeft;
                hGroup.childForceExpandHeight = false;
                hGroup.childForceExpandWidth = false;
                hGroup.childControlHeight = true;
                hGroup.childControlWidth = false;

                SpecializationDetailsPanel detailsPanel = specContainer.AddComponent<SpecializationDetailsPanel>();
                detailsPanel.Init(this, spec, upButton, downButton, toggle);
                _DetailPanels.Add(detailsPanel);

                specContainer.transform.SetParent(this.transform);
            }
        }

        /// <summary>
        /// Creates the SpecializationPanel
        /// </summary>
        /// <param name="parent">Parent game object to add the panel to</param>
        /// <param name="profPanel">The original ProfessionDetailPanel</param>
        /// <param name="upButton">The template for the Up button</param>
        /// <param name="downButton">The template for the Down button</param>
        /// <param name="toggle">The template for the Toggle</param>
        /// <param name="pType">The type of the profession</param>
        /// <param name="specializations">The specializations of the profession</param>
        /// <returns></returns>
        public static GameObject Create(GameObject parent, ProfessionDetailPanel profPanel, Button upButton, Button downButton, Toggle toggle, ProfessionType pType, List<ProfessionSpecializationDescription> specializations)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<CanvasRenderer>();
            obj.AddComponent<RectTransform>();

            VerticalLayoutGroup vGroup = obj.AddComponent<VerticalLayoutGroup>();
            vGroup.padding.left = vGroup.padding.right = vGroup.padding.top = vGroup.padding.bottom = 5;
            vGroup.spacing = 0;
            vGroup.childAlignment = TextAnchor.UpperLeft;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;

            SpecializationPanel sp = obj.AddComponent<SpecializationPanel>();
            sp.ParentPanel = profPanel;
            sp.Init(pType, specializations, upButton, downButton, toggle);

            ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            obj.transform.SetParent(parent.transform);

            return obj;
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
    }
}
