using System.Linq;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Queue", order = 2)]
    public class SerializedQueueChargesStrategy : SerializedGenericCooldownStrategy
    {
        [Min(1)] public int maxCharges = 3;

        public override BasicChargesStrategy Deserialize()
        {
            return new ChargesQueueStrategy(this);
        }
    }

    public class ChargesQueueStrategy : BasicChargesStrategy
    {
        // Пересчёт времени для каждого слота; 0 означает, что слот свободен
        private readonly float[] _cooldowns;

        public ChargesQueueStrategy(SerializedQueueChargesStrategy charges) : base(charges)
        {
            MaxCharges = charges.maxCharges;
            CurrentCharges = MaxCharges;

            _cooldowns = new float[MaxCharges];
            for (int i = 0; i < MaxCharges; i++)
            {
                _cooldowns[i] = 0f;
            }
        }

        public override void Tick(float delta)
        {
            base.Tick(delta); // Вызываем базовый тик, если там есть глобальная логика

            // Обновляем внутренние таймеры
            for (int i = 0; i < MaxCharges; i++)
            {
                if (_cooldowns[i] > 0f)
                {
                    _cooldowns[i] -= delta;
                }
            }

            // 💡 ФИКС: Синхронизируем текущее количество зарядов с состоянием массива.
            // UI зависит от свойства CurrentCharges, поэтому мы должны считать актуальное кол-во.
            int freeSlotsCount = 0;
            for (int i = 0; i < MaxCharges; i++)
            {
                if (_cooldowns[i] <= 0f)
                {
                    freeSlotsCount++;
                }
            }

            // Обновляем свойство только если значение изменилось (оптимизация)
            if (CurrentCharges != freeSlotsCount)
            {
                CurrentCharges = freeSlotsCount;
            }
        }

        protected override bool ReadyCheck()
        {
            // Способность готова, если есть ХОТЯ БЫ ОДИН свободный слот
            return _cooldowns.Any(c => c <= 0f);
        }

        public override void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            base.OnChangeUsableState(notifyType);

            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                {
                    // Находим индекс первого свободного слота
                    int readyIndex = -1;
                    for (int i = 0; i < MaxCharges; i++)
                    {
                        if (_cooldowns[i] <= 0f)
                        {
                            readyIndex = i;
                            break;
                        }
                    }

                    // Потребляем заряд
                    if (readyIndex != -1)
                    {
                        _cooldowns[readyIndex] = Cooldown;

                        // Важно: здесь мы декрементируем CurrentCharges. 
                        // Но так как UI читает его напрямую, а не через метод ReadyCheck,
                        // ему критически важно, чтобы он возвращал правильное число при следующем кадре.
                        // (Обновление до правильного значения произойдет на следующем кадре в Tick)
                        CurrentCharges--;
                    }

                    break;
                }
            }
        }

    }
}