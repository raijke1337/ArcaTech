namespace Arcatech.Stats
{
    public interface IDamageDrawer
    {
        public void DrawResourceChange(float amount, bool isDamage, 
            float? durationOverride, ResourceStatType type = ResourceStatType.Health);
    }
}