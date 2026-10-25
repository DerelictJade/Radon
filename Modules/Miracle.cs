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

        public static void Patch(bool apply)
        {
            miraclePatch.Patch(apply);
        }
        public static void PostUpdate()
        {
            if (!InputManager.GetKeyDown(Radon.Settings.MiracleKey.Value))
                return;

            if (RM.drifter == null || !Radon.IsPlaying())
                return;

            MainMenu.Instance().SetState(MainMenu.State.Miracle, true, true, false, false);
            AudioController.Play("MIRACLE_START");
        }
    }
}
