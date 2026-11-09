using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты логики Esc-приоритета, флага грязности города и подтверждения выхода (EPIC-16).
    public class MenuAndPauseTests
    {
        [Test]
        public void EscPriority_PauseAndBuildModes_FollowsEpic16Rules()
        {
            Assert.AreEqual(BuildModeHotkeys.EscAction.None,
                BuildModeHotkeys.EvaluateEsc(true, false, BuildModeMenu.Mode.Houses));

            Assert.AreEqual(BuildModeHotkeys.EscAction.ClosePause,
                BuildModeHotkeys.EvaluateEsc(false, true, BuildModeMenu.Mode.Off));

            Assert.AreEqual(BuildModeHotkeys.EscAction.TurnOffMode,
                BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Roads));

            Assert.AreEqual(BuildModeHotkeys.EscAction.OpenPause,
                BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Off));
        }

        [Test]
        public void DirtyFlag_MarkAndClear_ControlsExitConfirmation()
        {
            GameObject go = new GameObject("TestSave");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                Assert.IsFalse(save.IsDirty);
                Assert.IsFalse(PauseMenuView.NeedsExitConfirmation(save.IsDirty));

                save.MarkDirty();
                Assert.IsTrue(save.IsDirty);
                Assert.IsTrue(PauseMenuView.NeedsExitConfirmation(save.IsDirty));

                save.ClearDirty();
                Assert.IsFalse(save.IsDirty);
                Assert.IsFalse(PauseMenuView.NeedsExitConfirmation(save.IsDirty));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
