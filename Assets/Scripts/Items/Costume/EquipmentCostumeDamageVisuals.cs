using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>
    /// Часть префаба костюма: отключает назначенные куски брони (GameObject) в зависимости от стадии повреждения.
    /// Списки КУМУЛЯТИВНЫЕ: на стадии "Разрушенный" скрыты куски из списков "Повреждённый" и "Разрушенный".
    /// Подписывается на SuitDamageComponent владельца при экипировке, отписывается и возвращает куски при снятии.
    /// </summary>
    public class EquipmentCostumeDamageVisuals : MonoBehaviour, IEquipmentPart
    {
        [Tooltip("Скрываются на стадии «Повреждённый» и «Разрушенный»")]
        [SerializeField] private GameObject[] hiddenWhenDamaged;

        [Tooltip("Дополнительно скрываются на стадии «Разрушенный»")]
        [SerializeField] private GameObject[] hiddenWhenDestroyed;

        private SuitDamageComponent _suit;
        private bool _equipped;

        public void OnEquip()
        {
            if (_equipped) return;

            // DisplayItem на момент экипировки неактивен, поэтому includeInactive = true
            _suit = GetComponentInParent<SuitDamageComponent>(true);
            if (_suit == null)
            {
                Debug.LogWarning($"{nameof(EquipmentCostumeDamageVisuals)}: у владельца нет SuitDamageComponent", this);
                return;
            }

            _suit.StageChanged += OnStageChanged;
            _equipped = true;
            Apply(_suit.Stage);
        }

        public void OnRemove()
        {
            if (!_equipped) return;
            Unsubscribe();
            Apply(SuitDamageStage.Intact); // снятый костюм не должен остаться «рваным» в инвентаре
            _equipped = false;
        }

        public void TriggerState(StateMachineNotifyType notification)
        { }

        private void OnDestroy() => Unsubscribe();

        private void Unsubscribe()
        {
            if (_suit != null) _suit.StageChanged -= OnStageChanged;
            _suit = null;
        }

        private void OnStageChanged(SuitDamageStage previous, SuitDamageStage current) => Apply(current);

        private void Apply(SuitDamageStage stage)
        {
            // сначала возвращаем всё, потом скрываем по стадии — ремонт работает без отдельной логики
            SetActive(hiddenWhenDamaged, true);
            SetActive(hiddenWhenDestroyed, true);

            if (stage >= SuitDamageStage.Damaged) SetActive(hiddenWhenDamaged, false);
            if (stage >= SuitDamageStage.Destroyed) SetActive(hiddenWhenDestroyed, false);
        }

        private static void SetActive(GameObject[] objects, bool value)
        {
            if (objects == null) return;
            foreach (var go in objects)
                if (go != null) go.SetActive(value);
        }
    }
}
