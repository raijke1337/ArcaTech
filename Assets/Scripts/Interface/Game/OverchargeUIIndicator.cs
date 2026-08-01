using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.UI
{
    public class OverchargeUIIndicator : MonoBehaviour
    {
        private TailsOverchargeModule _tailsOverchargeModule; // the data source for the interface element
        public void SetDataSource(TailsOverchargeModule tailsOverchargeModule)
        {
            _tailsOverchargeModule = tailsOverchargeModule;
        }
    }
}