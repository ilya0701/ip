﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Dragable : MonoBehaviour
{

    public int normalCollisionCount = 1;
    public float moveLimit = .5f;
    public float collisionMoveFactor = .01f;
    public float addHeightWhenClicked = 0.0f;
    public bool freezeRotationOnDrag = true;
    public Camera cam;
    private Rigidbody myRigidbody;
    private Transform myTransform;
    private bool canMove = false;
    private float yPos;
    private bool gravitySetting;
    private bool freezeRotationSetting;
    private float sqrMoveLimit;
    private int collisionCount = 0;
    private Transform camTransform;

    private Vector3 screenPoint;
    private Vector3 offset;

    void Start()
    {
        myRigidbody = this.gameObject.GetComponent<Rigidbody>();
        myTransform = this.gameObject.transform;
        if (!cam)
        {
            cam = Camera.main;
        }
        if (!cam)
        {
            Debug.LogError("Can't find camera tagged MainCamera");
            return;
        }
        camTransform = cam.transform;
        sqrMoveLimit = moveLimit * moveLimit;   // Since we're using sqrMagnitude, which is faster than magnitude
    }

    void OnMouseDown()
    {
        canMove = true;
        myTransform.Translate(Vector3.up * addHeightWhenClicked);
        gravitySetting = myRigidbody.useGravity;
        freezeRotationSetting = myRigidbody.freezeRotation;
        myRigidbody.useGravity = false;
        myRigidbody.freezeRotation = freezeRotationOnDrag;
        yPos = myTransform.position.y;

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseUp()
    {
        canMove = false;
        myRigidbody.useGravity = gravitySetting;
        myRigidbody.freezeRotation = freezeRotationSetting;
        if (!myRigidbody.useGravity)
        {
            Vector3 pos = myTransform.position;
            pos.y = yPos - addHeightWhenClicked;
            myTransform.position = pos;
        }
    }

    void OnCollisionEnter()
    {
        collisionCount++;
    }

    void OnCollisionExit()
    {
        collisionCount--;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }

        myRigidbody.velocity = Vector3.zero;
        myRigidbody.angularVelocity = Vector3.zero;

        Vector3 pos = myTransform.position;
        pos.y = yPos;
        myTransform.position = pos;

        Vector3 mousePos = Input.mousePosition;
        Vector3 move = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camTransform.position.y - myTransform.position.y)) - myTransform.position;
        move.y = 0.0f;
        if (collisionCount > normalCollisionCount)
        {
            move = move.normalized * collisionMoveFactor;
        }
        else if (move.sqrMagnitude > sqrMoveLimit)
        {
            move = move.normalized * moveLimit;
        }

        Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
    //    transform.position = cursorPosition;

        myRigidbody.MovePosition(cursorPosition);

        //myRigidbody.MovePosition(myRigidbody.position + move);
    }
}