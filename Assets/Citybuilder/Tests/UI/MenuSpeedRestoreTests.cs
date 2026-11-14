using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Восстановление скорости симуляции при закрытии меню (issue #42):
    // x4 должен переживать паузу и цепочки меню слотов.
    public class MenuSpeedRestoreTests
    {
        [Test]
        public void Menus_RestorePreviousSpeed_FastForwardSurvivesPauseAndSlots()
        {
            GameObject go = new GameObject("TestSpeedRestore");
            try
            {
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                PauseMenuView pause = go.AddComponent<PauseMenuView>();
                SaveMenuView save = go.AddComponent<SaveMenuView>();
                LoadMenuView load = go.AddComponent<LoadMenuView>();
                pause.Runner = runner; save.Runner = runner; save.Pause = pause; load.Runner = runner; load.Pause = pause;
                runner.SetSpeed(SimulationSpeed.FastForward);
                pause.Open();
                Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Close();
                save.Open();
                Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                save.ReturnToPause();
                Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Close();
                Assert.AreEqual(SimulationSpeed.FastForward, runner.Speed);
                load.Open(); load.Close();
                Assert.AreEqual(SimulationSpeed.FastForward, runner.Speed);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
