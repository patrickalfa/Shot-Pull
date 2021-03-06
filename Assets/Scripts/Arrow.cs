﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D _rgbd;
    private Transform _trf;
    private Player _player;

    [Header("DEBUG")]
    public bool active;
    public bool deployed;
    public float maxVelocity;

    private void Start()
    {
        _trf = transform;
        _rgbd = GetComponent<Rigidbody2D>();
        _player = FindObjectOfType<Player>();

        SetDeployed(false);
    }

    private void Update()
    {
        if (deployed)
            _trf.up = Vector3.Lerp(_trf.up, _rgbd.velocity, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (deployed)
            LimitVelocity();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "Player")
        {
            col.GetComponent<Player>().isLoaded = true;
            active = true;
        }

        if (!deployed || !active)
            return;

        SetDeployed(false);
    }

    private void LimitVelocity()
    {
        Vector2 vel = _rgbd.velocity;
        if (vel.magnitude > maxVelocity)
            _rgbd.velocity = vel.normalized * maxVelocity;
    }

    public void SetDeployed(bool state)
    {
        deployed = state;
        _rgbd.constraints = state ?
            RigidbodyConstraints2D.None :
            RigidbodyConstraints2D.FreezeAll;
    }

    public void Deploy(Vector2 force)
    {
        SetDeployed(true);

        active = true;

        _rgbd.velocity = Vector2.zero;
        _rgbd.AddForce(force, ForceMode2D.Impulse);
    }

    public void Pull(Vector3 towards, float strength)
    {
        _rgbd.velocity += (Vector2)(towards - _trf.position).normalized * strength * Time.deltaTime;
    }
}