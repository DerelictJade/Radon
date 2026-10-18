using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Radon.Modules
{
    internal class Bullets
    {
        private static bool _scalePatched = false;
        private static bool _phantomPatched = false;
        private const float PhantomDissolve = 0.5f;

        private static readonly string[] _excludePaths = new string[]
        {
            "Projectiles/ProjectileBomb",
            "Projectiles/ProjectileMine",
            "Projectiles/ProjectileRocketFast"
        };

        static readonly MethodInfo ogpbcp = AccessTools.Method(typeof(ProjectileBase), "CreateProjectile", new Type[] { typeof(string), typeof(Vector3), typeof(Vector3), typeof(ProjectileWeapon) });
        static readonly MethodInfo pbcppostfixmi = typeof(Bullets).GetMethod(nameof(PostCreateProjectile));
        static readonly HarmonyMethod pbcppostfix = new HarmonyMethod(pbcppostfixmi);

        static readonly MethodInfo ogpbut = AccessTools.Method(typeof(ProjectileBase), "UpdateTime");
        static readonly MethodInfo pbutpostfixmi = typeof(Bullets).GetMethod(nameof(PostUpdateTime));
        static readonly HarmonyMethod pbutpostfix = new HarmonyMethod(pbutpostfixmi);

        static readonly MethodInfo ogebi = AccessTools.Method(typeof(Exploder), "BulletImpact");
        static readonly MethodInfo ebiprefixmi = typeof(Bullets).GetMethod(nameof(PreBulletImpact));
        static readonly HarmonyMethod ebiprefix = new HarmonyMethod(ebiprefixmi);

        public static void Patch(bool apply)
        {
            PatchScale(apply && Radon.Settings.ScaleBullets.Value);
            PatchPhantom(apply && Radon.Settings.PhantomVisualizer.Value);
        }

        public static void PatchScale(bool apply)
        {
            if (_scalePatched == apply)
                return;

            if (apply)
                Radon.Harmony.Patch(ogpbcp, postfix: pbcppostfix);
            else
                Radon.Harmony.Unpatch(ogpbcp, pbcppostfixmi);

            _scalePatched = apply;
        }

        public static void PatchPhantom(bool apply)
        {
            if (_phantomPatched == apply)
                return;

            if (apply)
            {
                Radon.Harmony.Patch(ogpbut, postfix: pbutpostfix);
                Radon.Harmony.Patch(ogebi, prefix: ebiprefix);
            }
            else
            {
                Radon.Harmony.Unpatch(ogpbut, pbutpostfixmi);
                Radon.Harmony.Unpatch(ogebi, ebiprefixmi);
            }

            _phantomPatched = apply;
        }

        public static void PostCreateProjectile(string path, ProjectileBase __result)
        {
            if (__result == null || _excludePaths.Contains(path))
                return;

            float size = Radon.Settings.BulletSize.Value;
            float visualScale = size / 2f;

            __result._collisionRadiusDamageable = size;
            __result.GetComponent<SphereCollider>().radius = size;
            __result.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }

        public static void PostUpdateTime(ProjectileBase __instance, bool ____isInHitDecayMode)
        {
            if (____isInHitDecayMode)
                __instance.SetMaterialPropertyFloat(BaseDamageable.shaderIDDissolve, PhantomDissolve);
        }

        public static bool PreBulletImpact()
        {
            return false;
        }
    }
}
