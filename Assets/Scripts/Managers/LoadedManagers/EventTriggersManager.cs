using System.Collections.Generic;
using UnityEngine;

public class EventTriggersManager : LoadedManagerBase
{
    [SerializeField] private List<LevelEventTrigger> triggers = new List<LevelEventTrigger>();

    #region managed
    public override void Initiate()
    {
        if (triggers != null) triggers.Clear();
        triggers = new List<LevelEventTrigger>();
    }

    public override void RunUpdate(float delta)
    {

    }

    public override void Stop()
    {
        foreach (var t in triggers) { t.EnterEvent -= OnEventActivated; }
    }
    #endregion

    public void RegisterEventTrigger(LevelEventTrigger tr, bool isReg)
    {
        if (isReg)
        {
            triggers.Add(tr);
            tr.EnterEvent += OnEventActivated;
        }
        else
        {
            triggers.Remove(tr);
            tr.EnterEvent -= OnEventActivated;
        }
    }


    private void OnEventActivated(LevelEventTrigger tr, bool isEnter)
    {
        switch (tr.EventType)
        {
            case LevelEventType.TextDisplay:
                GameManager.Instance.GetGameControllers.GameInterfaceManager.UpdateGameText(tr.ContentIDString, isEnter);
                break;
            case LevelEventType.LevelComplete:
                if (isEnter)  // to prevent double completes
                GameManager.Instance.OnLevelComplete();
                break;
            case LevelEventType.Cutscene:
                break;
            case LevelEventType.ItemPickup:
                GameManager.Instance.OnItemPickup(tr.ContentIDString);
                break;
            default:
                Debug.LogWarning($"{this} can't handle event of type {tr.EventType}");
                break;
        }
    }

}
