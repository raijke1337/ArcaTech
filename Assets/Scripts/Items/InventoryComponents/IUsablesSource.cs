using System.Collections.Generic;

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