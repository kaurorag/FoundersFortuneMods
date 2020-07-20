using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.UIImprovements
{
    [Serializable()]
    public class UIImprovementsMod : Mod
    {
        [NonSerialized]
        public Furniture SelectedFurniture = null;

        [NonSerialized]
        public EquipmentOverview EquipmentOverview = null;

        [NonSerialized]
        public static UIImprovementsMod Instance = null;

        [NonSerialized]
        private NotificationItem _MigrantNotification = null;

        [NonSerialized]
        private String _LastText = null;

        [NonSerialized]
        private String _TimerTextTemplate = null;

        public override void Load()
        {
            Harmony harmony = new Harmony("UIImprovements");
            harmony.PatchAll();
        }

        public override void Start()
        {
            Instance = this;
            _TimerTextTemplate = Localization.GetText("witchy_UIImprovements_MigrationTimer");
        }

        public override void Update()
        {
            String timeText = GetNotificationText();

            if (timeText != _LastText)
            {
                String text = String.Format(_TimerTextTemplate, timeText);

                if (_MigrantNotification != null && _MigrantNotification.isActiveAndEnabled)
                    _MigrantNotification.SetText(text);

                else if (ReadyForMigrant())
                    _MigrantNotification = new NotificationConfig("witchy_newMigrant", text, UISound.NotificationGeneric, null, new Func<bool>(this.IsNotificationObsolete), null, -1, false).Show();

                else
                {
                    _LastText = null;
                    return;
                }

                _LastText = timeText;
            }
        }

        private bool ReadyForMigrant()
        {
            HumanManager hManager = WorldScripts.Instance.humanManager;
            return (hManager.AreExpectationsMet() &&
                !hManager.HasTooManyColonists() &&
                WorldScripts.Instance.furnitureFactory.GetModules<AttentionModule>().Any<AttentionModule>(x => x.lit));
        }

        private bool IsNotificationObsolete()
        {
            return WorldScripts.Instance.incidentManager.currentEvents.Any<GameEvent>(x => x.GetEventType() == GameEventType.Migrant) || !ReadyForMigrant();
        }

        private String GetNotificationText()
        {
            IncidentManager incidentManager = WorldScripts.Instance.incidentManager;
            string second = Mathf.FloorToInt(incidentManager.timeToNextMigrant / 60f) + ":" + Mathf.FloorToInt(incidentManager.timeToNextMigrant % 60f).ToString("D2");
            return second;
        }
    }
}
