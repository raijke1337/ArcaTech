using KBCore.Refs;
using UnityEngine;
namespace Arcatech.UI
{
    public abstract class PanelWithBarGeneric : ValidatedMonoBehaviour
    {
        [SerializeField, Child] protected BarsContainersManager _bars;
        //public void ShowBar(BaseStatType type, StatValueContainer cont) => _bars.UpdateBarValue(type, cont);
        //public void ClearBar(BaseStatType type) => _bars.RemoveBar(type);
        //public void ClearAllBars() => _bars.ClearAllBars();
    }
}