using UnityEngine;

namespace Arcatech.Actions
{


    [CreateAssetMenu(fileName = "New place base entity", menuName = "Actions/Action Result/Place Base Entity")]
    public class SerializedPlaceEntityResult : SerializedActionResult
    {
        [SerializeField] BaseGameEntityComponent entityToPlace;
        public override ActionResult Deserialize()
        {
            return new PlaceEntityResult(entityToPlace);
        }
    }


    public class PlaceEntityResult : ActionResult
    {
        BaseGameEntityComponent _prefab;
        public PlaceEntityResult(BaseGameEntityComponent prefab)
        {
            _prefab = prefab;
        }
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            
            Object.Instantiate(_prefab, place, placeRot);
            return _prefab;
        }
    }
}