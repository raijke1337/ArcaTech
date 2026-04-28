using System;
using System.Collections.Generic;
using Arcatech.Triggers;
using CartoonFX;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Level
{
    [RequireComponent(typeof(Collider))]
    public class WaterSplashesDrawerComponent : MonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField] private CFXR_Effect onWaterEnter;
        [SerializeField] private CFXR_Effect onWaterExit;
        [SerializeField] private CFXR_Effect onWaterStay;
        private Collider waterCollisionArea; // potentially can use a triggertracker here
       private readonly Dictionary<BaseGameEntityComponent, CFXR_Effect> _inWater = new();

    private void OnValidate()
    {
        Assert.IsNotNull(onWaterEnter, "onWaterEnter is not assigned!");
        Assert.IsNotNull(onWaterExit, "onWaterExit is not assigned!");
        Assert.IsNotNull(onWaterStay, "onWaterStay is not assigned!");
    }

    private void Awake()
    {
        waterCollisionArea = GetComponent<Collider>();
        
        if (waterCollisionArea == null)
        {
            Debug.LogError("WaterSplashesDrawerComponent requires a Collider on the same GameObject!", this);
            return;
        }

        waterCollisionArea.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseGameEntityComponent component))
            return;

        // Вход в воду
        if (onWaterEnter != null)
        {
            Instantiate(onWaterEnter, component.transform.position, component.transform.rotation);
        }

        // Создаём постоянный эффект (рябь/брызги), который следует за объектом
        if (onWaterStay != null)
        {
            CFXR_Effect ripples = Instantiate(onWaterStay, 
                                              component.transform.position, 
                                              Quaternion.Euler(90, 0, 0));

            ripples.transform.SetParent(component.transform, true);
            _inWater[component] = ripples;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out BaseGameEntityComponent component))
            return;

        if (_inWater.TryGetValue(component, out CFXR_Effect ripples))
        {
            if (ripples != null)
                Destroy(ripples.gameObject);

            _inWater.Remove(component);
        }

        // Эффект выхода из воды
        if (onWaterExit != null)
        {
            Instantiate(onWaterExit, component.transform.position, component.transform.rotation);
        }
    }

        /// <summary>
/// use this in case I need entry normals
/// </summary>
/// <param name="triggerHitInfo"></param>
/// <exception cref="NotImplementedException"></exception>
        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            throw new NotImplementedException();
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            throw new NotImplementedException();
        }
    }
}