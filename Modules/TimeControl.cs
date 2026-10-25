using HarmonyLib;
using Radon.UI;
using Radon.Utils;
using System.Reflection;
using UnityEngine;
using UniverseLib.Input;

namespace Radon.Modules
{
    internal class TimeControl
    {
        static readonly MethodInfo ogmcu = AccessTools.Method(typeof(MechController), "Update");
        static readonly TogglePatch timePatch = TogglePatch.Postfix(ogmcu, typeof(TimeControl), nameof(PostUpdate));

        private const int StepPercent = 5;
        private const int MinPercent = 15;
        private const int MaxPercent = 200;
        private static int _currentPercent = 100;
        private static int _displayedPercent = 100;

        public static void Patch(bool apply)
        {
            if (!apply)
            {
                if (RM.time != null)
                {
                    float target = RM.time.GetTargetTimeScale();
                    if (target != 0f && target != 1f)
                        RM.time.SetTargetTimescale(1f);
                }

                _currentPercent = 100;
                _displayedPercent = 100;

                if (TimeScaleDisplay.Instance != null)
                    TimeScaleDisplay.Instance.Hide();
            }

            timePatch.Patch(apply);
        }
        public static void PostUpdate()
        {
            if (!Radon.IsPlaying())
                return;

            if (InputManager.GetKeyDown(Radon.Settings.SpeedResetKey.Value))
                _currentPercent = 100;
            else if (InputManager.GetKeyDown(Radon.Settings.SpeedUpKey.Value))
                _currentPercent = Mathf.Min(_currentPercent + StepPercent, MaxPercent);
            else if (InputManager.GetKeyDown(Radon.Settings.SpeedDownKey.Value))
                _currentPercent = Mathf.Max(_currentPercent - StepPercent, MinPercent);

            if (_currentPercent != _displayedPercent)
            {
                _displayedPercent = _currentPercent;
                if (TimeScaleDisplay.Instance != null)
                    TimeScaleDisplay.Instance.Show(_currentPercent);
            }

            float scale = _currentPercent / 100f;
            float current = RM.time.GetTargetTimeScale();
            if (current != 0f && current != scale)
                RM.time.SetTargetTimescale(scale);
        }
    }
}
