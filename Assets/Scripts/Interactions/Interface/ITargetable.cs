using Arcatech.Texts;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    public interface ITargetable: IPointerEnterHandler, IPointerExitHandler
    {
        public Description GetInfo { get; }
    }
}