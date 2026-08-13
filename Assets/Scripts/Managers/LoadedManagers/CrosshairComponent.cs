using UnityEngine;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using Unity.Cinemachine;

namespace Arcatech.UI
{

    public class CrosshairComponent : ValidatedMonoBehaviour
    {

        public BaseGameEntityComponent CurrentTarget { get; set; }
        
    }
}