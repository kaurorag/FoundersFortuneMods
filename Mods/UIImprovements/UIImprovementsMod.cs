using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.UIImprovements {
    [Serializable()]
    public class UIImprovementsMod : Mod {
        [NonSerialized]
        public Furniture SelectedFurniture = null;

        [NonSerialized]
        public EquipmentOverview EquipmentOverview = null;

        [NonSerialized]
        public static UIImprovementsMod Instance = null;

        [NonSerialized]
        private NotificationItem _MigrantNotification = null;

        [NonSerialized]
        private float _LastMigrantTime = -1;

        [NonSerialized]
        private String _TimerTextTemplate = null;


        public override void Load() {
            Harmony harmony = new Harmony("UIImprovements");
            harmony.PatchAll();
        }

        public override void Start() {
            Instance = this;
            _TimerTextTemplate = Localization.GetText("witchy_UIImprovements_MigrationTimer");
        }

        public override void Update() {
            if (WorldScripts.Instance.incidentManager.timeToNextMigrant != _LastMigrantTime) {
                _LastMigrantTime = WorldScripts.Instance.incidentManager.timeToNextMigrant;

                if (_MigrantNotification != null && _MigrantNotification.isActiveAndEnabled) {
                    _MigrantNotification.SetText(GetNotificationText());

                } else if (ReadyForMigrant())
                    _MigrantNotification = new NotificationConfig("witchy_newMigrant", GetNotificationText(), UISound.NotificationGeneric, null, new Func<bool>(this.IsNotificationObsolete), null, -1, false).Show();
            }
        }

        private bool ReadyForMigrant() {
            HumanManager hManager = WorldScripts.Instance.humanManager;
            return (hManager.AreExpectationsMet() &&
                !hManager.HasTooManyColonists() &&
                WorldScripts.Instance.furnitureFactory.GetModules<AttentionModule>().Any<AttentionModule>(x => x.lit));
        }

        private bool IsNotificationObsolete() {
            return WorldScripts.Instance.incidentManager.currentEvents.Any<GameEvent>(x => x.GetEventType() == GameEventType.Migrant) || !ReadyForMigrant();
        }

        private string GetNotificationText() {
            return String.Format(_TimerTextTemplate, 
                Mathf.FloorToInt(WorldScripts.Instance.incidentManager.timeToNextMigrant / 60f), 
                Mathf.FloorToInt(WorldScripts.Instance.incidentManager.timeToNextMigrant % 60f));
        }
    }
}
