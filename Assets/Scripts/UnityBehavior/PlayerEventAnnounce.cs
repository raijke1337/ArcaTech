using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/PlayerEventAnnounce")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "PlayerEventAnnounce", message: "Player has announced state", category: "Events", id: "8c20d8457bb26257231a9fcdeb7c9373")]
public sealed partial class PlayerEventAnnounce : EventChannel { }

