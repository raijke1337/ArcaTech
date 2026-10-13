using AYellowpaper.SerializedCollections;
using Arcatech.Items;
using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Animation
{
    /// <summary>
    /// Связывает стойку оружия (StanceClipSwitcher) с профилем процедурного слоя.
    /// Стойки без записи в словаре используют профиль состояния (обычно Style_Default).
    /// Вешается на объект с Animator рядом с ProceduralStyleController.
    /// </summary>
    public class StanceStyleBinder : MonoBehaviour
    {
        [SerializeField] private StanceClipSwitcher switcher;
        [SerializeField] private ProceduralStyleController style;
        [SerializeField] private SerializedDictionary<WeaponStanceSO, StyleProfile> profiles;

        private WeaponStanceSO _last;
        private bool _applied;

        private void Awake()
        {
            if (!switcher) switcher = GetComponentInParent<StanceClipSwitcher>();
            if (!style) style = GetComponent<ProceduralStyleController>();
        }

        private void Update()
        {
            if (!switcher || !style) return;
            WeaponStanceSO stance = switcher.CurrentStance;
            if (_applied && stance == _last) return;

            _applied = true;
            _last = stance;
            style.StanceProfile = stance != null && profiles != null && profiles.TryGetValue(stance, out StyleProfile p) ? p : null;
        }
    }
}
