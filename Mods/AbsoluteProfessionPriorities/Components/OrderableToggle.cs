using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WitchyMods.AbsoluteProfessionPriorities {
    public class OrderableToggle : MonoBehaviour {
        public Button UpButton;
        public Button DownButton;
        public InputField OrderInput;
        public Toggle Toggle;
        public Text ToggleText;

        public int Value {
            get { return Convert.ToInt32(OrderInput.text); }
            set {
                OrderInput.text = Mathf.Clamp(value, this.MinValue, this.MaxValue).ToString();
                UpButton.interactable = this.Value > this.MinValue;
                DownButton.interactable = this.Value < this.MaxValue;
            }
        }

        public int MinValue { get; set; } = 0;
        public int MaxValue { get; set; } = 10;
    }
}
