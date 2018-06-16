﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump {
	private PlayerController player;
	private bool startJump;
	private bool continueJump;
	private float jumpTimer;

	public Jump (PlayerController player) {
		this.player = player;
		this.startJump = false;
		this.continueJump = false;
		this.jumpTimer = 0;
	}

	public void Input() {
		float currTime = Time.time;
		if (UnityEngine.Input.GetAxisRaw ("Jump") != 0) {
			if (!player.jumping) {
				startJump = true;
				jumpTimer = currTime;
			} else {
				continueJump = true;
			}
		}
	}

	public void Action() {
		float currTime = Time.time;
		Rigidbody rigidbody = player.gameObject.GetComponent<Rigidbody> ();

		if (!player.airborne || player.onLedge) {
			if (player.jumping && !player.onLedge) {
			}

			player.jumping = false;
		}

		if (startJump) {
			rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
			rigidbody.AddForce (new Vector3 (0, player.jumpForce, 0), ForceMode.Impulse);
			startJump = false;
			player.jumping = true;
		}

		if (continueJump && currTime - jumpTimer <= player.jumpHoldDuration) {
			rigidbody.AddForce (new Vector3 (0, player.jumpForce*player.jumpHoldMultiplier, 0));
			continueJump = false;
		}
	}
}
