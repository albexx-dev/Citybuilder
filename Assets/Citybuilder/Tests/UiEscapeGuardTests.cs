using NUnit.Framework;

namespace Citybuilder
{
    // Чистые тесты общего Esc-гарда и приоритета слоёв UI (без Unity Input/сцены).
    public class UiEscapeGuardTests
    {
        [Test]
        public void UiEscapeGuard_OnePerFrame_FirstWinsAndResetsNextFrame()
        {
            UiEscapeGuard.Reset();
            Assert.IsTrue(UiEscapeGuard.TryConsume(1));
            Assert.IsFalse(UiEscapeGuard.TryConsume(1));
            Assert.IsTrue(UiEscapeGuard.TryConsume(2));
            Assert.IsFalse(UiEscapeGuard.TryConsume(2));
        }

        [Test]
        public void EscPriority_ModalAndInfoPanel_DeclinePausePath()
        {
            // Пока открыты слоты/настройки или инфопанель, путь паузы не трогает Esc.
            Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(true, 0));
            Assert.AreEqual(BuildModeHotkeys.EscAction.None,
                BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Houses, true));
            Assert.AreEqual(BuildModeHotkeys.EscAction.OpenPause,
                BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Off, false));
            Assert.AreEqual(BuildModeHotkeys.EscAction.ClosePause,
                BuildModeHotkeys.EvaluateEsc(false, true, BuildModeMenu.Mode.Off, false));
        }

        [Test]
        public void EscPriority_NoneAction_DoesNotConsumeFrame()
        {
            UiEscapeGuard.Reset();
            Assert.IsFalse(BuildModeHotkeys.TryBeginEscape(BuildModeHotkeys.EscAction.None, 5));
            Assert.IsTrue(UiEscapeGuard.TryConsume(5));
            Assert.IsTrue(BuildModeHotkeys.TryBeginEscape(BuildModeHotkeys.EscAction.OpenPause, 6));
            Assert.IsFalse(UiEscapeGuard.TryConsume(6));
        }

        [Test]
        public void EscPriority_GuardStopsPauseAfterPanelConsumedFrame()
        {
            UiEscapeGuard.Reset();
            int frame = 17;
            // Инфопанель забрала Esc первым в кадре.
            Assert.IsTrue(UiEscapeGuard.TryConsume(frame));
            // Пауза в том же кадре уже не может сработать.
            Assert.IsFalse(UiEscapeGuard.TryConsume(frame));
            // На следующем кадре гард снова свободен.
            Assert.IsTrue(UiEscapeGuard.TryConsume(frame + 1));
        }

        [Test]
        public void EscPriority_FullStack_InfoPanelBeatsSettingsPauseAndMode()
        {
            UiEscapePriority.Target target = UiEscapePriority.Evaluate(false, true, BuildModeMenu.Mode.Houses,
                true, true, true, true, true);
            Assert.AreEqual(UiEscapePriority.Target.InfoPanel, target);
            Assert.AreEqual(BuildModeHotkeys.EscAction.None, UiEscapePriority.ToLegacyAction(target));
        }

        [Test]
        public void EscPriority_PanelBeatsPauseAndBuildMode()
        {
            Assert.AreEqual(UiEscapePriority.Target.Panel, UiEscapePriority.Evaluate(false, true, BuildModeMenu.Mode.Roads,
                false, false, false, false, true));
            Assert.AreEqual(UiEscapePriority.Target.Pause, UiEscapePriority.Evaluate(false, true, BuildModeMenu.Mode.Roads,
                false, false, false, false, false));
            Assert.AreEqual(UiEscapePriority.Target.BuildMode, UiEscapePriority.Evaluate(false, false, BuildModeMenu.Mode.Roads,
                false, false, false, false, false));
            Assert.AreEqual(UiEscapePriority.Target.OpenPause, UiEscapePriority.Evaluate(false, false, BuildModeMenu.Mode.Off,
                false, false, false, false, false));
        }

        [Test]
        public void EscPriority_MainMenuAndSlots_SuppressPause()
        {
            Assert.AreEqual(UiEscapePriority.Target.None, UiEscapePriority.Evaluate(true, false, BuildModeMenu.Mode.Off,
                false, false, false, false, false));
            Assert.AreEqual(UiEscapePriority.Target.Settings, UiEscapePriority.Evaluate(false, false, BuildModeMenu.Mode.Houses,
                false, true, false, false, false));
            Assert.AreEqual(UiEscapePriority.Target.SaveSlot, UiEscapePriority.Evaluate(false, false, BuildModeMenu.Mode.Houses,
                false, false, true, false, false));
        }

        [Test]
        public void EscPriority_Guard_OnlyOneTargetPerFrame()
        {
            UiEscapeGuard.Reset();
            int frame = 31;
            Assert.IsTrue(UiEscapePriority.TryBegin(UiEscapePriority.Target.Panel, frame));
            Assert.IsFalse(UiEscapePriority.TryBegin(UiEscapePriority.Target.BuildMode, frame));
            Assert.IsTrue(UiEscapePriority.TryBegin(UiEscapePriority.Target.BuildMode, frame + 1));
            UiEscapeGuard.Reset();
        }

        [Test]
        public void EscPriority_Resolve_InfoPanelWinsOverOpenSettingsAndConsumesFrame()
        {
            UiEscapeGuard.Reset();
            int frame = 41;
            UiEscapePriority.Target target = UiEscapePriority.Resolve(0, frame, false, false,
                BuildModeMenu.Mode.Houses, true, true, true, true, true);
            Assert.AreEqual(UiEscapePriority.Target.InfoPanel, target);
            Assert.IsFalse(UiEscapeGuard.TryConsume(frame));
            UiEscapeGuard.Reset();
        }

        [Test]
        public void EscPriority_Resolve_SettingsTargetIsLiveAndKeyboardInputBlocksEsc()
        {
            UiEscapeGuard.Reset();
            Assert.AreEqual(UiEscapePriority.Target.Settings, UiEscapePriority.Resolve(0, 42, false, false,
                BuildModeMenu.Mode.Off, false, true, false, false, false));
            Assert.AreEqual(UiEscapePriority.Target.None, UiEscapePriority.Resolve(1, 43, false, false,
                BuildModeMenu.Mode.Off, false, true, false, false, false));
            Assert.IsTrue(UiEscapeGuard.TryConsume(43));
            UiEscapeGuard.Reset();
        }
    }
}
