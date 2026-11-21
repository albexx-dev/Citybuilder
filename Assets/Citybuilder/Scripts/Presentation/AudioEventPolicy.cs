using System.Collections.Generic;

namespace Citybuilder
{
    // Какой звуковой сигнал соответствует событию ленты (хвосты #71/#66, ADR-007).
    // Чистый C# без Unity — образец AmbientMixPolicy: EditMode-тесты и dotnet-прогон.
    public enum AudioEventSound
    {
        None = 0,
        FireStart = 1,
        Burned = 2,
        Crime = 3,
        Garbage = 4,
        Blackout = 5
    }

    public static class AudioEventPolicy
    {
        // Маркеры сообщений FireSystem: «Пожар в доме #N!», «потушен», «Дом #N сгорел дотла».
        public const string BurnedMarker = "сгорел";
        public const string ExtinguishedMarker = "потушен";

        // Один сигнал на вид события: пожар, кража, вывоз мусора, отключение света.
        // Потушенный пожар молчит (хорошая новость — достаточно записи в ленте).
        // Казна/доход молчат всегда: coin.wav — только награда цели (GoalView.cs:64).
        public static AudioEventSound Resolve(EventKind kind, string message)
        {
            if (kind == EventKind.Fire && !string.IsNullOrEmpty(message))
            {
                if (message.Contains(BurnedMarker)) return AudioEventSound.Burned;
                if (!message.Contains(ExtinguishedMarker)) return AudioEventSound.FireStart;
            }
            if (kind == EventKind.Crime) return AudioEventSound.Crime;
            if (kind == EventKind.Garbage) return AudioEventSound.Garbage;
            if (kind == EventKind.Blackout) return AudioEventSound.Blackout;
            return AudioEventSound.None;
        }

        // Индекс первого необработанного события снимка ленты: хвост до прошлого ключа.
        // Ключ потерян (старт вью, «Новая игра») — берём только последнее событие,
        // чтобы не переигрывать всю историю ленты.
        public static int NewEventsStart(IReadOnlyList<CityEvent> snapshot, string lastKey)
        {
            if (snapshot == null || snapshot.Count == 0) return 0;
            for (int i = snapshot.Count - 1; i >= 0; i--)
            {
                if (KeyOf(snapshot[i]) == lastKey) return i + 1;
            }
            return snapshot.Count - 1;
        }

        // Ключ последнего обработанного события: тик, вид и текст (текст уникален по дому).
        public static string KeyOf(CityEvent cityEvent)
        {
            return cityEvent.Tick + ":" + (int)cityEvent.Kind + ":" + cityEvent.Message;
        }
    }
}
