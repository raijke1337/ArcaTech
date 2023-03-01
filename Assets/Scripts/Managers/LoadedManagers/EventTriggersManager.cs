using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggersManager : LoadedManagerBase
{
    [SerializeField] private List<LevelEventTrigger> triggers = new List<LevelEventTrigger>();
    private GameTextComp _text;


    public override void Initiate()
    {
        triggers.AddRange(FindObjectsOfType<LevelEventTrigger>());
        foreach (var trigger in triggers)
        { trigger.EnterEvent += OnEventActivated; }
        _text = FindObjectOfType<GameTextComp>();
        if (_text == null) return;
        _text.IsShown = false;
    }

    public override void RunUpdate(float delta)
    {

    }

    public override void Stop()
    {
        foreach (var t in triggers) { t.EnterEvent -= OnEventActivated; }
    }

    private void OnEventActivated(LevelEventTrigger tr, bool isEnter)
    {

        switch (tr.EventType)
        {
            case LevelEventType.TextDisplay:
                if (isEnter)
                {
                    _text.IsShown  = true;
                    _text.SetText(TextsManager.Instance.GetContainerByID(tr.ContentIDString));
                }
                else
                {
                    _text.IsShown = false;
                }
                break;
            case LevelEventType.LevelComplete:
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