using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{

    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class DamageDrawerComponent : ValidatedMonoBehaviour, IDamageDrawer
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;

        public void DrawResourceChange(float amount, bool isDamage, float? durationOverride, 
            ResourceStatType type)
        {
            if (amount <= 0f || entity == null || entity.EffectSpawn == null || type != ResourceStatType.Health ) return;

            if (GameInterfaceManager.Instance == null)
            {
                Debug.LogWarning("DamageDrawerComponent: GameInterfaceManager.Instance not available.");
                return;
            }

            GameInterfaceManager.Instance.ShowFloatingNumber(
                amount,
                entity.EffectSpawn.position,
                isDamage,
                durationOverride
            );
        }
    }
}