using HarmonyLib;
using Radon.Utils;
using System.Reflection;
using UniverseLib.Input;

namespace Radon.Modules
{
    internal class Miracle
    {
        static readonly MethodInfo ogmcu = AccessTools.Method(typeof(MechController), "Update");
        static readonly TogglePatch miraclePatch = TogglePatch.Postfix(ogmcu, typeof(Miracle), nameof(PostUpdate));
        static readonly MethodInfo ogpcdi = AccessTools.Method(typeof(PlayerCardDeck), "Initialize");
        static readonly TogglePatch startPatch = TogglePatch.Postfix(ogpcdi, typeof(Miracle), nameof(PostInitialize));
        static readonly MethodInfo oglrcum = AccessTools.Method(typeof(LevelRush), "CanUseMiracle");
        static readonly TogglePatch canUsePatch = TogglePatch.Prefix(oglrcum, typeof(Miracle), nameof(PreCanUseMiracle));

        public static void Patch(bool apply)
        {
            miraclePatch.Patch(apply);
        }
        public static void PatchStart(bool apply)
        {
            startPatch.Patch(apply);
            canUsePatch.Patch(apply);
        }
        public static void PostUpdate()
        {
            if (!InputManager.GetKeyDown(Radon.Settings.MiracleKey.Value))
                return;

            if (!Radon.IsPlaying())
                return;

            MainMenu.Instance().SetState(MainMenu.State.Miracle, true, true, false, false);
            AudioController.Play("MIRACLE_START");
        }
        public static void PostInitialize(PlayerCardDeck __instance)
        {
            LevelData level = Radon.Game.GetCurrentLevel();
            if (level == null || level.type == LevelData.LevelType.Hub)
                return;

            PlayerCard katana = __instance.GetCurrentHand()[0];
            katana.data = Radon.Game.GetGameData().GetCard("KATANA_MIRACLE");

            RM.ui.DeleteUICards();
            RM.ui.SetupUICards(__instance);
        }
        public static bool PreCanUseMiracle(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
