using UnityEngine;

namespace Arcatech
{
    public interface ISpawnerProvider
    {
        public Transform SpawnPoint { get; }
    }
}