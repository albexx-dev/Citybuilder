using System.Collections.Generic;

namespace Citybuilder
{
    // Какой звуковой сигнал соответствует событию ленты (хвосты #71/#66, ADR-007/013).
    // Чистый C# без Unity — образец AmbientMixPolicy: EditMode-тесты и dotnet-прогон.
    public enum AudioEventSound
    {
        None = 0,
        FireStart = 1,
        Burned = 2,
        Crime = 3,
        Garbage = 4,
        Blackout = 5,
        Supply = 6,
        Congestion = 7,
        Employment = 8
    }

    public static class AudioEventPolicy
    {
        // Один сигнал на вид события: пожар, сгоревший дом, кража, мусор, свет,
        // снабжение, пробки, занятость. Потушенный пожар молчит: хорошая новость — запись в ленте.
        // Казна/доход молчат всегда: coin.wav — только награда цели (GoalView.cs:64).
        // Никакого разбора текста: подтип и ключ не зависят от языка (ADR-013).
        public static AudioEventSound Resolve(CityEvent cityEvent)
        {
            // Пустой ключ — не запись ленты (CityEventLog её не добавит), в том числе default(CityEvent).
            if (string.IsNullOrEmpty(cityEvent.MessageKey)) return AudioEventSound.None;
            if (cityEvent.Subtype == EventSubtype.Burned) return AudioEventSound.Burned;
            if (cityEvent.Subtype == EventSubtype.FireStart) return AudioEventSound.FireStart;
            if (cityEvent.Subtype == EventSubtype.Extinguished) return AudioEventSound.None;
            if (cityEvent.Kind == EventKind.Crime) return AudioEventSound.Crime;
            if (cityEvent.Kind == EventKind.Garbage) return AudioEventSound.Garbage;
            if (cityEvent.Kind == EventKind.Blackout) return AudioEventSound.Blackout;
            if (cityEvent.Kind == EventKind.Supply) return AudioEventSound.Supply;
            if (cityEvent.Kind == EventKind.Congestion) return AudioEventSound.Congestion;
            if (cityEvent.Kind == EventKind.Employment) return AudioEventSound.Employment;
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

        // Ключ последнего обработанного события: тик, вид, ключ сообщения и аргументы.
        // Смена языка не меняет ключ — последнее событие не проигрывается повторно.
        public static string KeyOf(CityEvent cityEvent)
        {
            return cityEvent.Tick + ":" + (int)cityEvent.Kind + ":" + cityEvent.MessageKey
                + ":" + CityEvent.ArgsKey(cityEvent.Args);
        }
    }
}
