using Arcatech.Interactions;
using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Эффект взаимодействия «уровень пройден» - вешается на выход с уровня
    /// (дверь, лифт, триггер после босса). Дальше всё делает
    /// GameInterfaceManager.OnLevelCompleted: фиксирует результат в LevelProgressController,
    /// отключает боевой ввод и показывает «Сводку прохождения уровня».</summary>
    public class LevelCompleteEffect : InteractionEffect
    {
        [SerializeField] private InteractionState onState = InteractionState.Success;

        private bool _played;

        public override void Play(InteractionContext ctx)
        {
            if (_played || ctx.State != onState) return;
            _played = true;

            GameInterfaceManager.Instance.OnLevelCompleted();
        }
    }
}
