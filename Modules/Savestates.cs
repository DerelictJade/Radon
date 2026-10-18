using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using UniverseLib.Input;

namespace Radon.Modules
{
    internal class Savestates
    {
        private static bool _patched = false;
        private const int HeavenEnvironmentBuildIndex = 6;

        private static readonly Dictionary<int, State> _states = new Dictionary<int, State>();

        static readonly MethodInfo ogmcu = AccessTools.Method(typeof(MechController), "Update");
        static readonly MethodInfo mcupostfixmi = typeof(Savestates).GetMethod(nameof(PostUpdate));
        static readonly HarmonyMethod mcupostfix = new HarmonyMethod(mcupostfixmi);

        static readonly FieldInfo rotationX = AccessTools.Field(typeof(MouseLook), "rotationX");
        static readonly FieldInfo rotationY = AccessTools.Field(typeof(MouseLook), "rotationY");
        static readonly Type ensureTimer = AccessTools.TypeByName("NeonLite.Modules.Optimization.EnsureTimer");
        static readonly MethodInfo resetTimer = ensureTimer == null ? null : AccessTools.Method(ensureTimer, "SetTrue");

        public static void Patch(bool apply)
        {
            if (_patched == apply)
                return;

            if (apply)
                DoPatch();
            else
                UndoPatch();

            _patched = apply;
        }

        private static void DoPatch()
        {
            Radon.Harmony.Patch(ogmcu, postfix: mcupostfix);
        }

        private static void UndoPatch()
        {
            Radon.Harmony.Unpatch(ogmcu, mcupostfixmi);
        }

        public static void PostUpdate()
        {
            // WIP
            //if (SavestateEditor.IsOpen)
            //    return;

            for (int i = 0; i < Radon.Settings.SavestateSlots.Count; i++)
            {
                if (!InputManager.GetKeyDown(Radon.Settings.SavestateSlots[i].Value))
                    continue;

                if (RM.drifter == null || SceneManager.GetActiveScene().buildIndex == HeavenEnvironmentBuildIndex)
                    return;

                if (InputManager.GetKey(Radon.Settings.SaveModifier.Value))
                    Save(i);
                else
                    Load(i, !InputManager.GetKey(Radon.Settings.LoadModifier.Value));

                return;
            }
        }

        private static void Save(int slot)
        {
            MouseLook[] looks = UnityEngine.Object.FindObjectsOfType<MouseLook>();
            if (looks.Length < 2)
                return;

            List<(string id, int rank, int ammo)> cards = new List<(string id, int rank, int ammo)>();
            foreach (PlayerCard card in RM.mechController.GetCurrentHand())
                cards.Add((card.data.cardID, card.GetRank(), card.GetCurrentAmmo()));
            foreach (PlayerCard card in RM.mechController.GetCurrentDeck())
                cards.Add((card.data.cardID, card.GetRank(), card.GetCurrentAmmo()));

            _states[slot] = new State(Radon.Game.GetCurrentLevel(), RM.drifter.Motor.TransientPosition, looks[0].RotationX, looks[1].RotationY, cards);
        }

        private static void Load(int slot, bool loadCards)
        {
            if (!_states.TryGetValue(slot, out State state))
                return;

            if (state.Level != Radon.Game.GetCurrentLevel())
                return;

            MouseLook[] looks = UnityEngine.Object.FindObjectsOfType<MouseLook>();
            if (looks.Length < 2)
                return;

            rotationX.SetValue(looks[0], state.RotationX);
            rotationY.SetValue(looks[1], state.RotationY);
            RM.drifter.Motor.SetTransientPosition(state.Position);

            if (Radon.Settings.ResetTimer.Value)
                ZeroTimer();

            if (!loadCards)
                return;

            RM.mechController.GetPlayerCardDeck().DiscardAllCards(true);
            for (int i = state.Cards.Count - 1; i >= 0; i--)
                for (int j = 0; j <= state.Cards[i].rank; j++)
                    GS.AddCard(state.Cards[i].id);

            if (!Radon.Settings.RestoreAmmo.Value)
                return;

            ApplyAmmo(state.Cards);
            RefreshAmmoUI();
        }

        private static void ZeroTimer()
        {
            if (resetTimer == null)
                return;

            resetTimer.Invoke(null, null);
        }

        private static void ApplyAmmo(List<(string id, int rank, int ammo)> saved)
        {
            foreach (PlayerCard card in RM.mechController.GetCurrentHand())
                SetAmmo(card, saved);
            foreach (PlayerCard card in RM.mechController.GetCurrentDeck())
                SetAmmo(card, saved);
        }

        private static void SetAmmo(PlayerCard card, List<(string id, int rank, int ammo)> saved)
        {
            foreach ((string id, int rank, int ammo) entry in saved)
            {
                if (entry.id != card.data.cardID)
                    continue;

                card.currentAmmo = entry.ammo;
                return;
            }
        }

        private static void RefreshAmmoUI()
        {
            UICard[] uiCards = UnityEngine.Object.FindObjectsOfType<UICard>();

            foreach (PlayerCard card in RM.mechController.GetCurrentHand())
            {
                foreach (UICard uiCard in uiCards)
                {
                    if (uiCard.GetCurrentPlayerCard() == card)
                        uiCard.SetCardAmmo(card);
                }
            }
        }
    }
}