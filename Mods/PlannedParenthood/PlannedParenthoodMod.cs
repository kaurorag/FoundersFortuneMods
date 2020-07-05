using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.PlannedParenthood
{
    public class PlannedParenthoodMod : Mod
    {
        public static bool InAddChild = false;

        public override void Load()
        {
            Harmony harmony = new Harmony("PlannedParenthood");
            harmony.PatchAll();

            InAddChild = false;
        }

        public override void Start()
        {
            
        }

        public override void Update()
        {
            
        }

        public static void SetupBirthdayPanel(BirthdayChildPanel bdPanel)
        {
            Transform panel = bdPanel.transform.GetChild(0);

            GameObject btnContainer = SetupButtons(bdPanel, panel);

            TextLocalization txtLoc = panel.Find("Title").GetComponentInChildren<TextLocalization>();
            txtLoc.key = "witchy_PlannedParenthood_BirthdayPanelTitle";
            txtLoc.UpdateText();

            GameObject boyWrapper = SetupBoyPanel(bdPanel, panel);

            GameObject girlWrapper = SetupGirlPanel(bdPanel, panel, boyWrapper);

            int descIndex = bdPanel.descriptionText.transform.GetSiblingIndex();
            boyWrapper.transform.SetSiblingIndex(descIndex + 1);
            girlWrapper.transform.SetSiblingIndex(descIndex + 2);
            btnContainer.transform.SetSiblingIndex(descIndex + 3);
        }

        private static GameObject SetupGirlPanel(BirthdayChildPanel bdPanel, Transform panel, GameObject boyWrapper)
        {
            GameObject girlWrapper = GameObject.Instantiate(boyWrapper, panel);
            girlWrapper.name = "GirlPanel";

            GameObject girlLabel = girlWrapper.transform.GetChild(0).gameObject;
            girlLabel.name = "GirlLabel";
            girlLabel.GetComponent<Text>().text = Localization.GetText("witchy_PlannedParenthood_BirthdayPanelGirlLabel");

            GameObject girlInput = girlWrapper.transform.GetChild(1).gameObject;
            girlInput.name = "GirlInput";
            girlInput.gameObject.GetComponent<InputField>().onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((x) => PlannedParenthoodMod.InputChanged(bdPanel)));

            return girlWrapper;
        }

        private static GameObject SetupBoyPanel(BirthdayChildPanel bdPanel, Transform panel)
        {
            GameObject boyWrapper = new GameObject("BoyPanel", typeof(CanvasRenderer), typeof(RectTransform), typeof(VerticalLayoutGroup));
            boyWrapper.transform.SetParent(panel);

            VerticalLayoutGroup vGroup = boyWrapper.GetComponent<VerticalLayoutGroup>();
            vGroup.spacing = 5;
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childControlHeight = true;
            vGroup.childControlWidth = true;
            vGroup.childForceExpandWidth = true;

            GameObject boyLabel = new GameObject("BoyLabel", typeof(CanvasRenderer), typeof(RectTransform), typeof(Text));

            Text txtBoy = boyLabel.GetComponent<Text>();
            txtBoy.alignment = TextAnchor.MiddleLeft;
            txtBoy.color = Color.white;
            txtBoy.font = bdPanel.descriptionText.font;
            txtBoy.fontSize = bdPanel.descriptionText.fontSize;
            txtBoy.text = Localization.GetText("witchy_PlannedParenthood_BirthdayPanelBoyLabel");

            boyLabel.transform.SetParent(boyWrapper.transform);

            GameObject boyInput = panel.Find("InputField").gameObject;
            boyInput.name = "BoyInput";
            boyInput.gameObject.GetComponent<InputField>().onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((x) => PlannedParenthoodMod.InputChanged(bdPanel)));
            boyInput.transform.SetParent(boyWrapper.transform);

            return boyWrapper;
        }

        private static GameObject SetupButtons(BirthdayChildPanel bdPanel, Transform panel)
        {
            GameObject btnContainer = new GameObject("ButtonContainer", typeof(CanvasRenderer), typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnContainer.transform.SetParent(panel);

            HorizontalLayoutGroup hGroup = btnContainer.GetComponent<HorizontalLayoutGroup>();
            hGroup.spacing = 20;
            hGroup.childAlignment = TextAnchor.MiddleCenter;

            GameObject acceptButtonObj = panel.Find("ApplyButton").gameObject;
            GameObject rejectButtonObj = GameObject.Instantiate(acceptButtonObj);

            acceptButtonObj.GetComponentInChildren<Text>().text = Localization.GetText("witchy_PlannedParenthood_BirthdayPanelAcceptText");
            rejectButtonObj.GetComponentInChildren<Text>().text = Localization.GetText("witchy_PlannedParenthood_BirthdayPanelRejectText");

            acceptButtonObj.transform.SetParent(btnContainer.transform);
            rejectButtonObj.transform.SetParent(btnContainer.transform);

            acceptButtonObj.transform.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                PlannedParenthoodMod.AcceptClicked(bdPanel);
            }));

            rejectButtonObj.transform.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                PlannedParenthoodMod.RejectClicked(bdPanel);
            }));
            return btnContainer;
        }

        public static void AcceptClicked(BirthdayChildPanel bdPanel)
        {
            bdPanel.gameObject.SetActive(false);
            
            HumanAI oldChild = (HumanAI)AccessTools.Field(typeof(BirthdayChildPanel), "child").GetValue(bdPanel);
            
            HumanConfiguration config = new HumanConfiguration(oldChild.humanType, AgeLevel.Child, 0f, oldChild.mother, oldChild.father);

            Relationship relationship = WorldScripts.Instance.relationshipManager.GetRelationship(oldChild.father, oldChild.mother);

            HumanAI newChild = WorldScripts.Instance.humanManager.SpawnHuman(config, relationship.GetFirstHuman().GetPosition(), relationship.GetFirstHuman().faction, true, null);
            newChild.mother.onHadChild.Invoke(newChild);
            newChild.father.onHadChild.Invoke(newChild);

            relationship.HadChild();

            WorldScripts.Instance.relationshipManager.SetRelationship(RelationshipLevel.Friend, 0.8f, newChild, newChild.mother);
            WorldScripts.Instance.relationshipManager.SetRelationship(RelationshipLevel.Friend, 0.8f, newChild, newChild.father);

            String childName = bdPanel.transform.GetChild(0).Find( (newChild.gender == Gender.Female ? "GirlPanel" : "BoyPanel")).GetComponentInChildren<InputField>().text;
            newChild.SetName(childName);

            WorldScripts.Instance.cameraMovement.PanToPosition(newChild.GetPosition());

            PlannedParenthoodMod.InAddChild = false;
        }

        public static void RejectClicked(BirthdayChildPanel bdPanel)
        {
            bdPanel.gameObject.SetActive(false);
        }

        public static void InputChanged(BirthdayChildPanel bdPanel)
        {
            Transform panel = bdPanel.transform.GetChild(0);

            String boyName = panel.Find("BoyPanel").GetComponentInChildren<InputField>().text;
            String girlName = panel.Find("GirlPanel").GetComponentInChildren<InputField>().text;

            bdPanel.applyButton.interactable = !String.IsNullOrWhiteSpace(boyName) && !String.IsNullOrWhiteSpace(girlName);
        }
    }

    [HarmonyPatch(typeof(BirthdayChildPanel))]
    public static class BirthdayChildPanelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Show")]
        public static bool ShowPrefix(BirthdayChildPanel __instance, HumanAI child)
        {
            Transform panel = __instance.transform.GetChild(0);

            if ( panel.Find("ButtonContainer") == null)
            {
                PlannedParenthoodMod.SetupBirthdayPanel(__instance);
            }

            __instance.descriptionText.text = String.Format(Localization.GetText("witchy_PlannedParenthood_BirthdayPanelText"), child.father.GetFullName(), child.mother.GetFullName());

            String randomGirlName = HumanManager.GetRandomHumanNamePart(false, Gender.Female);
            String randomBoyName = HumanManager.GetRandomHumanNamePart(false, Gender.Male);
            String lastName = (UnityEngine.Random.Range(0f, 1f) < 0.5f) ? child.mother.lastName : child.father.lastName;

            panel.Find("GirlPanel").GetComponentInChildren<InputField>().text = $"{randomGirlName} {lastName}";
            panel.Find("BoyPanel").GetComponentInChildren<InputField>().text = $"{randomBoyName} {lastName}";

            PlannedParenthoodMod.InputChanged(__instance);

            AccessTools.Field(typeof(BirthdayChildPanel), "child").SetValue(__instance, child);

            __instance.gameObject.SetActive(true);
            UnityUtils.LogUnityObject(__instance.gameObject);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool StartPrefix()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Close")]
        public static bool ClosePrefix(BirthdayChildPanel __instance)
        {
            PlannedParenthoodMod.RejectClicked(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(Relationship), "HadChild")]
    public static class RelationshipPatch
    {
        public static bool Prefix()
        {
            return !PlannedParenthoodMod.InAddChild;
        }
    }

    [HarmonyPatch(typeof(ChildWish), "HadChild")]
    public static class ChildWishPatch
    {
        public static bool Prefix(HumanAI child)
        {
            return !PlannedParenthoodMod.InAddChild;
        }
    }

    [HarmonyPatch(typeof(HumanAI), "SetupAvatar")]
    public static class HumanAIPatch
    {
        public static bool Prefix(HumanAI __instance, GameObject avatarTemplate)
        {
            return !(PlannedParenthoodMod.InAddChild && avatarTemplate == null);
        }
    }

    [HarmonyPatch(typeof(HumanManager), "SpawnHuman")]
    [HarmonyPatch(new Type[] { typeof(HumanConfiguration), typeof(Vector3), typeof(Faction), typeof(bool), typeof(HumanAI.Serializable)})]
    public static class HumanManagerPatch
    {
        public static bool Prefix(HumanManager __instance,ref HumanAI __result, HumanConfiguration config, Vector3 position, Faction faction, bool alive = true, HumanAI.Serializable serial = null)
        {
            if (!PlannedParenthoodMod.InAddChild && config.humanType == HumanType.Colonist && config.ageLevel == AgeLevel.Child && serial == null)
            {
                PlannedParenthoodMod.InAddChild = true;

                GameObject fakeObj = new GameObject();
                fakeObj.SetActive(false);
                HumanMonoBehaviour hMono = fakeObj.AddComponent<HumanMonoBehaviour>();

                __result = new HumanAI(hMono, null, config, Int64.MaxValue);

                __result.mother.children.Remove(__result);
                __result.father.children.Remove(__result);
                return false;
            }

            return true;
        }
    }
}
