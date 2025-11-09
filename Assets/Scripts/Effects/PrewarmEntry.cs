using CartoonFX;

namespace Arcatech.Effects
{
    [System.Serializable]
    public class PrewarmEntry
    {
        public CFXR_Effect prefab;
        public int initial = 8;
        public int max = 64; // soft cap; manager can expand unless you enforce it
    }
}