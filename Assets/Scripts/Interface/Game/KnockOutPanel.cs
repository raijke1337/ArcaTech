using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;

namespace Arcatech.UI
{
    public class KnockOutPanel : ValidatedMonoBehaviour
    {
        [SerializeField,Self] PanelAnimator panelAnimator;
        [SerializeField] private TextMeshProUGUI text;
        public void Show() => panelAnimator.Show();
        public void Hide() => panelAnimator.Hide();
        public void UpdateText(int lives)
        {
            text.text = $"Rewind:  {lives} left";
        }
    }
}