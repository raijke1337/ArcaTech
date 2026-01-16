namespace Arcatech.Stats
{
    public class StatRuntime
    {
        public float baseMax;
        public float current;
        public float max;
        public float minClamp;
        public float maxClamp;

        // Computed each recalc (not permanently accumulated)
        public float equipAddMax;
        public float equipMultMax;
        public float effectAddMax;
        public float effectMultMax;
    }
}