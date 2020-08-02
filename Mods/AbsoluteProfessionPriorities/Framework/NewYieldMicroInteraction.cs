//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using UnityEngine;
//using UnityEngine.UI;

//namespace WitchyMods.AbsoluteProfessionPriorities.Framework {

//    public partial class NewYieldMicroInteraction : YieldMicroInteraction {
//        public struct FightYieldResult {
//            public YieldResult yieldResult;
//            public bool didAttack;
//            public FightYieldResult(YieldResult yieldResult, bool didAttack) { this.yieldResult = yieldResult; this.didAttack = didAttack; }
//        }

//        #region Fields And Properties
//        public HumanAI human {
//            get { var value = AccessTools.Field(this.GetType(), "human").GetValue(this); if (value == null) return default(HumanAI); else return (HumanAI)value; }
//            set { AccessTools.Field(this.GetType(), "human").SetValue(this, value); }
//        }

//        public IEnumerator<YieldResult> enumerator {
//            get { var value = AccessTools.Field(this.GetType(), "enumerator").GetValue(this); if (value == null) return default(IEnumerator<YieldResult>); else return (IEnumerator<YieldResult>)value; }
//            set { AccessTools.Field(this.GetType(), "enumerator").SetValue(this, value); }
//        }

//        public bool continueOnFail {
//            get { var value = AccessTools.Field(this.GetType(), "continueOnFail").GetValue(this); if (value == null) return default(bool); else return (bool)value; }
//            set { AccessTools.Field(this.GetType(), "continueOnFail").SetValue(this, value); }
//        }

//        public bool failed {
//            get { var value = AccessTools.Field(this.GetType(), "failed").GetValue(this); if (value == null) return default(bool); else return (bool)value; }
//            set { AccessTools.Field(this.GetType(), "failed").SetValue(this, value); }
//        }

//        public TransformStruct tf {
//            get { var value = AccessTools.Field(this.GetType(), "tf").GetValue(this); if (value == null) return default(TransformStruct); else return (TransformStruct)value; }
//            set { AccessTools.Field(this.GetType(), "tf").SetValue(this, value); }
//        }

//        public Interactable InteractionTarget {
//            get { var value = AccessTools.Field(this.GetType(), "InteractionTarget").GetValue(this); if (value == null) return default(Interactable); else return (Interactable)value; }
//            set { AccessTools.Field(this.GetType(), "InteractionTarget").SetValue(this, value); }
//        }

//        #endregion

//        #region Constructors
//        public NewYieldMicroInteraction(InteractionInfo InteractionInfo, HumanAI human) : base(InteractionInfo, human) {
//        }
//        #endregion

//        #region New Public Methods
//        public new YieldMicroInteraction Handle() {
//            return base.Handle();
//        }

//        public new YieldMicroInteraction Handle(out YieldResult yieldResult) {
//            return base.Handle(out yieldResult);
//        }

//        public new void Stop() {
//            base.Stop();
//        }

//        public new void Fail() {
//            base.Fail();
//        }
//        #endregion

//        #region Private Methods Wrappers
//        public IEnumerable<YieldResult> GetInteractionEnumerable(Interaction interaction) {
//            var args = new object[] { interaction };
//            var argTypes = new Type[] { typeof(Interaction) };

//            var result = AccessTools.Method(base.GetType(), "GetInteractionEnumerable", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> MeetForInteraction(Interaction interaction, HumanAI partner, bool force) {
//            var args = new object[] { interaction, partner, force };
//            var argTypes = new Type[] { typeof(Interaction), typeof(HumanAI), typeof(bool) };

//            var result = AccessTools.Method(base.GetType(), "MeetForInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> HandleTalkingAnimations(float talkTime, HumanAI partner, Interaction partnerRequiredInteraction) {
//            var args = new object[] { talkTime, partner, partnerRequiredInteraction };
//            var argTypes = new Type[] { typeof(float), typeof(HumanAI), typeof(Interaction) };

//            var result = AccessTools.Method(base.GetType(), "HandleTalkingAnimations", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Talk(float talkTime) {
//            var args = new object[] { talkTime };
//            var argTypes = new Type[] { typeof(float) };

//            var result = AccessTools.Method(base.GetType(), "Talk", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> TalkAboutFood() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "TalkAboutFood", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Tame() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Tame", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Shear() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Shear", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Milk() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Milk", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Butcher() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Butcher", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public void CheckDiplomatIncreasingRelations(float intensity, HumanAI target) {
//            var args = new object[] { intensity, target };
//            var argTypes = new Type[] { typeof(float), typeof(HumanAI) };

