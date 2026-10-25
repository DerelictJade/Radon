using HarmonyLib;
using Radon.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniverseLib.Input;

namespace Radon.Modules
{
    internal class Savestates
    {
        static readonly MethodInfo ogmcu = AccessTools.Method(typeof(MechController), "Update");
        static readonly TogglePatch mcupostfix = TogglePatch.Postfix(ogmcu, typeof(Savestates), nameof(PostUpdate));
        private static readonly Dictionary<int, State> _states = new Dictionary<int, State>();
        private static GameObject _wallHolder;

        // Neon White swag section
        private static readonly FieldInfo rotationX = AccessTools.Field(typeof(MouseLook), "rotationX");
        private static readonly FieldInfo rotationY = AccessTools.Field(typeof(MouseLook), "rotationY");
        private static readonly FieldInfo isDead = AccessTools.Field(typeof(BaseDamageable), "_isDead");
        private static readonly FieldInfo pendingKills = AccessTools.Field(typeof(PlayerUI), "_kills");
        private static readonly FieldInfo onPickupCollected = AccessTools.Field(typeof(CardPickup), "_onPickupCollected");
        private static readonly FieldInfo cardHasBeenCollected = AccessTools.Field(typeof(CardPickupSpawner), "_cardHasBeenCollected");

        // NeonLite swag section
        private static readonly Type ensureTimer = AccessTools.TypeByName("NeonLite.Modules.Optimization.EnsureTimer");
        private static readonly MethodInfo resetTimer = ensureTimer == null ? null : AccessTools.Method(ensureTimer, "SetTrue");
        private static readonly Type superRestart = AccessTools.TypeByName("NeonLite.Modules.Optimization.SuperRestart");
        private static readonly FieldInfo superRestartDestroy = superRestart == null ? null : AccessTools.Field(superRestart, "destroy");

