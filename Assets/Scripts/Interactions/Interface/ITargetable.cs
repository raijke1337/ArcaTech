using Arcatech.Stats;
using System.Collections.Generic;
using Arcatech;
using Arcatech.Items;
using Arcatech.Texts;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    public interface ITargetable : IPointerEnterHandler, IPointerExitHandler
    {
        public Side Side { get; }
        public string TargetName { get; }
    }
}