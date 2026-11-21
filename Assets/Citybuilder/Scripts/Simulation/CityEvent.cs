using System;
using System.Globalization;
using System.Text;

namespace Citybuilder
{
    // Одно событие в ленте города (EPIC-09 шаг 1, ADR-007/013): только данные.
    // Домен хранит ключ сообщения и аргументы; готовую строку собирает EventLogView.
    public struct CityEvent
    {
        public readonly EventKind Kind;
        public readonly EventSubtype Subtype;
        public readonly string MessageKey;
        public readonly object[] Args;
        public readonly int Tick;

        public CityEvent(EventKind kind, string messageKey, object[] args, int tick)
            : this(kind, EventSubtype.None, messageKey, args, tick)
        {
        }

        public CityEvent(EventKind kind, EventSubtype subtype, string messageKey, object[] args, int tick)
        {
            Kind = kind;
            Subtype = subtype;
            MessageKey = messageKey ?? string.Empty;
            Args = args == null ? new object[0] : (object[])args.Clone();
            Tick = tick < 0 ? 0 : tick;
        }

        // Равенство по значению: Args копируется в конструкторе, поэтому ссылочное
        // сравнение struct ломало тесты детерминизма пожаров/краж (ADR-013).
        public override bool Equals(object obj)
        {
            if (!(obj is CityEvent)) return false;
            CityEvent other = (CityEvent)obj;
            return Kind == other.Kind && Subtype == other.Subtype && Tick == other.Tick
                && MessageKey == other.MessageKey && ArgsKey(Args) == ArgsKey(other.Args);
        }

        public override int GetHashCode()
        {
            int code = (int)Kind * 397 ^ (int)Subtype ^ Tick;
            code = code * 397 ^ (MessageKey == null ? 0 : MessageKey.GetHashCode());
            return code * 397 ^ ArgsKey(Args).GetHashCode();
        }

        // Стабильный вид аргументов для дедупа и ключа опроса звука: язык не участвует.
        public static string ArgsKey(object[] args)
        {
            if (args == null || args.Length == 0) return string.Empty;
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                text.Append(args[i] == null ? "~" : Convert.ToString(args[i], CultureInfo.InvariantCulture));
                text.Append('|');
            }
            return text.ToString();
        }
    }
}
