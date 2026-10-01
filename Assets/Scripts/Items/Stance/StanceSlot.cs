namespace Arcatech.Items
{
    /// <summary>
    /// Роль клипа в базовом аниматоре, который подменяется под оружие в руках.
    /// Какой именно клип стоит в слоте у безоружной Теилс, задаёт StanceSlotMap.
    /// Числа заданы явно: на них ссылаются сериализованные ассеты, порядок можно менять свободно.
    /// </summary>
    public enum StanceSlot
    {
        Idle = 0,

        RunForward = 10,
        RunBackward = 11,
        RunLeft = 12,
        RunRight = 13,

        // Заготовка на будущее. LocomotionStart и LocomotionStop в контроллере используют один
        // общий набор из четырёх клипов, поэтому слоты общие для обоих стейтов.
        StartStopForward = 20,
        StartStopBackward = 21,
        StartStopLeft = 22,
        StartStopRight = 23,

        DamageInterrupt =30
    }

    /// <summary>
    /// Группы слотов, которые имеет смысл заполнять целиком.
    /// </summary>
    public static class StanceSlotGroups
    {
        public static readonly StanceSlot[] Run =
        {
            StanceSlot.RunForward,
            StanceSlot.RunBackward,
            StanceSlot.RunLeft,
            StanceSlot.RunRight,
        };

        public static readonly StanceSlot[] StartStop =
        {
            StanceSlot.StartStopForward,
            StanceSlot.StartStopBackward,
            StanceSlot.StartStopLeft,
            StanceSlot.StartStopRight,
        };

        public static readonly StanceSlot[] DamageInterrupt =
        {
            StanceSlot.DamageInterrupt
        };
    }
}
