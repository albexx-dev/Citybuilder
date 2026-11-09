using NUnit.Framework;

namespace Citybuilder
{
    // Матрица привлекательности (EPIC-05 шаг 1): сдвиг по каждому фактору, границы 0.5/1.5,
    // нейтральность NaN. Политика чистая и без состояния — Unity тестам не нужен.
    public class AttractivenessPolicyTests
    {
        private const float Delta = 1e-4f;
        private const float Nan = 0f / 0f;

        // Короткий вызов политики: строки матрицы остаются в одной строке файла.
        private static float Value(float leisure, float health, float power, float water,
            float waste, float congestion, bool deficit)
        {
            return AttractivenessPolicy.Evaluate(leisure, health, power, water, waste, congestion, deficit);
        }

        // Нейтральные факторы: база 1.0, ни один вклад её не смещает.
        [Test]
        public void Evaluate_NeutralFactors_ReturnsBase()
        {
            Assert.AreEqual(1f, Value(0f, 0f, 1f, 1f, 1f, 1f, false), Delta);
        }

        // Каждый фактор по отдельности: направление и величина из формулы плана.
        [TestCase(1f, 0f, 1f, 1f, 1f, 1f, false, 1.2f)]           // досуг: +0.2 × 1
        [TestCase(0.5f, 0f, 1f, 1f, 1f, 1f, false, 1.1f)]         // досуг: +0.2 × 0.5
        [TestCase(0f, 1f, 1f, 1f, 1f, 1f, false, 1.1f)]           // здоровье: +0.1 × 1
        [TestCase(0f, 0.4f, 1f, 1f, 1f, 1f, false, 1.04f)]        // здоровье: +0.1 × 0.4
        [TestCase(0f, 0f, 0f, 1f, 1f, 1f, false, 0.7f)]           // энергия 0: −0.3 × (1 − 0)
        [TestCase(0f, 0f, 1f, 0f, 1f, 1f, false, 0.7f)]           // вода 0: −0.3 × 1
        [TestCase(0f, 0f, 1f, 1f, 0f, 1f, false, 0.7f)]           // мусор 0: −0.3 × 1
        [TestCase(0f, 0f, 0.5f, 1f, 1f, 1f, false, 0.85f)]        // энергия 0.5: −0.3 × 0.5
        [TestCase(0f, 0f, 0.6f, 0.9f, 0.2f, 1f, false, 0.76f)]    // штраф по худшей 0.2, не по средней
        [TestCase(0f, 0f, 1f, 1f, 1f, 2f, false, 0.9f)]           // пробки ×2: −0.1 × (2 − 1)
        [TestCase(0f, 0f, 1f, 1f, 1f, 3f, false, 0.8f)]           // пробки ×3: −0.1 × (3 − 1)
        [TestCase(0f, 0f, 1f, 1f, 1f, 0.5f, false, 1f)]           // быстрее нормы — бонуса нет
        [TestCase(0f, 0f, 1f, 1f, 1f, 1f, true, 0.8f)]            // дефицит: −0.2
        public void Evaluate_SingleFactorShiftsMultiplier(float leisure, float health, float power,
            float water, float waste, float congestion, bool deficit, float expected)
        {
            Assert.AreEqual(expected, Value(leisure, health, power, water, waste, congestion, deficit), Delta);
        }

        // Все факторы идеальны в своей шкале: 1 + 0.2 + 0.1 = 1.3, выше потолка клампа не бывает.
        [Test]
        public void Evaluate_AllIdeal_SumsUpliftsAndStaysUnderCeiling()
        {
            float actual = Value(1f, 1f, 1f, 1f, 1f, 1f, false);
            Assert.AreEqual(1.3f, actual, Delta);
            Assert.LessOrEqual(actual, AttractivenessPolicy.MaxMultiplier);
        }

        // Перебор в покрытиях: 1 + 0.2 × 2 + 0.1 × 2 = 1.6 → кламп ровно 1.5.
        [Test]
        public void Evaluate_AboveCeiling_ClampsToMax()
        {
            Assert.AreEqual(1.5f, Value(2f, 2f, 1f, 1f, 1f, 1f, false), Delta);
        }

