﻿using Arcatech.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Managers
{
    [XmlRoot("GameSave"), Serializable]
    public class SerializedSaveData
    {
        public List<string> OpenedLevels;
        public ItemsStringsSave PlayerItems;

        public SerializedSaveData(GameSave save)
        {
            OpenedLevels = new List<string>();
            foreach (var l in save.OpenedLevels)
            {
                OpenedLevels.Add(l.ID);
            }
            PlayerItems = save.Items;
        }


        public SerializedSaveData()
        {
        }
    }

    public class GameSave
    {
        public List<SceneContainer> OpenedLevels;
        public ItemsStringsSave Items;
        public GameSave(List<SceneContainer> lvs, ItemsStringsSave items)
        {
            OpenedLevels = lvs; 
            Items = items;
        }

    }

    [Serializable]
    public class ItemsStringsSave
    {
        public List<string> Equips;
        public List<string> Inventory;
        public ItemsStringsSave()
        {
            Equips = new List<string>();
            Inventory = new List<string>();
        }
    }


    public class SavesSerializer
    {
        private SerializedSaveData _data;
        private string _path;
        public SavesSerializer()
        {
            _path =  Application.dataPath + Constants.Configs.c_SavesPath;
            if ( TryLoadSaveData(out var s ))
            { 
                _data = s;
            }
        }

        public bool TryLoadSerializedSave(out SerializedSaveData save)
        {
            save = _data;
            return _data != null;
        }

        public void UpdateSerializedSave (GameSave save)
        {
            _data = new SerializedSaveData(save);
            SaveDataXML(_data);
        }


        internal void SaveDataXML(SerializedSaveData data)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SerializedSaveData));
            FileStream fs = new FileStream(_path, FileMode.Create);
            ser.Serialize(fs, data);
            fs.Close();
            AssetDatabase.Refresh();
        }
        internal bool TryLoadSaveData(out SerializedSaveData data)
        {

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(SerializedSaveData));
                FileStream fs = new FileStream(_path, FileMode.Open);
                data = (SerializedSaveData)ser.Deserialize(fs);
                fs.Close();

                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                data = null;
                Debug.Log("Save file not found, creating new");
                return false;
            }

        }
    }
}