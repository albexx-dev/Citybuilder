using UnityEngine;

namespace Citybuilder
{
    // Мост дефицита энергии/воды/мусора в темп роста домов. Считает SupplyGrowthPolicy.
    // Настройка: объект Grid, привязки — существующие Power/Water/WasteController.
    public class SupplyGrowthController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Энергия города. Пусто = штрафа нет.")]
        public PowerController Power;

        [Tooltip("Вода города. Пусто = штрафа нет.")]
        public WaterController Water;

        [Tooltip("Мусор города. Пусто = штрафа нет.")]
        public WasteController Waste;

        // Во сколько раз дольше растёт дом при худшем дефиците. Без ссылок — 1.
        public int GrowthTickMultiplier
        {
            get
            {
                return SupplyGrowthPolicy.Multiplier(
                    Power != null ? Power.Rate01 : 1f,
                    Water != null ? Water.Rate01 : 1f,
                    Waste != null ? Waste.Rate01 : 1f);
            }
        }
    }
}
