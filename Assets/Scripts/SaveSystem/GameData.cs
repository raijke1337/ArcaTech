using System;
using System.Collections.Generic;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class GameData
    {
        public int version;
        public string timestamp;
        public List<LevelProgressData> levelRecords = new();
    }
}