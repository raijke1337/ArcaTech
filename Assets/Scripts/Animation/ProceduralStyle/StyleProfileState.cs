using UnityEngine;

namespace Arcatech.Animation
{
    /// <summary>
    /// Вешается на состояние или под-машину состояний в Animator Controller.
    /// Пока персонаж в этом состоянии, процедурный слой плавно переходит на указанный профиль.
    /// Примеры: «Combat» (всё по нулям), «Rifle» (анти-клиппинг рук = 0, руки держат оружие),
    /// «Showcase» (клипы уже женственные: покачивание 0, анти-клиппинг 1).
    /// </summary>
    public class StyleProfileState : StateMachineBehaviour
    {
        public StyleProfile profile;
        [Min(0f)] public float blendTime = 0.2f;
        [Tooltip("Если у текущей стойки оружия есть свой профиль (StanceStyleBinder) — использовать его вместо этого")]
        public bool useStanceProfile = true;

        ProceduralStyleController controller;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!controller) controller = animator.GetComponent<ProceduralStyleController>();
            if (controller) controller.PushProfile(this, profile, blendTime, useStanceProfile);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (controller) controller.RemoveProfile(this);
        }
    }
}
