using System.Collections.Generic;
using Arcatech.Stats;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class OverchargeUIMain : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI text;

        private readonly Dictionary<OverchargeModuleState, string> stateTexts =
            new Dictionary<OverchargeModuleState, string>()
            {
                { OverchargeModuleState.Idle, "IDL" },
                { OverchargeModuleState.Ready, "CAP" },
                { OverchargeModuleState.InSpendWindow, "CHG" },
                { OverchargeModuleState.Activation, "ACT" },
                { OverchargeModuleState.Active, "ENG" } 
            };

        private TailsOverchargeModule overchargeModule;
        
        // Поля для корректного управления анимацией перехода
        private OverchargeModuleState _previousState = OverchargeModuleState.Idle;
        private bool _isHiding = false;

        public void SetDataSource(TailsOverchargeModule mod)
        {
            overchargeModule = mod;
            text.text = "IDL";
            mod.OnUIUpdate += OnUpdate;
            
            // Инициализация
            fill.fillAmount = 0f;
            fill.gameObject.SetActive(false);
            _previousState = OverchargeModuleState.Idle;
            _isHiding = false;
        }

        private void OnDisable()
        {
            if (overchargeModule != null)
                overchargeModule.OnUIUpdate -= OnUpdate;
                
            // Важно: убиваем твины при отключении, чтобы избежать утечек и ошибок
            if (fill != null)
            {
                fill.DOKill();
                fill.gameObject.SetActive(false);
            }
            _isHiding = false;
        }

        private void OnUpdate(OverchargeUISnapshot data)
        {
            text.text = stateTexts[data.CurrentState];
            Debug.Log(data.CurrentState);
            bool wasInSpendWindow = _previousState == OverchargeModuleState.InSpendWindow;
            bool isNowInSpendWindow = data.CurrentState == OverchargeModuleState.InSpendWindow;

            
            
            if (isNowInSpendWindow)
            {
                // 1. СОСТОЯНИЕ: InSpendWindow (Накопление энергии)
                if (_isHiding)
                {
                    // Если новое окно началось раньше, чем старое успело скрыться - прерываем скрытие
                    fill.DOKill();
                    _isHiding = false;
                }

                if (!fill.gameObject.activeSelf)
                {
                    fill.fillAmount = 0f; // Сбрасываем перед показом, чтобы не было визуального скачка
                    fill.gameObject.SetActive(true);
                }
                
                float targetFill = data.RequiredSpentEnergy > 0f 
                    ? data.WindowSpentEnergy / data.RequiredSpentEnergy 
                    : 0f;
                    
                fill.DOKill(); // Останавливаем предыдущую анимацию заполнения, чтобы начать новую от текущего значения
                fill.DOFillAmount(targetFill, 0.5f);
            }
            else if (wasInSpendWindow && !_isHiding)
            {
                // 2. ПЕРЕХОД: Только что вышли из InSpendWindow -> Запускаем плавный сброс
                _isHiding = true;
                fill.DOKill(); 
                fill.DOFillAmount(0f, 0.5f).OnComplete(() => 
                {
                    // Скрываем объект только после завершения анимации
                    fill.gameObject.SetActive(false);
                    _isHiding = false;
                });
            }
            else 
            {
                // 3. СОСТОЯНИЕ: Idle, Ready, Activation или Active
                // Если fill активен, но мы не в процессе анимации скрытия (например, при инициализации) - выключаем мгновенно
                if (fill.gameObject.activeSelf && !_isHiding)
                {
                    fill.DOKill();
                    fill.fillAmount = 0f;
                    fill.gameObject.SetActive(false);
                }
            }

            _previousState = data.CurrentState;
        }
    }
}