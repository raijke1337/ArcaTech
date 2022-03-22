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

public class CameraController : MonoBehaviour
{
    private PlayerUnitController _target;
    [SerializeField]
    private float _camMoveSpeed = 3f;


    private void Start()
    {
        _target = transform.parent.GetComponent<PlayerUnitController>();
        transform.parent = null;
    }

    private void LateUpdate()
    {
        var cursorLoc = _target.GetLookTarget().normalized;
        transform.position = Vector3.Lerp(transform.position, _target.transform.position + cursorLoc, Time.deltaTime * _camMoveSpeed);
    }




}

