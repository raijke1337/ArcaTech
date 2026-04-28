using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SecretAreaTriggerComponent : MonoBehaviour, ISavedProgressItem
    {
        /// example implementation class
        [SerializeField] private SerializableGuid _guid;
        public string ItemID => _guid.ToString();
        private bool _state;

        public bool Completed
        {
            get => _state;
            set => OnSetState(value);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Completed = true;
            }
        }

        public event SimpleEventsHandler<ISavedProgressItem> UpdateEvent;

        public int CompareTo(ISavedProgressItem other)
        {
            return ItemID.CompareTo(other.ItemID);
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