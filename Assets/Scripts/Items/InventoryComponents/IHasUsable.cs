using System.Collections.Generic;

namespace Arcatech.Items
{
    /// <summary>
    /// for inventory components
    /// </summary>
    public interface IHasUsable
    {
        public List<IUsable> GetUsables { get; }
    }

   
}