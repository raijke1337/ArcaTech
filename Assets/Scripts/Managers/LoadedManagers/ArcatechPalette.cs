using UnityEngine;

namespace Arcatech.Managers
{
    public class ArcatechPalette
    {
        // Базовый фон — затемнение сцены, крупные фоновые области
        public readonly Color BaseBackground = HexToColor("#10131D");

        // Поверхность панели — карточки, меню, контейнеры
        public readonly Color PanelSurface = HexToColor("#1B2230");

        // Вторичная поверхность — внутренние блоки, неактивные ячейки
        public readonly Color SecondarySurface = HexToColor("#273044");

        // Светлый текст — основной текст, активные подписи
        public readonly Color TextPrimary = HexToColor("#EAF4FF");

        // Вторичный текст — описания, служебные данные
        public readonly Color TextSecondary = HexToColor("#91A0B8");

        // Циан ARCA — интерактивные элементы, энергия, выбор
        public readonly Color ArcaCyan = HexToColor("#29D7FF");

        // Синий техно — вторичные активные состояния, навигация
        public readonly Color TechnoBlue = HexToColor("#447BFF");

        // Янтарный кристалл — редкие предметы, секреты, исследование
        public readonly Color CrystalAmber = HexToColor("#FFB84A");

        // Розово-магентовый резонанс — перегрузка, риск, взрослый контент, особые состояния
        public readonly Color ResonanceMagenta = HexToColor("#FF4FA3");

        // Красный тревоги — урон, поражение, критические предупреждения
        public readonly Color AlertRed = HexToColor("#FF5268");

        // Зеленый подтверждения — успешное действие, доступно, завершено
        public readonly Color ConfirmGreen = HexToColor("#5EE6A8");

        // Заблокированный — недоступные вкладки, закрытые награды
        public readonly Color Locked = HexToColor("#566174");

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            Debug.LogError($"[ArcatechPalette] Не удалось распарсить цвет: {hex}");
            return Color.magenta;
        }
    }
}