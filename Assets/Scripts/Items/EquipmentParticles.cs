using System;
using System.Collections.Generic;
using Arcatech.Effects;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Arcatech.EventBus;
using CartoonFX;

namespace Arcatech.Items
{
    public class EquipmentParticles : MonoBehaviour, IEquipmentPart
    {
        [SerializeField] private SerializedDictionary<StateMachineNotifyType, ParticleSystem[]> effects;
        private StateMachineNotifyType current;
        public void TriggerState(StateMachineNotifyType notification)
        {

            if (effects.TryGetValue(current, out var ef))
            {
                foreach (var e in ef)
                {
                    e.Stop();
                    SetCfxrActive(e, false);
                }
            }
            if (effects.TryGetValue(notification, out var ne))
            {
                foreach (var e in ne)
                {
                    SetCfxrActive(e, true);
                    e.Play();
                }
            }
            
            current = notification;
        }

        private ParticleSystem[] _allParticles;
        private CFXR_Effect[] _cfxrEffects;

        private void Awake()
        {
            // Частицы запускаются только по уведомлениям состояния. Play On Awake срабатывал бы при каждом
            // повторном включении предмета (перерисовка оружия в EntityInventoryDrawerComponent.DrawItems).
            _allParticles = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particle in _allParticles)
            {
                var main = particle.main;
                main.playOnAwake = false;
            }

            _cfxrEffects = GetComponentsInChildren<CFXR_Effect>(true);
            DeactivateAll();
        }

        private void OnEnable()
        {
            current = StateMachineNotifyType.NoNotify;
            DeactivateAll();
        }

        private void OnDisable()
        {
            // CFXR_Effect.OnEnable включает свет при каждой активации объекта, поэтому выключаем
            // компонент заранее: пока он выключен, его OnEnable не вызывается
            DeactivateAll();
        }

        private void DeactivateAll()
        {
            // Clear обязателен: Stop() без него лишь прекращает эмиссию, уже выпущенный взрыв частиц остаётся видимым
            if (_allParticles != null)
            {
                foreach (var particle in _allParticles)
                {
                    if (particle != null) particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            if (_cfxrEffects != null)
            {
                foreach (var fx in _cfxrEffects) SetCfxrActive(fx, false);
            }
        }

        /// <summary>
        /// CFXR_Effect анимирует Point Light от собственного таймера и включает свет в OnEnable.
        /// Поэтому компонент и свет держим выключенными и включаем только на время проигрывания эффекта.
        /// </summary>
        private void SetCfxrActive(ParticleSystem system, bool active)
        {
            if (system == null) return;
            SetCfxrActive(system.GetComponent<CFXR_Effect>(), active);
        }

        private static void SetCfxrActive(CFXR_Effect fx, bool active)
        {
            if (fx == null) return;

            // выключение компонента вызывает OnDisable -> ResetState (сброс таймера и значений света)
            fx.enabled = active;

            if (fx.animatedLights == null) return;
            foreach (var animated in fx.animatedLights)
            {
                if (animated?.light != null) animated.light.enabled = active;
            }
        }

        public void OnEquip()
        {
        }

        public void OnRemove()
        {
        }
    }
}