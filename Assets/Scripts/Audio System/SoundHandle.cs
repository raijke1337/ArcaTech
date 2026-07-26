namespace Arcatech.Audio
{
    public readonly struct SoundHandle
    {
        public readonly int Id;
        public readonly SoundEmitter Emitter;
        public SoundHandle(int id, SoundEmitter emitter) { Id = id; Emitter = emitter; }
        public bool IsValid => Emitter != null && Emitter.CurrentId == Id;
    }
}