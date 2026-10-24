using HarmonyLib;
using Radon.Utils;
using System.Reflection;
using UniverseLib.Input;

namespace Radon.Modules
{
    internal class Noclip
    {
        static readonly MethodInfo ogmcu = AccessTools.Method(typeof(MechController), "Update");
        static readonly TogglePatch noclipPatch = TogglePatch.Postfix(ogmcu, typeof(Noclip), nameof(PostUpdate));
        private static readonly FieldInfo noclipField = AccessTools.Field(typeof(FirstPersonDrifter), "_noclip");

        public static void Patch(bool apply)
        {
            if (!apply && RM.drifter != null && IsActive())
                RM.drifter.SetNoclip(false);

            noclipPatch.Patch(apply);
        }
        public static void PostUpdate()
        {
            if (!InputManager.GetKeyDown(Radon.Settings.NoclipKey.Value))
                return;

            if (RM.drifter == null)
                return;

            RM.drifter.SetNoclip(!IsActive());
        }
        private static bool IsActive()
        {
            return (bool)noclipField.GetValue(RM.drifter);
        }
    }
}
