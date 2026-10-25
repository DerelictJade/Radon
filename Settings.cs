using MelonLoader;
using Radon.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radon
{
    public partial class Radon
    {
        public static class Settings
        {
            public static MelonPreferences_Category Category;
            public static MelonPreferences_Entry<bool> Enabled;
            public static MelonPreferences_Entry<bool> ResetTimer;
            public static MelonPreferences_Entry<bool> RestoreAmmo;
            public static MelonPreferences_Entry<bool> RestoreHealth;
            public static MelonPreferences_Entry<bool> RestoreEnemies;
            public static MelonPreferences_Entry<bool> RestorePickups;
            public static MelonPreferences_Entry<bool> RestoreBreakables;
            public static MelonPreferences_Entry<bool> ScaleBullets;
            public static MelonPreferences_Entry<float> BulletSize;
            public static MelonPreferences_Entry<bool> PhantomVisualizer;

            public static MelonPreferences_Category KeybindsCategory;
            public static MelonPreferences_Entry<KeyCode> NoclipKey;
            public static MelonPreferences_Entry<KeyCode> MiracleKey;
            public static MelonPreferences_Entry<KeyCode> SpeedUpKey;
            public static MelonPreferences_Entry<KeyCode> SpeedDownKey;
            public static MelonPreferences_Entry<KeyCode> SpeedResetKey;
            public static MelonPreferences_Entry<KeyCode> SaveModifier;
            public static MelonPreferences_Entry<KeyCode> LoadModifier;
            //WIP
            //public static MelonPreferences_Entry<KeyCode> EditorKey;
            public static List<MelonPreferences_Entry<KeyCode>> SavestateSlots = new List<MelonPreferences_Entry<KeyCode>>();
            public static void Register(Radon modInstance)
            {
                Category = MelonPreferences.CreateCategory("Radon");
                KeybindsCategory = MelonPreferences.CreateCategory("Radon/Keybinds");

                Enabled = CreateSettingEntry(modInstance.Toggle, "Enable Radon", false,
                    description: "Enables/Disables everything. Enabling triggers anticheat");

                // Savestate Related Settings
                ResetTimer = CreateSettingEntry(null, "Reset Timer on Load", true,
                    description: "Sets the timer to 0 when loading a savestate");

                RestoreAmmo = CreateSettingEntry(null, "Restore Ammo", true,
                    description: "Restore each card's ammo count when loading a savestate with cards");

                RestoreHealth = CreateSettingEntry(null, "Restore Health", true,
                    description: "Restore health when loading a savestate");

                RestoreEnemies = CreateSettingEntry(null, "Restore Enemies", true,
                    description: "Remove enemies that were dead when the savestate was made");

                RestorePickups = CreateSettingEntry(null, "Restore Pickups", true,
                    description: "Restore cards on the ground when loading a savestate");

                RestoreBreakables = CreateSettingEntry(null, "Restore Breakables", true,
                    description: "Restore chests and breakable walls when loading a savestate");

                // Bullet Related Settings
                ScaleBullets = CreateSettingEntry(enabled => Modules.ScaleBullets.Patch(enabled && Active), "Scale Bullets", false,
                    description: "Sets size of bullets, it's SBB time");

                BulletSize = CreateSettingEntry(null, "Bullet Size", 0.01f,
                    description: "Bullet hitbox radius when Scale Bullets is enabled. \nWARNING: Setting above 1000 may freeze your game");

                PhantomVisualizer = CreateSettingEntry(enabled => Modules.PhantomVisualizer.Patch(enabled && Active), "Show Bullet Phantoms", false,
                    description: "Keeps bullets visible after they hit something, for visualizing phantom/SBB timing");

                // Keybinds
                NoclipKey = CreateSettingEntry(null, "Noclip", KeyCode.N,
                    description: "Toggles noclip",
                    KeybindsCategory);

                MiracleKey = CreateSettingEntry(null, "Miracle Katana", KeyCode.K,
                    description: "Opens the miracle katana menu",
                    KeybindsCategory);

                SpeedUpKey = CreateSettingEntry(null, "Speed Up", KeyCode.UpArrow,
                    description: "Increases game speed by 5%",
                    KeybindsCategory);

                SpeedDownKey = CreateSettingEntry(null, "Speed Down", KeyCode.DownArrow,
                    description: "Decreases game speed by 5%",
                    KeybindsCategory);

                SpeedResetKey = CreateSettingEntry(null, "Speed Reset", KeyCode.LeftArrow,
                    description: "Resets game speed to 100%",
                    KeybindsCategory);

                SaveModifier = CreateSettingEntry(null, "Save Modifier", KeyCode.LeftControl,
                    description: "Hold while pressing a savestate slot key to save",
                    KeybindsCategory);

                LoadModifier = CreateSettingEntry(null, "Load Modifier", KeyCode.LeftAlt,
                    description: "Hold while pressing a savestate slot key to load position only, without restoring cards",
                    KeybindsCategory);
                // WIP
                //EditorKey = CreateSettingEntry(null, "Savestate Editor Key", KeyCode.P,
                //    description: "Opens/closes the savestate editor window.",
                //    KeybindsCategory);

                for (int i = 0; i < 10; i++)
                {
                    SavestateSlots.Add(CreateSettingEntry(null, $"Savestate Slot {i}", KeyCode.Alpha0 + i,
                        category: KeybindsCategory));
                }

                MelonPreferences_Entry<T> CreateSettingEntry<T>(Action<T> patchCallback, string title, T defaultValue, string description = "", MelonPreferences_Category category = null)
                {
                    if (category == null)
                    {
                        category = Settings.Category;
                    }

                    var entry = category.CreateEntry(title, defaultValue, description: description);

                    if (patchCallback != null)
                        entry.OnEntryValueChanged.Subscribe((before, after) => patchCallback.Invoke(after));

                    return entry;
                }
            }
        }
    }
}
