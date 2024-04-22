﻿using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using UnityEngine;

namespace Arcatech.Skills
{
    #region skill configs

    [Serializable]
    public class SkillObjectForControls
    {
        // skill controller data
        public BaseUnit Owner { get; }
        public float CurrentCooldown { get; protected set; }

        // end
        public SkillComponent GetInstantiatedSkillCollider
        {
            get
            {
                var item = GameObject.Instantiate(_settings.SkillObject);
                item.Data = _settings;
                if (item is BoosterSkillInstanceComponent bb)
                {
                    bb.Data = _settings as DodgeSkillConfigurationDictionarySO;
                }

                item.Owner = Owner;

                return item;
            }
        }

        // from stored cfg
        private SkillControlSettingsSO _settings;

        public TextContainerSO Description { get => _settings.Description; }
        public BaseStatTriggerConfig[] Triggers { get => _settings.Triggers; }
        public EffectsCollection Effects { get => _settings.Effects; }
        public int Cost { get => _settings.Cost; }
        public float PlacerRadius { get => _settings.PlacerRadius; }
        public float EffectRadius { get => _settings.AoERadius; }
        public ProjectileConfiguration GetProjectileData
        { get; private set; }

        // end

        public SkillObjectForControls(SkillControlSettingsSO cfg, BaseUnit owner)
        {
            Owner = owner;

            if (cfg is DodgeSkillConfigurationDictionarySO dcfg)
            {
                foreach (var v in dcfg.DodgeSkillStats.Values)
                {
                    v.Setup();
                }
                _settings = dcfg;
            }
            else
            {
                _settings = cfg;
            }
            
            CurrentCooldown = 0;

            if (cfg is ProjectileSkillConfigurationPackSO projectile)
            {
                GetProjectileData = projectile.SkillProjectile;
                Debug.Log($"Projectile skill data set for {cfg.Description.Title}");
            }
        }

        public virtual bool TryUse()
        {
            bool ok = CurrentCooldown <= 0;
            if (ok)
            {
                CurrentCooldown = _settings.Cooldown;
            }

            return ok;
        }
        public virtual void UpdateInDelta(float time)
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown -= time;
            }
        }
    }


    #endregion


}