        public static void Patch(bool apply)
        {
            mcupostfix.Patch(apply);
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

                if (RM.drifter == null || !Radon.IsPlaying())
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
            // Look angle
            MouseLook[] looks = UnityEngine.Object.FindObjectsOfType<MouseLook>();
            if (looks.Length < 2)
                return;

            // Cards
            List<CardState> cards = new List<CardState>();
            foreach (PlayerCard card in RM.mechController.GetCurrentHand())
                cards.Add(new CardState(card.data.cardID, card.GetRank(), card.GetCurrentAmmo()));
            foreach (PlayerCard card in RM.mechController.GetCurrentDeck())
                cards.Add(new CardState(card.data.cardID, card.GetRank(), card.GetCurrentAmmo()));

            // Enemies
            HashSet<string> deadEnemies = new HashSet<string>();
            foreach (EnemySpawner spawner in UnityEngine.Object.FindObjectsOfType<EnemySpawner>())
            {
                if (spawner.HasSpawnedEnemy && !spawner.GetSpawnedEnemy().GetIsAlive())
                    deadEnemies.Add(GetSpawnerKey(spawner.transform));
            }

            // Ground pickups
            CardPickupSpawner[] spawners = UnityEngine.Object.FindObjectsOfType<CardPickupSpawner>();

            List<PickupState> pickups = new List<PickupState>();
            foreach (CardPickup pickup in UnityEngine.Object.FindObjectsOfType<CardPickup>())
            {
                if (IsCardPickup(pickup) && !IsSpawnerCard(pickup, spawners))
                    pickups.Add(new PickupState(pickup.GetPlayerCardData().cardID, pickup.transform.position, pickup.uiCard.GetCurrentPlayerCard().currentAmmo));
            }

            // Breakables
            // - Chests
            HashSet<string> spawnerCards = new HashSet<string>();
            HashSet<string> chests = new HashSet<string>();
            foreach (CardPickupSpawner spawner in spawners)
            {
                if (spawner.holderType == CardPickupSpawner.Type.Card && spawner.GetSpawnedGameObject() != null)
                    spawnerCards.Add(GetSpawnerKey(spawner.transform));
                else if (IsBrokenChest(spawner))
                    chests.Add(GetSpawnerKey(spawner.transform));
            }

            // - Walls and glass platforms
            if (_wallHolder == null)
            {
                _wallHolder = new GameObject("Radon Walls");
                _wallHolder.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(_wallHolder);
            }

            List<WallState> walls = new List<WallState>();
            foreach (Breakable breakable in UnityEngine.Object.FindObjectsOfType<Breakable>())
            {
                if (!IsWallOrPlatform(breakable) || !breakable.GetIsAlive())
                    continue;

                GameObject wall = UnityEngine.Object.Instantiate(breakable.gameObject, _wallHolder.transform, true);
                wall.name = breakable.name;
                walls.Add(new WallState(wall, breakable.gameObject.scene.name));
            }

            if (_states.TryGetValue(slot, out State oldState))
            {
                foreach (WallState entry in oldState.Walls)
                    UnityEngine.Object.Destroy(entry.Wall);
            }

            _states[slot] = new State
            {
                Level = Radon.Game.GetCurrentLevel(),
                Position = RM.drifter.Motor.TransientPosition,
                RotationX = looks[0].RotationX,
                RotationY = looks[1].RotationY,
                Cards = cards,
                Health = RM.mechController.currentHealth,
                DeadEnemies = deadEnemies,
                Pickups = pickups,
                SpawnerCards = spawnerCards,
                Chests = chests,
                Walls = walls
            };
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

            if (Radon.Settings.RestoreHealth.Value)
                RestoreHealth(state.Health);

            if (Radon.Settings.RestoreEnemies.Value)
                RestoreDeadEnemies(state.DeadEnemies);

            if (Radon.Settings.RestoreBreakables.Value)
                RestoreBreakables(state.Chests, state.Walls);

            if (Radon.Settings.RestorePickups.Value)
                RestorePickups(state.Pickups, state.SpawnerCards);

            if (!loadCards)
                return;

            PlayerCardDeck deck = RM.mechController.GetPlayerCardDeck();
            List<PlayerCard> hand = deck.GetCurrentHand();

            for (int i = hand.Count - 1; i >= 0; i--)
                deck.RemoveCard(hand[i], false);
            foreach (PlayerCard card in deck.GetCurrentDeck())
                deck.RemoveCard(card, false);
            deck.GetCurrentDeck().Clear();

            for (int i = state.Cards.Count - 1; i >= 0; i--)
                for (int j = 0; j <= state.Cards[i].Rank; j++)
                    GS.AddCard(state.Cards[i].Id);

            if (!Radon.Settings.RestoreAmmo.Value)
                return;

            RestoreAmmo(state.Cards);
            RefreshAmmoUI();
        }
        private static void ZeroTimer()
        {
            if (resetTimer == null)
                return;

            resetTimer.Invoke(null, null);
        }
        private static void RestoreHealth(int health)
        {
            MechController mech = RM.mechController;
            mech.currentHealth = health;
            RM.ui.SetNewHealth(mech.currentHealth, mech.maxHealth);
        }
        private static void RestoreDeadEnemies(HashSet<string> deadEnemies)
        {
            foreach (EnemySpawner spawner in UnityEngine.Object.FindObjectsOfType<EnemySpawner>())
            {
                if (!spawner.HasSpawnedEnemy || !deadEnemies.Contains(GetSpawnerKey(spawner.transform)))
                    continue;

                Enemy enemy = spawner.GetSpawnedEnemy();
                if (!enemy.GetIsAlive())
                    continue;

                enemy.ForceDie();
                isDead.SetValue(enemy, true);
            }

            // prevents kill sounds
            pendingKills.SetValue(RM.ui, 0);
        }
        private static void RestoreBreakables(HashSet<string> chests, List<WallState> walls)
        {
            foreach (CardPickupSpawner spawner in UnityEngine.Object.FindObjectsOfType<CardPickupSpawner>())
            {
                if (spawner.holderType != CardPickupSpawner.Type.Chest)
                    continue;

                UnityEngine.Object.Destroy(spawner.GetSpawnedGameObject());
                if (!chests.Contains(GetSpawnerKey(spawner.transform)))
                    spawner.SpawnCard();
            }

            foreach (Breakable breakable in UnityEngine.Object.FindObjectsOfType<Breakable>())
            {
                if (IsWallOrPlatform(breakable))
                    UnityEngine.Object.Destroy(breakable.gameObject);
            }

            HashSet<GameObject> restartDestroy = superRestartDestroy == null ? null : (HashSet<GameObject>)superRestartDestroy.GetValue(null);
            foreach (WallState entry in walls)
            {
                GameObject copy = UnityEngine.Object.Instantiate(entry.Wall, entry.Wall.transform.position, entry.Wall.transform.rotation);
                copy.name = entry.Wall.name;

                Scene scene = SceneManager.GetSceneByName(entry.Scene);
                if (scene.IsValid())
                    SceneManager.MoveGameObjectToScene(copy, scene);

                if (restartDestroy != null)
                    restartDestroy.Add(copy);
            }
        }
        private static bool IsBrokenChest(CardPickupSpawner spawner)
        {
            if (spawner.holderType != CardPickupSpawner.Type.Chest)
                return false;

            GameObject chest = spawner.GetSpawnedGameObject();
            return chest == null || !chest.GetComponent<BaseDamageable>().GetIsAlive();
        }
        private static bool IsWallOrPlatform(Breakable breakable)
        {
            if (breakable.name.Contains("(Clone)")) // NeonLite compatibility frfr
                return false;

            BaseDamageable.DamageableType type = breakable.GetDamageableType();
            return type == BaseDamageable.DamageableType.Wall || type == BaseDamageable.DamageableType.Platform;
        }
        private static void RestorePickups(List<PickupState> pickups, HashSet<string> spawnerCards)
        {
            CardPickupSpawner[] spawners = UnityEngine.Object.FindObjectsOfType<CardPickupSpawner>();
            foreach (CardPickupSpawner spawner in spawners)
            {
                if (spawner.holderType != CardPickupSpawner.Type.Card || !spawner.HasSpawnedCard)
                    continue;

                GameObject card = spawner.GetSpawnedGameObject();
                bool onGround = spawnerCards.Contains(GetSpawnerKey(spawner.transform));
                if (onGround && card == null)
                {
                    cardHasBeenCollected.SetValue(spawner, false);
                    spawner.SpawnCard();
                }
                else if (!onGround && card != null)
                {
                    Action collect = (Action)onPickupCollected.GetValue(card.GetComponent<CardPickup>());
                    if (collect != null)
                        collect.Invoke();

                    UnityEngine.Object.Destroy(card);
                }
            }

            foreach (CardPickup pickup in UnityEngine.Object.FindObjectsOfType<CardPickup>())
            {
                if (IsCardPickup(pickup) && !IsSpawnerCard(pickup, spawners))
                    UnityEngine.Object.Destroy(pickup.gameObject);
            }

            foreach (PickupState entry in pickups)
            {
                PlayerCardData data = Radon.Game.GetGameData().GetCard(entry.Id);
                if (data != null)
                    CardPickup.Spawn(data, entry.Position, out CardPickup spawned, null, false, entry.Ammo);
            }
        }
        private static bool IsSpawnerCard(CardPickup pickup, CardPickupSpawner[] spawners)
        {
            foreach (CardPickupSpawner spawner in spawners)
            {
                if (spawner.GetSpawnedGameObject() == pickup.gameObject)
                    return true;
            }

            return false;
        }
        private static bool IsCardPickup(CardPickup pickup)
        {
            PlayerCardData data = pickup.GetPlayerCardData();
            if (data == null)
                return false;

            switch (data.consumableType)
            {
                case PlayerCardData.ConsumableType.GiftCollectible:
                case PlayerCardData.ConsumableType.LoreCollectible:
                case PlayerCardData.ConsumableType.Memory:
                case PlayerCardData.ConsumableType.GreenMemoryItem:
                    return false;
                default:
                    return true;
            }
        }
        private static string GetSpawnerKey(Transform spawner)
        {
            string key = spawner.GetSiblingIndex().ToString();
            for (Transform parent = spawner.parent; parent != null; parent = parent.parent)
                key = $"{parent.name}/{key}";

            return key;
        }
        private static void RestoreAmmo(List<CardState> saved)
        {
            foreach (PlayerCard card in RM.mechController.GetCurrentHand())
                SetAmmo(card, saved);
            foreach (PlayerCard card in RM.mechController.GetCurrentDeck())
                SetAmmo(card, saved);
        }
        private static void SetAmmo(PlayerCard card, List<CardState> saved)
        {
            foreach (CardState entry in saved)
            {
                if (entry.Id != card.data.cardID)
                    continue;

                card.currentAmmo = entry.Ammo;
                return;
            }
        }
        private static void RefreshAmmoUI()
        {
            List<PlayerCard> hand = RM.mechController.GetCurrentHand();

            for (int i = 0; i < hand.Count; i++)
                RM.ui.UpdateCardAmmo(hand[i], i == 0);
            foreach (PlayerCard card in RM.mechController.GetCurrentDeck())
                RM.ui.UpdateCardAmmo(card, false);
        }
    }
}