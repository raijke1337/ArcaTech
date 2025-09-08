using Arcatech.Stats;
using System.Collections.Generic;
using Arcatech;
using UnityEngine.EventSystems;

public interface ITargetable : IPointerEnterHandler, IPointerExitHandler
{
    public BaseGameEntityComponent GetEntity { get; }

}
