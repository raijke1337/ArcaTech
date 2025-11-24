using System.Collections.Generic;
using Arcatech.Usables;

namespace Arcatech.Items
{
    /// <summary>
    /// for inventory components
    /// </summary>
    public interface IUsablesSource
    {
        public IDictionary<UnitActionType,IUsable> GetUsables { get; }
    }

   
}