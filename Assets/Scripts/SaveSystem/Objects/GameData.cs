using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class GameData
    {
        public int version;
        public string timestamp;
        public GameSettingsData settings = new();
        public MetaProgressData metaProgress = new();
    }
}