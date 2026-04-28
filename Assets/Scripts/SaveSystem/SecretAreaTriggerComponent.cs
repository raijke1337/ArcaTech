using System;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SecretAreaTriggerComponent : MonoBehaviour, ISavedProgressItem
    {
        /// example implementation class
        [SerializeField] private SerializableGuid _guid;
        
        
        public string SavedItemID => _guid.ToString();
        private bool _state;

        public bool SavedItemState
        {
            get => _state;
            set => OnSetState(value);
        }

        private void OnEnable()
        {
            Debug.Log($"Enabling ISavedProgressItem {gameObject.name}: ID {SavedItemID}");
        }

        private void OnDisable()
        {
            Debug.Log($"Disabling ISavedProgressItem {gameObject.name}: ID {SavedItemID}");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SavedItemState = true;
            }
        }

        public event SimpleEventsHandler<ISavedProgressItem> UpdateEvent;

        public int CompareTo(ISavedProgressItem other)
        {
            return SavedItemID.CompareTo(other.SavedItemID);
            // TODO? sort by GUID
        }

        void OnSetState(bool state)
        {
            _state = state;
            UpdateEvent?.Invoke(this);
            if (state) gameObject.SetActive(false);
        }
    }
}