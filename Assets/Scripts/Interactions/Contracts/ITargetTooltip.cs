using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// Объект, у которого есть всплывающая подсказка.
    /// Наведение определяет TargetTooltipController (свой рейкаст), а не EventSystem,
    /// поэтому IPointerEnterHandler/IPointerExitHandler здесь больше не нужны.
    /// </summary>
    public interface ITargetTooltip
    {
        Description GetInfo { get; }

        /// <summary>Заголовок подсказки.</summary>
        string TooltipTitle => GetInfo != null ? GetInfo.Title : string.Empty;

        /// <summary>Текст подсказки. Может содержать токены действий: {Interact}, {Jump}, {Minigame/ButtonN}.</summary>
        string TooltipBody => GetInfo != null ? GetInfo.Text : string.Empty;

        /// <summary>Можно ли сейчас показывать подсказку (объект доступен, не занят и т.п.).</summary>
        bool CanShowTooltip => true;

        /// <summary>Точка в мире, над которой висит подсказка в режиме геймпада.</summary>
        Vector3 TooltipAnchor => this is Component c ? c.transform.position : Vector3.zero;
    }
}
