namespace Citybuilder
{
    // Скорость симуляции. Пауза останавливает тики,
    // Frame Update (камера, ввод, UI) продолжает работать.
    public enum SimulationSpeed
    {
        Paused = 0,
        Playing = 1,
        FastForward = 4
    }
}
