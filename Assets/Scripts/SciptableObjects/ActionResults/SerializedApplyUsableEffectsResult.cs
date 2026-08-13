using System.Collections.Generic;
using System.Linq;
using Arcatech.Usables.Effects;
using ArcaTech.Usables.Effects;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "usable_package_effects_", menuName = "Usables/Apply Usable Effects (Action Result)")]
    public class SerializedApplyUsableEffectsResult : SerializedActionResult
    {
        [SerializeField,FormerlySerializedAs("StatChanges")] SerializedDictionary<TargetingType, BaseAppliedEffect[]> effects;
        public override ActionResult Deserialize()
        {
            return new ApplyUsableEffectsResult(effects);
        }

        private void OnValidate()
        {
            var firstKey = effects.Keys.FirstOrDefault();
        }

        public override string ToString()
        {
            return $"apply effects result total {effects.Count}";
        }
    }

        /// <summary>
    /// Applies ANY BaseAppliedEffect kind via the EffectFactory + EntityEffectController.
    /// Renamed from ApplyStatChangeEffectResult — it is no longer stat-specific.
    /// </summary>
    public class ApplyUsableEffectsResult : ActionResult
    {
        private readonly Dictionary<TargetingType, BaseAppliedEffect[]> _effs;
        private readonly ITargetSelector _selector;
        private readonly EffectFactory _factory;

        public ApplyUsableEffectsResult(
            SerializedDictionary<TargetingType, BaseAppliedEffect[]> cfg,
            ITargetSelector selector = null,
            EffectFactory factory = null)
        {
            _effs = cfg;
            _selector = selector ?? new EffectTargetSelector();
            _factory = factory ?? new EffectFactory();
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target,
            Vector3 place, Quaternion placeRot)
        {
            bool any = false;
            foreach (var type in _effs.Keys)
            {
                if (!_selector.TryPickTarget(type, user, target, out var final)) continue;

                // "Can effects be applied here?" == "does it have an EffectsReceiverComponent?"
                if (!final.TryGetComponent<EffectsReceiverComponent>(out var receiver))
                {
                    Debug.Log($"[Eff] {final.name} has no EffectsReceiverComponent — not a valid effect target.");
                    continue;
                }

                var defs = _effs[type];
                if (defs == null) continue;
                foreach (var def in defs)
                {
                    if (def == null) continue;
                    var instance = _factory.Create(def, user);
                    receiver.Controller.AddEffect(instance, user, receiver, place, placeRot);
                    any = true;
                }
            }
            return any;
        }
    }
}

