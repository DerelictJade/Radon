using MelonLoader;
using Radon.Modules;
//using Radon.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radon
{
    public class Radon : MelonMod
    {
        internal static Game Game { get; private set; }
        internal static new HarmonyLib.Harmony Harmony { get; private set; }
        internal static bool Active { get; private set; }

        public override void OnLateInitializeMelon()
        {
            Game = Singleton<Game>.Instance;
            Harmony = new HarmonyLib.Harmony("nz.DerelictJade.Radon");

            Settings.Register(this);

            Game.OnInitializationComplete += () =>
            {
                if (Settings.Enabled.Value)
                    Toggle(true);
            };
        }

        internal void Toggle(bool enabled)
        {
            if (Active == enabled)
                return;

            if (enabled)
            {
                NeonLite.Modules.Anticheat.Register(MelonAssembly);
                Savestates.Patch(true);
                Bullets.Patch(true);
            }
            else
            {
                Bullets.Patch(false);
                Savestates.Patch(false);
                NeonLite.Modules.Anticheat.Unregister(MelonAssembly);
            }

            Active = enabled;
        }

        public static class Settings
        {
            public static MelonPreferences_Category Category;
            public static MelonPreferences_Entry<bool> Enabled;
            public static MelonPreferences_Entry<bool> RestoreAmmo;
            public static MelonPreferences_Entry<bool> ResetTimer;
            public static MelonPreferences_Entry<bool> ScaleBullets;
            public static MelonPreferences_Entry<float> BulletSize;
            public static MelonPreferences_Entry<bool> PhantomVisualizer;

            public static MelonPreferences_Category KeybindsCategory;
            public static MelonPreferences_Entry<KeyCode> SaveModifier;
            public static MelonPreferences_Entry<KeyCode> LoadModifier;
            //public static MelonPreferences_Entry<KeyCode> EditorKey;
            public static List<MelonPreferences_Entry<KeyCode>> SavestateSlots = new List<MelonPreferences_Entry<KeyCode>>();

            public static void Register(Radon modInstance)
            {
                Category = MelonPreferences.CreateCategory("Radon");
                KeybindsCategory = MelonPreferences.CreateCategory("Radon/Keybinds");

                Enabled = CreateSettingEntry(modInstance.Toggle, "Enable Radon", false,
                    description: "Enables/Disables everything. Enabling triggers anticheat",
                    category: Category);

                RestoreAmmo = CreateSettingEntry<bool>(null, "Restore Ammo", true,
                    description: "Restore each card's ammo count when loading a savestate with cards",
                    category: Category);

                ResetTimer = CreateSettingEntry<bool>(null, "Reset Timer on Load", true,
                    description: "Sets the timer to 0 when loading a savestate",
                    category: Category);

                ScaleBullets = CreateSettingEntry<bool>(enabled => Bullets.PatchScale(enabled && Active), "Scale Bullets", false,
                    description: "Overrides the hitbox and model size of bullets",
                    category: Category);

                BulletSize = CreateSettingEntry<float>(null, "Bullet Size", 0.01f,
                    description: "Bullet hitbox radius when Scale Bullets is enabled. \nWARNING: Setting above 1000 may freeze your game",
                    category: Category);

                PhantomVisualizer = CreateSettingEntry<bool>(enabled => Bullets.PatchPhantom(enabled && Active), "Show Bullet Phantoms", false,
                    description: "Keeps bullets visible after they hit something, for visualizing phantom/SBB timing",
                    category: Category);

                SaveModifier = CreateSettingEntry<KeyCode>(null, "Save Modifier", KeyCode.LeftControl,
                    description: "Hold while pressing a savestate slot key to save.",
                    category: KeybindsCategory);

                LoadModifier = CreateSettingEntry<KeyCode>(null, "Load Modifier", KeyCode.LeftAlt,
                    description: "Hold while pressing a savestate slot key to load position only, without restoring cards.",
                    category: KeybindsCategory);
                // WIP
                //EditorKey = CreateSettingEntry<KeyCode>(null, "Savestate Editor Key", KeyCode.P,
                //    description: "Opens/closes the savestate editor window.",
                //    category: KeybindsCategory);

                for (int i = 0; i < 10; i++)
                {
                    SavestateSlots.Add(CreateSettingEntry<KeyCode>(null, $"Savestate Slot {i}", KeyCode.Alpha0 + i,
                        category: KeybindsCategory));
                }

                MelonPreferences_Entry<T> CreateSettingEntry<T>(Action<T> patchCallback, string title, T defaultValue, string description = "", MelonPreferences_Category category = null)
                {
                    var entry = category.CreateEntry(title, defaultValue, description: description);

                    if (patchCallback != null)
                        entry.OnEntryValueChanged.Subscribe((before, after) => patchCallback.Invoke(after));

                    return entry;
                }
            }
        }
    }
}