﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration;
    public float maxSpeed;
    public float jumpForce;
    public LayerMask groundMask;

    [Header("Force")]
    public float shootForce;
    public float grappleForce;
    public float pullForce;

    /////////////////////////////////////////

    private float stringForce;
    private bool charging;
    private bool grounded;

    /////////////////////////////////////////

    public Arrow _arrow;
    public Bow _bow;
    public Rope _rope;

    private Transform _trf;
    private Rigidbody2D _rgbd;
    private DistanceJoint2D _joint;

    /////////////////////////////////////////

    [Header("DEBUG")]
    public float maxVelocity;
    private bool __isLoaded;
    public bool isLoaded
    {
        get
        {
            return __isLoaded;
        }
        set
        {
            __isLoaded = value;
            _rope.active = !value;
        }
    }

    private void Start()
    {
        _trf = transform;
        _rgbd = GetComponent<Rigidbody2D>();
        _joint = GetComponent<DistanceJoint2D>();
        isLoaded = true;
    }

    private void Update()
    {
        UpdateArrow();
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(_trf.position + (Vector3.down * 1f), .5f, groundMask);
        Debug.DrawLine(_trf.position, _trf.position + (Vector3.down * 1.5f), (grounded ? Color.cyan : Color.magenta));

        LimitVelocity();
    }

    /////////////////////////////////////////

    private void LimitVelocity()
    {
        Vector2 vel = _rgbd.velocity;
        float curMaxVelocity = (charging ? maxVelocity * .25f : maxVelocity);

        if (vel.magnitude > curMaxVelocity)
            _rgbd.velocity = vel.normalized * curMaxVelocity;
    }

    /////////////////////////////////////////

    public void Move(float direction)
    {
        if (charging)
            direction = 0f;

        _rgbd.AddForce(Vector2.right * direction * acceleration * Time.deltaTime);

        Vector2 vel = _rgbd.velocity;
        if (Mathf.Abs(vel.x) > maxSpeed)
            vel.x = (vel.x / Mathf.Abs(vel.x)) * maxSpeed;

        _rgbd.velocity = vel;
    }

    public void Jump()
    {
        if (!grounded)
            return;

        Vector2 vel = _rgbd.velocity;
        vel.y = 0f;
        _rgbd.velocity = vel;

        _rgbd.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void ChargeShot()
    {
        if (!isLoaded)
            return;

        charging = true;

        stringForce = Mathf.Clamp(stringForce + Time.deltaTime, 0f, 1f);
        _bow.UpdateString(stringForce);
    }

    public void Shoot(Vector2 axis)
    {
        if (!isLoaded)
            return;

        charging = false;

        isLoaded = false;
        _arrow.Deploy(axis * (shootForce * stringForce));

        stringForce = 0f;
        _bow.UpdateString(stringForce);
    }

    public void Pull()
    {
        if (isLoaded)
            return;

        if (!_arrow.deployed)
        {
            _rgbd.velocity += (_rope.GetFirstPoint() - (Vector2)_trf.position).normalized * grappleForce * Time.deltaTime;
        }
        else
        {
            _arrow.Pull(_rope.GetLastPoint(), pullForce);
        }
    }

    public void HoldRope()
    {
        if (isLoaded || _arrow.deployed || _joint.enabled ||
            _trf.position.y >= _rope.GetFirstPoint().y)
            return;

        _joint.enabled = true;
    }

    public void ReleaseRope()
    {
        if (_joint.enabled)

        _joint.enabled = false;
    }

    public void SetAxis(Vector2 axis)
    {
        _bow.transform.right = axis;

        if (isLoaded)
        {
            _arrow.transform.up = axis;
        }
    }

    private void UpdateArrow()
    {
        if (isLoaded)
        {
            Vector3 arrowPos = _trf.position +
                (_arrow.transform.up * (1.3f - (stringForce * .75f)));
                
            _arrow.transform.position = arrowPos;
        }
        else
        {
            if (_joint.enabled && _trf.position.y >= _arrow.transform.position.y)
                ReleaseRope();
        }
    }
}