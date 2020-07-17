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
        /// The toggle control of the specialization
        /// </summary>
        public Toggle Toggle;
        public Text SpecializationText;
        public Button UpButton;
        public Button DownButton;

#if !MODKIT
        /// <summary>
        /// Name of the specialization
        /// </summary>
        public String Specialization { get; set; }

        public SpecializationPanel ParentPanel = null;

#endif
        /// <summary>
        /// Increases the priority of the specialization
        /// </summary>
        public void OrderUp()
        {
#if !MODKIT
            AbsoluteProfessionPrioritiesMod mod = AbsoluteProfessionPrioritiesMod.Instance;
            List<String> specs = mod.specializationPriorities[ParentPanel.Human.GetID()][ParentPanel.ProfessionType];

            int curIndex = specs.IndexOf(this.Specialization);

            String old = specs[curIndex - 1];
            specs[curIndex - 1] = this.Specialization;
            specs[curIndex] = old;

            //Reorders the UI elements
            ParentPanel.UpdateDetails();
#endif
        }

        /// <summary>
        /// Lowers the priority of the specialization
        /// </summary>
        public void OrderDown()
        {
#if !MODKIT
            AbsoluteProfessionPrioritiesMod mod = AbsoluteProfessionPrioritiesMod.Instance;
            List<String> specs = mod.specializationPriorities[ParentPanel.Human.GetID()][ParentPanel.ProfessionType];

            int curIndex = specs.IndexOf(this.Specialization);

            String old = specs[curIndex + 1];
            specs[curIndex + 1] = this.Specialization;
            specs[curIndex] = old;

            //Reorders the UI elements
            ParentPanel.UpdateDetails();
#endif
        }

        /// <summary>
        /// If the toggle value changes, tell the profession manager
        /// </summary>
        /// <param name="value"></param>
        public void ToggleValueChanged(bool value)
        {
#if !MODKIT
            ParentPanel.Human.professionManager.SetSpecializationActive(this.Specialization, ParentPanel.ProfessionType, Toggle.isOn, ParentPanel.Human);
#endif
        }

#if !MODKIT
        public void SetUpButtonEnabled(bool enabled)
        {
            UpButton.enabled = enabled;
            UpButton.image.color = enabled ? Color.white : Color.gray;
        }

        public void SetDownButtonEnabled(bool enabled)
        {
            DownButton.enabled = enabled;
            DownButton.image.color = enabled ? Color.white : Color.gray;
        }
#endif
    }
}
