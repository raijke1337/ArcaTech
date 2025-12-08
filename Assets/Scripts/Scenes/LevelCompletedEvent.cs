using Arcatech.EventBus;
using Arcatech.Scenes;

namespace Arcatech.Items
{
    public struct LevelCompletedEvent : IEvent
    {
        public SceneContainer CompletedLevel;

        public LevelCompletedEvent(SceneContainer completedLevel)
        {
            CompletedLevel = completedLevel;
        }
    }
}