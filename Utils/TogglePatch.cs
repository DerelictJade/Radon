using HarmonyLib;
using System;
using System.Reflection;

namespace Radon.Utils
{
    internal class TogglePatch
    {
        private bool _patched = false;
        private readonly MethodBase _original;
        private readonly MethodInfo _patchmi;
        private readonly bool _isPrefix;
        private TogglePatch(MethodBase original, MethodInfo patchmi, bool isPrefix)
        {
            _original = original;
            _patchmi = patchmi;
            _isPrefix = isPrefix;
        }
        public static TogglePatch Prefix(MethodBase original, Type patchType, string patchName)
        {
            return new TogglePatch(original, AccessTools.Method(patchType, patchName), true);
        }
        public static TogglePatch Postfix(MethodBase original, Type patchType, string patchName)
        {
            return new TogglePatch(original, AccessTools.Method(patchType, patchName), false);
        }
        public void Patch(bool apply)
        {
            if (_patched == apply)
                return;
            if (!apply)
                Radon.Harmony.Unpatch(_original, _patchmi);
            else if (_isPrefix)
                Radon.Harmony.Patch(_original, prefix: new HarmonyMethod(_patchmi));
            else
                Radon.Harmony.Patch(_original, postfix: new HarmonyMethod(_patchmi));
            _patched = apply;
        }
    }
}
