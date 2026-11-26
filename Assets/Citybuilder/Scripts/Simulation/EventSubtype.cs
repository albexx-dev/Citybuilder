namespace Citybuilder
{
    // Типизированное подсобытие внутри EventKind (ADR-013, issue #88).
    // Пожар: возгорание, сгоревший дом, тушение — разные подтипы, а не варианты текста.
    // Звук выбирается по этому полю, без разбора локализованного сообщения.
    public enum EventSubtype
    {
        None = 0,
        FireStart = 1,
        Burned = 2,
        Extinguished = 3,
        DecisionOffered = 4,
        DecisionSuppressed = 5,
        DecisionIgnored = 6
    }
}
