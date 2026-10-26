using NUnit.Framework;

namespace Citybuilder
{
    // Ступень цели в слоте v4: запись, JSON-roundtrip, старые перегрузки и восстановление.
    public class GoalSaveTests
    {
        [Test]
        public void BuildSnapshot_WithGoalStage_WritesVersionFour()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 5, null, 2, null, 2);
            Assert.AreEqual(4, snapshot.Version);
            Assert.AreEqual(2, snapshot.GoalStage);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Json_Roundtrip_KeepsGoalStage()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.GoalStage = 2;
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.AreEqual(4, restored.Version);
            Assert.AreEqual(2, restored.GoalStage);
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
        }

        // Слоты v1..v3 поля не знают: после чтения ступень остаётся нулевой.
        [Test]
        public void Json_LegacySlotWithoutField_DefaultsZero()
        {
            CitySnapshot restored = SaveSlotStorage.FromJson("{\"Version\":3,\"Balance\":10}");
            Assert.AreEqual(3, restored.Version);
            Assert.AreEqual(0, restored.GoalStage);
        }

        [Test]
        public void BuildSnapshot_OldOverloads_WriteZeroGoalStage()
        {
            CitySnapshot six = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            CitySnapshot eight = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 1, null);
            Assert.AreEqual(4, six.Version);
            Assert.AreEqual(0, six.GoalStage);
            Assert.AreEqual(4, eight.Version);
            Assert.AreEqual(0, eight.GoalStage);
        }

        [Test]
        public void BuildSnapshot_OutOfRangeGoalStage_WritesZero()
        {
            CitySnapshot high = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 0, null, 99);
            CitySnapshot negative = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 0, null, -1);
            Assert.AreEqual(0, high.GoalStage);
            Assert.AreEqual(0, negative.GoalStage);
        }

        [Test]
        public void Validate_VersionsOneToFour_Pass()
        {
            CitySnapshot snapshot = new CitySnapshot();
            for (int version = 1; version <= 4; version++)
            {
                snapshot.Version = version;
                Assert.IsTrue(SaveSlotStorage.Validate(snapshot), "version=" + version);
            }
        }

        [Test]
        public void Validate_VersionFive_Fails()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = 5;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Restore_SecondStage_Target200()
        {
            GoalStages stages = new GoalStages();
            stages.Restore(2);
            Assert.AreEqual(2, stages.Stage);
            Assert.AreEqual(200, stages.Target);
            Assert.IsFalse(stages.AllDone);
        }

        [Test]
        public void Restore_Negative_ClampsToZero()
        {
            GoalStages stages = new GoalStages();
            stages.Restore(-5);
            Assert.AreEqual(0, stages.Stage);
            Assert.AreEqual(50, stages.Target);
        }

        [Test]
        public void Restore_AboveStageCount_AllDone()
        {
            GoalStages stages = new GoalStages();
            stages.Restore(99);
            Assert.AreEqual(3, stages.Stage);
            Assert.IsTrue(stages.AllDone);
        }
    }
}
