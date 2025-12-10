using UnityEngine;

namespace Arcatech.Usables
{
  //  [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Queue",order = 2)]
    public class SerializedQueueChargesStrategy : SerializedChargesStrategy
    {
        public override BasicChargesStrategy Deserialize()
        {
            return new ChargesQueueStrategy(this);
        }
    }

    public class ChargesQueueStrategy : BasicChargesStrategy
    {
        public ChargesQueueStrategy(SerializedChargesStrategy charges) : base(charges)
        {
        }
    }
}