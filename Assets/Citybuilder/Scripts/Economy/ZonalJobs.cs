namespace Citybuilder
{
    // Счётчик по трём зонам (эпик #49): рабочие места или занятые. Чистые данные без Unity.
    // Зоны вне трёх известных (None) не считаются: здания вне зон налога не платят.
    public class ZonalJobs
    {
        public int Residential;
        public int Commercial;
        public int Industrial;

        // Сумма по всем трём зонам.
        public int Total
        {
            get { return Residential + Commercial + Industrial; }
        }

        // Значение по зоне. None и битые зоны — 0.
        public int ByZone(ZoneType zone)
        {
            if (zone == ZoneType.Residential) return Residential;
            if (zone == ZoneType.Commercial) return Commercial;
            if (zone == ZoneType.Industrial) return Industrial;
            return 0;
        }

        // Прибавить рабочие места здания по AllowedZone его типа. Неизвестная зона — пропуск.
        public void AddBuilding(BuildingState state)
        {
            if (state == null || state.Definition == null)
            {
                return;
            }
            AddJobs(state.Definition.AllowedZone,
                BuildingDefinition.ClampNonNegative(state.Definition.JobsProvided));
        }

        // Прибавить места к зоне. Неизвестная зона и неположительные значения — пропуск.
        public void AddJobs(ZoneType zone, int jobs)
        {
            if (jobs <= 0) return;
            if (zone == ZoneType.Residential) Residential += jobs;
            else if (zone == ZoneType.Commercial) Commercial += jobs;
            else if (zone == ZoneType.Industrial) Industrial += jobs;
        }

        // Распределить занятых по долям рабочих мест зон: кумулятивные доли, сумма ровно employed.
        // Мест нет — вся занятость уходит в жилую зону (сбор как при одной ставке).
        public ZonalJobs DistributeEmployed(int employed)
        {
            ZonalJobs result = new ZonalJobs();
            if (employed <= 0)
            {
                return result;
            }
            int total = Total;
            if (total <= 0)
            {
                result.Residential = employed;
                return result;
            }
            int residentialCumulative = employed * Residential / total;
            int commercialCumulative = residentialCumulative + employed * Commercial / total;
            result.Residential = residentialCumulative;
            result.Commercial = commercialCumulative - residentialCumulative;
            result.Industrial = employed - commercialCumulative;
            return result;
        }
    }
}
