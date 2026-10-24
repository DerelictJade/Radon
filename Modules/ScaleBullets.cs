using HarmonyLib;
using Radon.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Radon.Modules
{
    internal class ScaleBullets
    {
        static readonly MethodInfo ogpbcp = AccessTools.Method(typeof(ProjectileBase), "CreateProjectile", new Type[] { typeof(string), typeof(Vector3), typeof(Vector3), typeof(ProjectileWeapon) });
        static readonly TogglePatch sbbPatch = TogglePatch.Postfix(ogpbcp, typeof(ScaleBullets), nameof(PostCreateProjectile));

        private static readonly string[] _excludePaths = new string[]
        {
            "Projectiles/ProjectileBomb",
            "Projectiles/ProjectileMine",
            "Projectiles/ProjectileRocketFast"
        };
        public static void Patch(bool apply)
        {
            sbbPatch.Patch(apply);
        }
        public static void PostCreateProjectile(string path, ProjectileBase __result)
        {
            if (__result == null || _excludePaths.Contains(path))
                return;

            float size = Radon.Settings.BulletSize.Value;
            float visualScale = size / 2f; // don't know why we do this but slacking did

            __result._collisionRadiusDamageable = size;
            __result.GetComponent<SphereCollider>().radius = size;
            __result.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }
    }
}
