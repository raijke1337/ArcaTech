using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ProjectileSkill : BaseSkill, IProjectile
{
    
    public event SimpleEventsHandler<IProjectile> ExpiryEventProjectile;

    public void SetProjectileData(ProjectileDataConfig data) => ProjData = new ProjectileData(data);
    private ProjectileData ProjData;

    private float _exp;
    private int _penetr;
    public string GetID { get => SkillID;  }

    BaseUnit IProjectile.Source => Source;

    public void OnExpiryProj()
    {
        Destroy(gameObject);
    }

    public void OnSpawnProj()
    {
        transform.position += transform.forward;
        _exp = ProjData.TimeToLive;
        _penetr = ProjData.Penetration;
    }

    public void OnUpdateProj()
    {
        transform.position += ProjData.Speed * Time.deltaTime * transform.forward;
        _exp -= Time.deltaTime;
        if (_exp <= 0f) ExpiryEventProjectile?.Invoke(this);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<BaseUnit>() == Source) return;
        base.OnCollisionEnter(collision);
        if (_penetr > 0)
        {
            _penetr--;
        }
        if (_penetr == 0) ExpiryEventProjectile?.Invoke(this);

    }

}

