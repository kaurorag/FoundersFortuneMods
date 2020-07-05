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
    /// Contains the priority buttons and text of a specialization
    /// </summary>
    public class SpecializationDetailsPanel : MonoBehaviour
    {
        /// <summary>
        /// Name of the specialization
        /// </summary>
        public String Specialization { get; set; }

        /// <summary>
        /// The toggle control of the specialization
        /// </summary>
        public Toggle Toggle { get; private set; }

        private Button _UpButton = null;
        private Button _DownButton = null;

        private SpecializationPanel _ParentPanel = null;

        /// <summary>
        /// Initializes the panel
        /// </summary>
        /// <param name="parent">Parent panel</param>
        /// <param name="specialization">The description of the specialization</param>
        /// <param name="upButton"></param>
        /// <param name="downButton"></param>
        /// <param name="toggle"></param>
        public void Init(SpecializationPanel parent, ProfessionSpecializationDescription specialization, Button upButton, Button downButton, Toggle toggle)
        {
            _ParentPanel = parent;
            this.Specialization = specialization.name;

            //Wraps the buttons in a panel so they are aligned
            GameObject buttonPanel = new GameObject();
            buttonPanel.AddComponent<CanvasRenderer>();
            buttonPanel.AddComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 40f);

            HorizontalLayoutGroup buttonVGroup = buttonPanel.AddComponent<HorizontalLayoutGroup>();

            buttonVGroup.childControlHeight = false;
            buttonVGroup.childControlWidth = false;
            buttonVGroup.childForceExpandHeight = false;
            buttonVGroup.childForceExpandWidth = false;
            buttonVGroup.childAlignment = TextAnchor.UpperCenter;

            buttonPanel.transform.SetParent(this.transform);

            //Models the buttons on the existing buttons for the profession priority
            GameObject upButtonObj = GameObject.Instantiate(upButton.gameObject, buttonPanel.transform);
            GameObject downButtonObj = GameObject.Instantiate(downButton.gameObject, buttonPanel.transform);

            _UpButton = upButtonObj.GetComponent<Button>();
            _DownButton = downButtonObj.GetComponent<Button>();

            //Adds the listeners
            _UpButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OrderUp));
            _DownButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OrderDown));

            //Models the toggle on the existing toggle
            GameObject toggleObj = GameObject.Instantiate(toggle.gameObject, this.transform);
            toggleObj.transform.Find("Label").gameObject.GetComponent<Text>().text = specialization.GetNameInGame();

            this.Toggle = toggleObj.GetComponent<Toggle>();

            //Adds the listener
            this.Toggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(ToggleValueChanged));

            //Changes some values of the layout group so that the toggle is properly aligned
            HorizontalLayoutGroup toggleHGroup = toggleObj.GetComponent<HorizontalLayoutGroup>();
            toggleHGroup.childControlHeight = false;
            toggleHGroup.childForceExpandHeight = false;
            toggleHGroup.padding.left = 0;
            toggleHGroup.spacing = 2;

            //We set the width so we fit in the window
            toggleObj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400f);
        }

        /// <summary>
        /// Increases the priority of the specialization
        /// </summary>
        public void OrderUp()
        {
            AbsoluteProfessionPrioritiesMod mod = ModHandler.mods.scriptMods.OfType<AbsoluteProfessionPrioritiesMod>().First();
            List<String> specs = mod.specializationPriorities[_ParentPanel.Human.GetID()][_ParentPanel.ProfessionType];

            int curIndex = specs.IndexOf(this.Specialization);

            String old = specs[curIndex - 1];
            specs[curIndex - 1] = this.Specialization;
            specs[curIndex] = old;

            //Reorders the UI elements
            _ParentPanel.UpdateDetails();
        }

        /// <summary>
        /// Lowers the priority of the specialization
        /// </summary>
        public void OrderDown()
        {
            AbsoluteProfessionPrioritiesMod mod = ModHandler.mods.scriptMods.OfType<AbsoluteProfessionPrioritiesMod>().First();
            List<String> specs = mod.specializationPriorities[_ParentPanel.Human.GetID()][_ParentPanel.ProfessionType];

            int curIndex = specs.IndexOf(this.Specialization);

            String old = specs[curIndex + 1];
            specs[curIndex + 1] = this.Specialization;
            specs[curIndex] = old;

            //Reorders the UI elements
            _ParentPanel.UpdateDetails();
        }

        /// <summary>
        /// If the toggle value changes, tell the profession manager
        /// </summary>
        /// <param name="value"></param>
        public void ToggleValueChanged(bool value)
        {
            _ParentPanel.Human.professionManager.SetSpecializationActive(this.Specialization, _ParentPanel.ProfessionType, value, _ParentPanel.Human);
        }

        public void SetUpButtonEnabled(bool enabled)
        {
            _UpButton.enabled = enabled;
            _UpButton.image.color = enabled ? Color.white : Color.gray;
        }

        public void SetDownButtonEnabled(bool enabled)
        {
            _DownButton.enabled = enabled;
            _DownButton.image.color = enabled ? Color.white : Color.gray;
        }
    }
}