//            AccessTools.Method(base.GetType(), "CheckDiplomatIncreasingRelations", argTypes).Invoke(this, args);
//        }

//        public void UpdateTalkingAnimations() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            AccessTools.Method(base.GetType(), "UpdateTalkingAnimations", argTypes).Invoke(this, args);
//        }

//        public bool IsPartnerDoingPartnerInteraction(HumanAI partner, Interaction interaction) {
//            var args = new object[] { partner, interaction };
//            var argTypes = new Type[] { typeof(HumanAI), typeof(Interaction) };

//            var result = AccessTools.Method(base.GetType(), "IsPartnerDoingPartnerInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public bool FigureOutWhoLeadsInteraction(HumanAI partner) {
//            var args = new object[] { partner };
//            var argTypes = new Type[] { typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "FigureOutWhoLeadsInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public bool SwitchInteractionLead(HumanAI partner) {
//            var args = new object[] { partner };
//            var argTypes = new Type[] { typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "SwitchInteractionLead", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public YieldResult Interact() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Interact", argTypes).Invoke(this, args);
//            return result == null ? default(YieldResult) : (YieldResult)result;
//        }

//        public YieldResult Interact(HumanAI targetHuman) {
//            var args = new object[] { targetHuman };
//            var argTypes = new Type[] { typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "Interact", argTypes).Invoke(this, args);
//            return result == null ? default(YieldResult) : (YieldResult)result;
//        }

//        public YieldResult ObjectInteract(Interactable objectToInteractWith) {
//            var args = new object[] { objectToInteractWith };
//            var argTypes = new Type[] { typeof(Interactable) };

//            var result = AccessTools.Method(base.GetType(), "ObjectInteract", argTypes).Invoke(this, args);
//            return result == null ? default(YieldResult) : (YieldResult)result;
//        }

//        public YieldResult LockInteraction() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "LockInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(YieldResult) : (YieldResult)result;
//        }

//        public IEnumerable<YieldResult> Wait(float seconds, Ref<float> progress = null) {
//            var args = new object[] { seconds, progress };
//            var argTypes = new Type[] { typeof(float), typeof(Ref<float>) };

//            var result = AccessTools.Method(base.GetType(), "Wait", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> EnableNavMeshAgent() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "EnableNavMeshAgent", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> GoHere() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "GoHere", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Walk(Interactable target, float distance, bool refuseOnFail = true, Vector3 offset = default(Vector3)) {
//            var args = new object[] { target, distance, refuseOnFail, offset };
//            var argTypes = new Type[] { typeof(Interactable), typeof(float), typeof(bool), typeof(Vector3) };

//            var result = AccessTools.Method(base.GetType(), "Walk", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WalkImpl(Interactable target, float distance, Vector3 offset = default(Vector3)) {
//            var args = new object[] { target, distance, offset };
//            var argTypes = new Type[] { typeof(Interactable), typeof(float), typeof(Vector3) };

//            var result = AccessTools.Method(base.GetType(), "WalkImpl", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public bool GetReachablePosition(IEnumerable<TransformStruct> sortedInteractionTransforms, Vector3 offset, out Vector3 finalPosition) {
//            finalPosition = default(Vector3);
//            var args = new object[] { sortedInteractionTransforms, offset, finalPosition };
//            var argTypes = new Type[] { typeof(IEnumerable<TransformStruct>), typeof(Vector3), typeof(Vector3) };

//            var result = AccessTools.Method(base.GetType(), "GetReachablePosition", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public void Continue(float distance) {
//            var args = new object[] { distance };
//            var argTypes = new Type[] { typeof(float) };

//            AccessTools.Method(base.GetType(), "Continue", argTypes).Invoke(this, args);
//        }

//        public void TurnToTarget() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            AccessTools.Method(base.GetType(), "TurnToTarget", argTypes).Invoke(this, args);
//        }

//        public void SlowTurnTo(Vector3 target) {
//            var args = new object[] { target };
//            var argTypes = new Type[] { typeof(Vector3) };

//            AccessTools.Method(base.GetType(), "SlowTurnTo", argTypes).Invoke(this, args);
//        }

//        public void TurnTo(Vector3 target) {
//            var args = new object[] { target };
//            var argTypes = new Type[] { typeof(Vector3) };

//            AccessTools.Method(base.GetType(), "TurnTo", argTypes).Invoke(this, args);
//        }

//        public void TurnToTransform() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            AccessTools.Method(base.GetType(), "TurnToTransform", argTypes).Invoke(this, args);
//        }

//        public bool IsConscious(Interactable interactable) {
//            var args = new object[] { interactable };
//            var argTypes = new Type[] { typeof(Interactable) };

