using MelonLoader;
using Radon.Modules;

namespace Radon
{
    public partial class Radon : MelonMod
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
                ScaleBullets.Patch(Settings.ScaleBullets.Value);
                PhantomVisualizer.Patch(Settings.PhantomVisualizer.Value);
            }
            else
            {
                PhantomVisualizer.Patch(false);
                ScaleBullets.Patch(false);
                Savestates.Patch(false);
                NeonLite.Modules.Anticheat.Unregister(MelonAssembly);
            }

            Active = enabled;
        }
    }
}
