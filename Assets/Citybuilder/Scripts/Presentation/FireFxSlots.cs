using System;

namespace Citybuilder
{
    // Снимок горящих домов и пул слотов эффекта (issue #116): фиксированные массивы,
    // никаких List/GameObject в Update; слоты переиспользуются между синхронизациями.
    public sealed class FireFxSlots
    {
        public const int Capacity = 12;
        public const float CycleSec = 1.4f;

        private readonly EntityId[] ids = new EntityId[Capacity];
        private readonly float[] xs = new float[Capacity];
        private readonly float[] zs = new float[Capacity];
        private readonly float[] ages = new float[Capacity];
        private readonly bool[] seen = new bool[Capacity];
        private int used;

        public int Count { get { return used; } }
        public EntityId IdAt(int index) { return ids[index]; }
        public float XAt(int index) { return xs[index]; }
        public float ZAt(int index) { return zs[index]; }
        public float AgeAt(int index) { return ages[index]; }

        // Начало синхронизации: все слоты пока невиданные (без аллокаций).
        public void BeginSync() { Array.Clear(seen, 0, seen.Length); }

        // Горящий дом: обновить слот или занять свободный. true = слот создан сейчас.
        public bool Touch(EntityId id, float x, float z)
        {
            int index = IndexOf(id);
            if (index >= 0)
            {
                xs[index] = x;
                zs[index] = z;
                seen[index] = true;
                return false;
            }
            if (used >= Capacity) return false;
            index = used++;
            ids[index] = id;
            xs[index] = x;
            zs[index] = z;
            ages[index] = 0f;
            seen[index] = true;
            return true;
        }

        // Конец синхронизации: вытесняем слоты домов, которые перестали гореть.
        public void EndSync()
        {
            int write = 0;
            for (int i = 0; i < used; i++)
            {
                if (!seen[i]) continue;
                if (write != i)
                {
                    ids[write] = ids[i];
                    xs[write] = xs[i];
                    zs[write] = zs[i];
                    ages[write] = ages[i];
                }
                write++;
            }
            used = write;
        }

        // Фаза анимации всех активных слотов: время идёт по кругу, объекты не создаются.
        public void Advance(float dt)
        {
            for (int i = 0; i < used; i++)
            {
                float age = ages[i] + dt;
                while (age >= CycleSec) age -= CycleSec;
                ages[i] = age;
            }
        }

        public void Clear() { used = 0; }

        private int IndexOf(EntityId id)
        {
            for (int i = 0; i < used; i++) if (ids[i].Equals(id)) return i;
            return -1;
        }
    }
}