        // Всё плохо в своей шкале: 1 − 0.3 − 0.2 = ровно 0.5, ниже пола клампа.
        [Test]
        public void Evaluate_AllBad_IsFloor()
        {
            float actual = Value(0f, 0f, 0f, 0f, 0f, 1f, true);
            Assert.AreEqual(0.5f, actual, Delta);
            Assert.GreaterOrEqual(actual, AttractivenessPolicy.MinMultiplier);
        }

        // Недобор ниже пола: 1 − 0.3 − 0.1 × 2 − 0.2 = 0.3 → кламп ровно 0.5.
        [Test]
        public void Evaluate_BelowFloor_ClampsToMin()
        {
            Assert.AreEqual(0.5f, Value(0f, 0f, 0f, 0f, 0f, 3f, true), Delta);
        }

        // Дефицит против равного вызова без него: разница ровно его вклад 0.2.
        [Test]
        public void Evaluate_DeficitDiffersByExactlyItsPenalty()
        {
            float without = Value(0.5f, 0.4f, 0.8f, 0.9f, 0.7f, 1.5f, false);
            Assert.AreEqual(0.2f, without - Value(0.5f, 0.4f, 0.8f, 0.9f, 0.7f, 1.5f, true), Delta);
        }

        // NaN в каждом факторе по очереди: вклад 0, остальные факторы считаются как обычно.
        [TestCase(Nan, 1f, 1f, 1f, 1f, 1f, false, 1.1f)]          // досуг, здоровье +0.1
        [TestCase(0.5f, Nan, 1f, 1f, 1f, 1f, false, 1.1f)]        // здоровье, досуг +0.2 × 0.5
        [TestCase(Nan, Nan, 1f, 1f, 1f, 1f, false, 1f)]           // оба покрытия, только база
        [TestCase(0f, 0f, Nan, 1f, 0.5f, 1f, false, 0.85f)]       // энергия, мусор −0.3 × 0.5
        [TestCase(0f, 0f, 0.4f, Nan, 0.3f, 1f, false, 0.79f)]     // вода, худшая 0.3: −0.3 × 0.7
        [TestCase(0f, 0f, 1f, 1f, Nan, 1f, false, 1f)]            // мусор
        [TestCase(0f, 0f, 1f, 1f, 1f, Nan, false, 1f)]            // пробки
        [TestCase(0f, 0f, 1f, 1f, 1f, Nan, true, 0.8f)]           // пробки, дефицит −0.2
        [TestCase(Nan, Nan, Nan, Nan, Nan, Nan, true, 0.8f)]      // все факторы: одна база и дефицит
        public void Evaluate_NaNSubstitutions(float leisure, float health, float power,
            float water, float waste, float congestion, bool deficit, float expected)
        {
            float actual = Value(leisure, health, power, water, waste, congestion, deficit);
            Assert.IsFalse(float.IsNaN(actual));
            Assert.AreEqual(expected, actual, Delta);
        }

        // NaN-фактор = тот же вызов с нейтральным значением этого фактора (0, 0, 1, 1, 1).
        [Test]
        public void Evaluate_NaNEqualsNeutralSubstitute()
        {
            Assert.AreEqual(Value(0f, 0.3f, 0.4f, 0.5f, 0.6f, 1.7f, true),
                Value(Nan, 0.3f, 0.4f, 0.5f, 0.6f, 1.7f, true), Delta);
            Assert.AreEqual(Value(0.8f, 0f, 0.4f, 0.5f, 0.6f, 1.7f, true),
                Value(0.8f, Nan, 0.4f, 0.5f, 0.6f, 1.7f, true), Delta);
            Assert.AreEqual(Value(0.8f, 0.3f, 1f, 0.5f, 0.6f, 1.7f, true),
                Value(0.8f, 0.3f, Nan, 0.5f, 0.6f, 1.7f, true), Delta);
            Assert.AreEqual(Value(0.8f, 0.3f, 0.4f, 1f, 0.6f, 1.7f, true),
                Value(0.8f, 0.3f, 0.4f, Nan, 0.6f, 1.7f, true), Delta);
            Assert.AreEqual(Value(0.8f, 0.3f, 0.4f, 0.5f, 1f, 1.7f, true),
                Value(0.8f, 0.3f, 0.4f, 0.5f, Nan, 1.7f, true), Delta);
            Assert.AreEqual(Value(0.8f, 0.3f, 0.4f, 0.5f, 0.6f, 1f, true),
                Value(0.8f, 0.3f, 0.4f, 0.5f, 0.6f, Nan, true), Delta);
        }
    }
}
