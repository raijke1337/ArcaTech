using System;
using System.Collections.Generic;
using Arcatech.Stats;
using DG.Tweening;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator_Free))]
    public class OverchargeUIMain : ValidatedMonoBehaviour
    {
        [SerializeField,Self] private PanelAnimator_Free panelAnimator;
        [SerializeField] private Image fill;
        [SerializeField] private Image activatingFrame;
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

        private const int Segments = 12;

        public PanelAnimator_Free Animator => panelAnimator;
        private TailsOverchargeModule overchargeModule;

        private OverchargeModuleState _previousState = OverchargeModuleState.Idle;
        private bool _isHiding = false;

        public void SetDataSource(TailsOverchargeModule mod)
        {
            overchargeModule = mod;
            text.text = "IDL";
            mod.OnUIUpdate += OnUpdate;

            fill.fillAmount = 0f;
            fill.gameObject.SetActive(false);
            _previousState = OverchargeModuleState.Idle;
            _isHiding = false;
        }

        private void OnDisable()
        {
            if (overchargeModule != null)
                overchargeModule.OnUIUpdate -= OnUpdate;

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

            bool wasInSpendWindow = _previousState == OverchargeModuleState.InSpendWindow;
            bool wasActive = _previousState == OverchargeModuleState.Active;

            switch (data.CurrentState)
            {
                case OverchargeModuleState.InSpendWindow:
                    ShowSpendProgress(data);
                    break;

                case OverchargeModuleState.Ready:
                case OverchargeModuleState.Activation:
                    // Если только что вышли из InSpendWindow, шкала должна остаться видимой и заполненной на 100%
                    if (wasInSpendWindow)
                    {
                        if (_isHiding)
                        {
                            fill.DOKill();
                            _isHiding = false;
                        }

                        fill.gameObject.SetActive(true);
                        fill.DOKill();
                        fill.fillAmount = 1f; // Мгновенно заполняем на 100%
                    }
                    // Если не в цепочке InSpendWindow -> Ready/Activation, шкала остаётся скрытой
                    break;

                case OverchargeModuleState.Active:
                    // Countdown запускаем ОДИН раз — в момент входа в состояние
                    if (!wasActive)
                        StartOverchargeCountdown(data);
                    break;

                case OverchargeModuleState.Idle:
                default:
                    // Только что вышли из InSpendWindow или Active -> плавно гасим
                    if ((wasInSpendWindow || wasActive) && !_isHiding)
                    {
                        HideFillSmooth();
                    }
                    else if (fill.gameObject.activeSelf && !_isHiding)
                    {
                        fill.DOKill();
                        fill.fillAmount = 0f;
                        fill.gameObject.SetActive(false);
                    }
                    break;
            }

            activatingFrame.gameObject.SetActive(data.CurrentState == OverchargeModuleState.Activation);

            _previousState = data.CurrentState;
        }

        private void ShowSpendProgress(OverchargeUISnapshot data)
        {
            if (_isHiding)
            {
                fill.DOKill();
                _isHiding = false;
            }

            if (!fill.gameObject.activeSelf)
            {
                fill.fillAmount = 0f;
                fill.gameObject.SetActive(true);
            }

            float targetFill = data.RequiredSpentEnergy > 0f
                ? data.WindowSpentEnergy / data.RequiredSpentEnergy
                : 0f;

            float quantized = Mathf.Ceil(targetFill * Segments) / Segments;

            fill.DOKill();
            fill.DOFillAmount(quantized, 0.5f);
        }

        private void StartOverchargeCountdown(OverchargeUISnapshot data)
        {
            if (_isHiding)
            {
                fill.DOKill();
                _isHiding = false;
            }

            fill.gameObject.SetActive(true);
            fill.DOKill();
            fill.fillAmount = 1f;

            float duration = Mathf.Max(data.OverchargeDuration, 0.01f);
            fill.DOFillAmount(0f, duration).SetEase(Ease.Linear);
        }

        private void HideFillSmooth()
        {
            _isHiding = true;
            fill.DOKill();
            fill.DOFillAmount(0f, 0.5f).OnComplete(() =>
            {
                fill.gameObject.SetActive(false);
                _isHiding = false;
            });
        }
    }
}