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
    }
}
