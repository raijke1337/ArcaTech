using Arcatech.Texts;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.UI
{
    public class UsablePanel : ValidatedMonoBehaviour
    {
        [SerializeField, Child] private IconContainerUIScript icon;
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;

        public void SetInformation(IUsable data)
        {
            icon.AssignIcon(data);
            titleText.text = data.Description.Title;
            descriptionText.text = data.Description.Text;   
        }
    }
}