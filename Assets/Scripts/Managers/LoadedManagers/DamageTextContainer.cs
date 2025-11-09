using CartoonFX;
//using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Managers
{

    public class DamageTextContainer : ValidatedMonoBehaviour
    {
        public int Number { get; private set; }


        public void PlayNumbers(float value) => PlayNumbers(Mathf.RoundToInt(value));
        public void PlayNumbers(int number)
        {
            if (number == 0) return;
            Number = number;

            int damage = Mathf.Abs(number);
            string text = damage.ToString();
            float intensity = damage / 50f;
            float size = Mathf.Lerp(0.3f, 0.5f, intensity);
            Color color1 = Color.Lerp(Color.red, Color.yellow, intensity);
            dynamicParticleText.UpdateText(text, size, color1);
            //particles.Play(true);
        }

        [SerializeField, Self] ParticleSystem particles;
        [SerializeField, Self] CFXR_ParticleText dynamicParticleText;

    }


}