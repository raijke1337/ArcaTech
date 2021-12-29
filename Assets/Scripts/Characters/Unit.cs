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

public class Unit : MonoBehaviour
{

    private bool _isInAtkAnimation = false;

    private Animator _animator;
    private UnitController _inputs;
    private UnitStats _stats;


    [SerializeField]
    public int EquippedWeaponID;

    public MovementDebugData GetDebugData { get;private set; }   

    [SerializeField]
    private Transform _targetPoint;
    public Transform GetBullshotTarget => _targetPoint;
    public void TakeDamage(int damage) => AnimateDamage(damage);

    public Unit CurrentTarget { get; protected set; }

    private WeaponHitTriggerComponent[] _colliders;
    private Rigidbody _rb;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _inputs = GetComponent<UnitController>();
        _stats = GetComponent<UnitStats>();
        _rb = GetComponent<Rigidbody>();
        Binds();
        _colliders = GetComponentsInChildren<WeaponHitTriggerComponent>();
        foreach (var c in _colliders)
        {
            c.SetUpWeapon(this, GameManager._self.GetWeaponStats(EquippedWeaponID));
        }
        if (_inputs == null) return;

        #region debug
#if UNITY_EDITOR
        GetDebugData = new MovementDebugData();
#endif
        #endregion
    }

    protected virtual void Update()
    {
        if (_isInAtkAnimation) return;
        AnimateAndPerformMovement();
    }

    private void AnimateAttack()
    {
        if (_isInAtkAnimation) return;
        _animator.SetTrigger("2HAttack");
        _isInAtkAnimation = true;
    }
    private void AnimateQSpecial()
    {
        if (_isInAtkAnimation) return;
        _animator.SetTrigger("QSpecial");
        _isInAtkAnimation = true;
    }
    private void AnimateDeath()
    {
        _animator.SetTrigger("Death");
    }
    private void AnimateDamage(int damage)
    {
        _stats.currHP -= damage;
        if (_stats.currHP <= 0) return;
        _animator.SetTrigger("TakeDamage");
    }

    private void AnimateAndPerformMovement()
    {
        ref var movement = ref _inputs.MoveDirection;
        if (movement.x == 0f && movement.z == 0f)
        {
            _animator.SetBool("Moving", false);
            _animator.SetFloat("ForwardMove", 0f);
            _animator.SetFloat("SideMove", 0f);
        }
        else
        {
            _animator.SetBool("Moving", true);
            transform.position += movement * Time.deltaTime * _stats.MS;
            CalcAnimVector(movement);
        }
        #region debug
#if UNITY_EDITOR
        GetDebugData._movement = movement;
        GetDebugData._facing = transform.forward;
        GetDebugData._animVector = CalcAnimVector(movement);
#endif
        #endregion
    }

    //  calculates the vector which is used to set values in animator
    private Vector3 CalcAnimVector(Vector3 vector)
    {
        var playerFwd = transform.forward;
        var playerRght = transform.right;
        var camFwd = vector;
        camFwd.y = 0;
        camFwd.Normalize();
        // Dot product of two vectors determines how much they are pointing in the same direction.
        // If the vectors are normalized (transform.forward and right are)
        // then the value will be between -1 and +1.
        var x = Vector3.Dot(playerRght, camFwd);
        var z = Vector3.Dot(playerFwd, camFwd);

        _animator.SetFloat("ForwardMove", z);
        _animator.SetFloat("SideMove", x);

        return new Vector3(x,0,z);
    }

    private void OnAnimationEnd_UE(AnimationEvent data)
    {
        _isInAtkAnimation = false;
        if (data.stringParameter == "Dead") Destroy(gameObject);
    }

    private void OnCollider_UE(AnimationEvent data)
    {
        var coll = (from c in _colliders where c.ID == EquippedWeaponID select c).First();
        coll.Enable = !coll.Enable;
    }

    private void Binds()
    {
        _inputs.OnMeleeAttack += AnimateAttack;
        _inputs.OnQSpecial += AnimateQSpecial;
        _stats.ZeroHealthEvent += AnimateDeath;
    }

}

