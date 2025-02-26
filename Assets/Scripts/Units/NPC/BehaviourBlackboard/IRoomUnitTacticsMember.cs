
using System.Collections;
using UnityEngine;

namespace Arcatech.AI
{
    /// <summary>
    /// expert is a class that makes a decision regarding a certain aspect of behavior
    /// </summary>
    public interface IRoomUnitTacticsMember 
    {
        bool UnitInCombatState { get; set; }
        void SetUnitsGroup(RoomUnitsGroup g);
    }


}