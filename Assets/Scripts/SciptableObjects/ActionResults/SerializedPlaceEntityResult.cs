using System;
using UnityEngine;

namespace Arcatech.Actions
{


    [CreateAssetMenu(fileName = "New place base entity", menuName = "Actions/Action Result/Place Base Entity")]
    public class SerializedPlaceEntityResult : SerializedActionResult
    {
        [SerializeField] BaseGameEntityComponent entityToPlace;
        public override ActionResult BuildActionResult()
        {
            return new PlaceEntityResult(entityToPlace);
        }
    }


    public class PlaceEntityResult : ActionResult
    {
        BaseGameEntityComponent EntityToPlace;
        public PlaceEntityResult(BaseGameEntityComponent entityToPlace)
        {
            EntityToPlace = entityToPlace;
        }
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            GameObject.Instantiate(EntityToPlace, place.position, place.rotation);
            return EntityToPlace;
        }
    }
}