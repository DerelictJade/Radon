using MelonLoader;
using Radon.Modules;
using Radon.UI;

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
            Utils.Swag.Apply();

            Game.OnInitializationComplete += () =>
            {
                TimeScaleDisplay.Create();

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
                Noclip.Patch(true);
                Miracle.Patch(true);
                TimeControl.Patch(true);
                ScaleBullets.Patch(Settings.ScaleBullets.Value);
                PhantomVisualizer.Patch(Settings.PhantomVisualizer.Value);
                Miracle.PatchStart(Settings.StartWithMiracle.Value);
            }
            else
            {
                Miracle.PatchStart(false);
                PhantomVisualizer.Patch(false);
                ScaleBullets.Patch(false);
                TimeControl.Patch(false);
                Miracle.Patch(false);
                Noclip.Patch(false);
                Savestates.Patch(false);
                NeonLite.Modules.Anticheat.Unregister(MelonAssembly);
            }

            Active = enabled;
        }
        internal static bool IsPlaying()
        {
            LevelData level = Game.GetCurrentLevel();
            return MainMenu.Instance().GetCurrentState() == MainMenu.State.None && level != null && level.type != LevelData.LevelType.Hub;
        }
    }
}