//            var result = AccessTools.Method(base.GetType(), "IsConscious", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public bool IsAlive(Interactable interactable) {
//            var args = new object[] { interactable };
//            var argTypes = new Type[] { typeof(Interactable) };

//            var result = AccessTools.Method(base.GetType(), "IsAlive", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public void StopCurrentSubtask() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            AccessTools.Method(base.GetType(), "StopCurrentSubtask", argTypes).Invoke(this, args);
//        }

//        public IEnumerable<YieldResult> WaitHere() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "WaitHere", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Eat() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Eat", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> DrinkBeer() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "DrinkBeer", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> EatAtChair() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "EatAtChair", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> DrinkBeerAtChair() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "DrinkBeerAtChair", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> GiveFood() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "GiveFood", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public bool TryTakeFood(int amountToTake, out int amountTaken, out Resource eatenFood, HumanAI human) {
//            amountTaken = default(int);
//            eatenFood = default(Resource);
//            var args = new object[] { amountToTake, amountTaken, eatenFood, human };
//            var argTypes = new Type[] { typeof(int), typeof(int), typeof(Resource), typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "TryTakeFood", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public bool TryEatFoodInInventory(HumanAI target) {
//            var args = new object[] { target };
//            var argTypes = new Type[] { typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "TryEatFoodInInventory", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public void TryEatFood(HumanAI target, Resource food) {
//            var args = new object[] { target, food };
//            var argTypes = new Type[] { typeof(HumanAI), typeof(Resource) };

//            AccessTools.Method(base.GetType(), "TryEatFood", argTypes).Invoke(this, args);
//        }

//        public IEnumerable<YieldResult> TakeHealingPotion() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "TakeHealingPotion", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> DrinkElixirOfYouth() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "DrinkElixirOfYouth", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> GiveHealingPotion() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "GiveHealingPotion", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> TryFluTreatment() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "TryFluTreatment", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> HealInteraction() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "HealInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> BandageWounds() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "BandageWounds", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Sow() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Sow", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Equip() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Equip", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> UnequipBoth() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "UnequipBoth", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Unequip(bool weapon) {
//            var args = new object[] { weapon };
//            var argTypes = new Type[] { typeof(bool) };

//            var result = AccessTools.Method(base.GetType(), "Unequip", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public HumanAI GetTargetHumanFromFurniture(Interactable InteractionTarget) {
//            var args = new object[] { InteractionTarget };
//            var argTypes = new Type[] { typeof(Interactable) };

//            var result = AccessTools.Method(base.GetType(), "GetTargetHumanFromFurniture", argTypes).Invoke(this, args);
//            return result == null ? default(HumanAI) : (HumanAI)result;
//        }

//        public IEnumerable<YieldResult> ChangeProfession() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "ChangeProfession", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> GiveProductionOrders() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "GiveProductionOrders", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> ChangeOwnership() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "ChangeOwnership", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> ContinueInteraction(IEnumerable<YieldResult> interactionEnumberable, float distance, Func<bool> continueCondition = null, bool continueOnFail = true) {
//            var args = new object[] { interactionEnumberable, distance, continueCondition, continueOnFail };
//            var argTypes = new Type[] { typeof(IEnumerable<YieldResult>), typeof(float), typeof(Func<bool>), typeof(bool) };

//            var result = AccessTools.Method(base.GetType(), "ContinueInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Construct() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Construct", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> BuildSnowman() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "BuildSnowman", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WorkOnFurniture() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "WorkOnFurniture", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> StartWorkOnFurniture() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "StartWorkOnFurniture", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WorkOnFurnitureStep(float duration) {
//            var args = new object[] { duration };
//            var argTypes = new Type[] { typeof(float) };

//            var result = AccessTools.Method(base.GetType(), "WorkOnFurnitureStep", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Learn() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Learn", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> LearnLifeSkills() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "LearnLifeSkills", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> TeachLifeSkills() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "TeachLifeSkills", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WaterPlant() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "WaterPlant", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WalkAndInteract(float walkFinishedDistance) {
//            var args = new object[] { walkFinishedDistance };
//            var argTypes = new Type[] { typeof(float) };

//            var result = AccessTools.Method(base.GetType(), "WalkAndInteract", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> WalkWaitInteract(float walkFinishedDistance, float waitTime, bool displayProgressbar = false, String animationParameter = null) {
//            var args = new object[] { walkFinishedDistance, waitTime, displayProgressbar, animationParameter };
//            var argTypes = new Type[] { typeof(float), typeof(float), typeof(bool), typeof(String) };

