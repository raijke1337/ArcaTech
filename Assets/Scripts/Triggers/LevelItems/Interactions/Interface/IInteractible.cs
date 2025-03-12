using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Triggers
{
    public interface IInteractible : ITargetable
    {
        void AcceptInteraction(IInteractible actor);
        Vector3 Position { get; }
    }



}