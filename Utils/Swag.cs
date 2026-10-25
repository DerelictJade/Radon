using HarmonyLib;
using MelonLoader;
using MelonPrefManager.UI;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;

namespace Radon.Utils
{
    // the most important class in the mod
    internal static class Swag
    {
        static readonly Color Inactive = new Color(0.40f, 0.28f, 0.04f);
        static readonly Color Active = new Color(0.82f, 0.56f, 0f);
        static readonly Color Highlighted = new Color(0.83f, 0.69f, 0.22f);
        static readonly Color Pressed = new Color(0.50f, 0.36f, 0.02f);
        static readonly Color Label = new Color(0.95f, 0.77f, 0.25f);

        static FieldInfo categoryListContent;

        public static void Apply()
        {
            if (MelonBase.RegisteredMelons.Any(m => m.Info.Name == "MelonPreferencesManager"))
                PatchManager();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PatchManager()
        {
            categoryListContent = AccessTools.Field(typeof(UIManager), "CategoryListContent");
            Radon.Harmony.Patch(AccessTools.Method(typeof(CachedConfigEntry), "ConstructUI"), postfix: new HarmonyMethod(typeof(Swag), nameof(PostConstructUI)));
            Radon.Harmony.Patch(AccessTools.Method(typeof(UIManager), "SetActiveCategory"), postfix: new HarmonyMethod(typeof(Swag), nameof(PostSetActiveCategory)));
            Radon.Harmony.Patch(AccessTools.Method(typeof(UIManager), "UnsetActiveCategory"), postfix: new HarmonyMethod(typeof(Swag), nameof(PostUnsetActiveCategory)));
        }

        public static void PostConstructUI(object __instance)
        {
            var entry = (CachedConfigEntry)__instance;
            if (!IsRadon(entry.RefConfig.Category.Identifier))
                return;

            entry.mainLabel.color = Label;
            PaintButtons(null);
        }
        public static void PostSetActiveCategory(string categoryIdentifier) => PaintButtons(categoryIdentifier);
        public static void PostUnsetActiveCategory() => PaintButtons(null);

        static void PaintButtons(string activeIdentifier)
        {
            var content = (GameObject)categoryListContent.GetValue(null);
            foreach (Transform child in content.transform)
            {
                if (!child.name.StartsWith("BUTTON_"))
                    continue;

                string identifier = child.name.Substring("BUTTON_".Length);
                if (IsRadon(identifier))
                    RuntimeHelper.SetColorBlock(child.GetComponent<Selectable>(), identifier == activeIdentifier ? Active : Inactive, Highlighted, Pressed);
            }
        }
        static bool IsRadon(string identifier) => identifier == "Radon" || identifier.StartsWith("Radon/");
    }
}
