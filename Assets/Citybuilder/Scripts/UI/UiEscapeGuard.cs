namespace Citybuilder
{
    // Общий гард Esc на один кадр: первый обработчик, забравший Esc, блокирует остальные.
    // Приоритет задаётся вызывающими: слоты/настройки/инфопанель не дают пути паузы
    // забрать кадр, пока открыты, поэтому в кадре срабатывает только верхний слой UI.
    public static class UiEscapeGuard
    {
        private static int consumedFrame = -1;

        // true — кадр свободен, Esc забираем; повтор в том же кадре вернёт false.
        public static bool TryConsume(int frame)
        {
            if (consumedFrame == frame) return false;
            consumedFrame = frame;
            return true;
        }

        // Сброс между тестами/сценами: статическое состояние не живёт дольше кадра.
        public static void Reset()
        {
            consumedFrame = -1;
        }
    }
}
