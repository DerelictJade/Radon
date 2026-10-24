using HarmonyLib;
using Radon.Utils;
using System.Reflection;

namespace Radon.Modules
{
    internal class PhantomVisualizer
    {
        private const float PhantomDissolve = 0.5f;
        static readonly MethodInfo ogpbut = AccessTools.Method(typeof(ProjectileBase), "UpdateTime");
        static readonly TogglePatch bulletPatch = TogglePatch.Postfix(ogpbut, typeof(PhantomVisualizer), nameof(PostUpdateTime));
        static readonly MethodInfo ogebi = AccessTools.Method(typeof(Exploder), "BulletImpact");
        static readonly TogglePatch bulletImpactPatch = TogglePatch.Prefix(ogebi, typeof(PhantomVisualizer), nameof(PreBulletImpact));

        public static void Patch(bool apply)
        {
            bulletPatch.Patch(apply);
            bulletImpactPatch.Patch(apply);
        }
        public static void PostUpdateTime(ProjectileBase __instance, bool ____isInHitDecayMode)
        {
            if (____isInHitDecayMode)
                __instance.SetMaterialPropertyFloat(BaseDamageable.shaderIDDissolve, PhantomDissolve);
        }
        public static bool PreBulletImpact() => false; // Keeps the bullet alive after it hits something
    }
}
