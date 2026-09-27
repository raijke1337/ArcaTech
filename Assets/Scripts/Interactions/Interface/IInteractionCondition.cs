using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this interface is for checking if interaction can be performed
    /// </summary>
    public abstract class InteractionCondition : MonoBehaviour
    {
        [SerializeField] private List<InteractionEffect> denyEffects;

        /// <summary>
        /// Запускает проверку условия. Callback ОБЯЗАН быть вызван ровно один раз —
        /// либо синхронно внутри метода, либо позже (например, после завершения QTE).
        /// </summary>
        public abstract void Check(InteractionContext ctx, UnityAction<InteractionState> callback);

        /// <summary>
        /// Вызывается пайплайном, если взаимодействие было отменено, пока проверка этого
        /// условия ещё не вернула результат. Переопределите, если проверка запускает что-то,
        /// что нужно остановить (мини-игру, таймер и т.п.).
        /// </summary>
        public virtual void CancelCheck(InteractionContext ctx) { }

        public void PlayDenyEffects(InteractionContext ctx)
        {
            foreach (var e in denyEffects) e.Play(ctx);
        }
    }
}