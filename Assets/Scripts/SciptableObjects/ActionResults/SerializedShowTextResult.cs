using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "actionResult_ShowDialogue_", menuName = "Actions/Action Result/Text display")]
    public class SerializedShowTextResult : SerializedActionResult
    {
        public DialoguePart Texts;
        
        public override ActionResult Deserialize()
        {
            return new ShowTextResult(Texts);
        }
    }

    public class ShowTextResult : ActionResult
    {
        DialoguePart texts;
        public ShowTextResult(DialoguePart txts)
        {
            texts = txts;
        }
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            GameInterfaceManager.Instance.ShowDialoguePart(texts);
            return true;
        }
    }
}