//            var result = AccessTools.Method(base.GetType(), "WalkWaitInteract", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> CarryToBed() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "CarryToBed", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> LockUp() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "LockUp", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Bury() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Bury", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> BringToGrave(bool lastTry) {
//            var args = new object[] { lastTry };
//            var argTypes = new Type[] { typeof(bool) };

//            var result = AccessTools.Method(base.GetType(), "BringToGrave", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Retry(Func<bool, IEnumerable<YieldResult>> actionToRetry, int attempts) {
//            var args = new object[] { actionToRetry, attempts };
//            var argTypes = new Type[] { typeof(Func<bool, IEnumerable<YieldResult>>), typeof(int) };

//            var result = AccessTools.Method(base.GetType(), "Retry", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public void Carry(HumanAI target, Ragdoll ragdoll) {
//            var args = new object[] { target, ragdoll };
//            var argTypes = new Type[] { typeof(HumanAI), typeof(Ragdoll) };

//            AccessTools.Method(base.GetType(), "Carry", argTypes).Invoke(this, args);
//        }

//        public IEnumerable<YieldResult> Fight() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Fight", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> LearnFighting() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "LearnFighting", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public float GetFightDistanceOffset(HumanType humanType) {
//            var args = new object[] { humanType };
//            var argTypes = new Type[] { typeof(HumanType) };

//            var result = AccessTools.Method(base.GetType(), "GetFightDistanceOffset", argTypes).Invoke(this, args);
//            return result == null ? default(float) : (float)result;
//        }

//        public IEnumerable<FightYieldResult> FightHumanMelee() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "FightHumanMelee", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<FightYieldResult>) : (IEnumerable<FightYieldResult>)result;
//        }

//        public void SetIsAttackingAnimationParameters(bool isAttacking) {
//            var args = new object[] { isAttacking };
//            var argTypes = new Type[] { typeof(bool) };

//            AccessTools.Method(base.GetType(), "SetIsAttackingAnimationParameters", argTypes).Invoke(this, args);
//        }

//        public IEnumerable<FightYieldResult> FightTowerMelee() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "FightTowerMelee", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<FightYieldResult>) : (IEnumerable<FightYieldResult>)result;
//        }

//        public IEnumerable<FightYieldResult> FightRanged() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "FightRanged", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<FightYieldResult>) : (IEnumerable<FightYieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Destroy() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Destroy", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<Pair<YieldResult, Vector3>> FindLineOfSightPosition(Vector3 targetPos, HumanAI shooter) {
//            var args = new object[] { targetPos, shooter };
//            var argTypes = new Type[] { typeof(Vector3), typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "FindLineOfSightPosition", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<Pair<YieldResult, Vector3>>) : (IEnumerable<Pair<YieldResult, Vector3>>)result;
//        }

//        public bool HasLineOfSight(Vector3 start, Vector3 end, HumanAI shooter) {
//            var args = new object[] { start, end, shooter };
//            var argTypes = new Type[] { typeof(Vector3), typeof(Vector3), typeof(HumanAI) };

//            var result = AccessTools.Method(base.GetType(), "HasLineOfSight", argTypes).Invoke(this, args);
//            return result == null ? default(bool) : (bool)result;
//        }

//        public IEnumerable<YieldResult> Sit() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Sit", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Sleep() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Sleep", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Relax() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Relax", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Enjoy() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Enjoy", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Drum() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Drum", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> RefuseInteraction(Sprite sprite) {
//            var args = new object[] { sprite };
//            var argTypes = new Type[] { typeof(Sprite) };

//            var result = AccessTools.Method(base.GetType(), "RefuseInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Hug() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Hug", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Kiss() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Kiss", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> CoordinatedPartnerInteraction(String animationName, float duration, float distance) {
//            var args = new object[] { animationName, duration, distance };
//            var argTypes = new Type[] { typeof(String), typeof(float), typeof(float) };

//            var result = AccessTools.Method(base.GetType(), "CoordinatedPartnerInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Insult() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Insult", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Apologize() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Apologize", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> MagicSmile() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "MagicSmile", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> BreakUp() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "BreakUp", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> RiskyJoke() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "RiskyJoke", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Impress() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Impress", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> ReceiveInteraction() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "ReceiveInteraction", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Dance() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Dance", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Cheer() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Cheer", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> GuardTower() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "GuardTower", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> Remove() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "Remove", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        public IEnumerable<YieldResult> AskAboutTraits() {
//            var args = new object[] { };
//            var argTypes = new Type[] { };

//            var result = AccessTools.Method(base.GetType(), "AskAboutTraits", argTypes).Invoke(this, args);
//            return result == null ? default(IEnumerable<YieldResult>) : (IEnumerable<YieldResult>)result;
//        }

//        #endregion
//    }
//}
