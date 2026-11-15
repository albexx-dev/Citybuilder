using NUnit.Framework;

namespace Citybuilder
{
    // Счётчик по зонам (эпик #49): суммы, bucket-фолбэки и распределение занятых по долям мест.
    // Чистая логика без Unity: гоняется в редакторе и через dotnet вне репозитория.
    public class ZonalJobsTests
    {
        [Test]
        public void Total_And_ByZone_SumBuckets()
        {
            ZonalJobs jobs = ZoneJobs(2, 3, 5);
            Assert.AreEqual(10, jobs.Total);
            Assert.AreEqual(2, jobs.ByZone(ZoneType.Residential));
            Assert.AreEqual(3, jobs.ByZone(ZoneType.Commercial));
            Assert.AreEqual(5, jobs.ByZone(ZoneType.Industrial));
            Assert.AreEqual(0, jobs.ByZone(ZoneType.None));
        }

        [Test]
        public void AddJobs_SkipsUnknownZoneAndNonPositive()
        {
            ZonalJobs jobs = new ZonalJobs();
            jobs.AddJobs(ZoneType.Commercial, 6);
            jobs.AddJobs(ZoneType.None, 4);
            jobs.AddJobs((ZoneType)99, 7);
            jobs.AddJobs(ZoneType.Industrial, -1);
            jobs.AddJobs(ZoneType.Industrial, 0);
            Assert.AreEqual(6, jobs.Total);
        }

        [Test]
        public void DistributeEmployed_SumsExactlyToEmployed()
        {
            ZonalJobs employed = ZoneJobs(2, 3, 5).DistributeEmployed(8);
            Assert.AreEqual(1, employed.Residential);
            Assert.AreEqual(2, employed.Commercial);
            Assert.AreEqual(5, employed.Industrial);
            Assert.AreEqual(8, employed.Total);
        }

        [Test]
        public void DistributeEmployed_ShareFollowsJobCounts()
        {
            ZonalJobs employed = ZoneJobs(6, 0, 8).DistributeEmployed(7);
            Assert.AreEqual(3, employed.Residential);
            Assert.AreEqual(0, employed.Commercial);
            Assert.AreEqual(4, employed.Industrial);
        }

        [Test]
        public void DistributeEmployed_WithoutJobs_AllToResidential()
        {
            ZonalJobs employed = new ZonalJobs().DistributeEmployed(4);
            Assert.AreEqual(4, employed.Residential);
            Assert.AreEqual(0, employed.Commercial);
            Assert.AreEqual(0, employed.Industrial);
        }

        [Test]
        public void DistributeEmployed_WithoutWorkers_AllZero()
        {
            ZonalJobs employed = ZoneJobs(2, 3, 5).DistributeEmployed(0);
            Assert.AreEqual(0, employed.Total);
            Assert.AreEqual(0, employed.Residential);
        }

        private static ZonalJobs ZoneJobs(int residential, int commercial, int industrial)
        {
            return new ZonalJobs
            {
                Residential = residential,
                Commercial = commercial,
                Industrial = industrial
            };
        }
    }
}
