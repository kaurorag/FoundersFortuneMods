using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.UIImprovements {
    public class DayTimePanel : MonoBehaviour {
        public Text DayText;
        public Text TimeText;

        private string dayWord;
        private int daysPerSeasons;
        private float secondsPerDay;
        private float secondsPerMinute;
        private float secondsPerHour;

        private void Start() {
            dayWord = Localization.GetText("day");
            daysPerSeasons = ModHandler.mods.config.daysPerSeason;
            secondsPerHour = 1f / 24f;
            secondsPerMinute = 1f / 24f / 60f;
        }

        private void Update() {
            int day = (int)HarmonyLib.AccessTools.Field(typeof(SeasonManager), "day").GetValue(WorldScripts.Instance.seasonManager);
            int dayOfSeason = (day % daysPerSeasons) + 1;

            DayText.text = $"{dayWord} {dayOfSeason}/{daysPerSeasons}";

            float time = WorldScripts.Instance.dayTimeController.time;
            bool isAM = true;
            int hour = Mathf.FloorToInt(time / secondsPerHour);
            int minutes = Mathf.FloorToInt((time - hour * secondsPerHour) / secondsPerMinute / 5f) * 5;

            //time zero isn't midnight, it's 6AM
            hour = (hour + 6) % 24;
            
            if(hour > 12) {
                isAM = false;
                hour -= 12;
            }

            TimeText.text = $"{hour}:{minutes.ToString("00")} {(isAM ? "AM" : "PM")}";
        }
    }
}
