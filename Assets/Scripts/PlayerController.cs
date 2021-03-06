﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public float ledgeHelper;
	public Text healthCanvas;

	// Sounds and animations
	public PlayerSoundsController sounds;
	public PlayerAnimationsController animations;
	public bool cutscene;

	public GameObject hips;
	public GameObject leftLeg;
	public GameObject rightLeg;
	public GameObject leftFoot;
	public GameObject rightFoot;
	public GameObject spine;
	public GameObject leftArm;
	public GameObject rightArm;
	public GameObject leftHand;
	public GameObject rightHand;
	public GameObject gun;

	public GameObject joints;
	public GameObject surface;

	// Health
	private float health;
	private float velocityY;
	private float timeLastHit;

	// Actions
	private Move move;
	private Jump jump;
	private ChangeForm form;
	private Shoot shoot;
	private Dash dash;
	public Reset reset;

	// Status
	public bool airborne;
	public bool shooting;
	public bool dashing;
	public bool jumping;
	public bool isHuman;
	public bool onLedge;
	public bool dead;
	public bool running;
	private bool stopped;
	public bool hangStopped;
	public Vector3 pushing;
	public bool onMetal;

	// Attributes
	public float y;
	public float z;

	public float moveSpeed;
	public float ledgeSpeed;
	public float grabLedgeOffset;
	public float jumpForce;
	public float jumpHoldMultiplier;
	public float jumpHoldDuration;
	public float changeFormCooldown;
	public float bulletSpeed;
	public float attackCooldown;
	public float dashSpeed;
	public float dashCooldown;
	public float dashDuration;

	public Vector3 moveDirection;

	void Start () {
		cutscene = true;
		gameObject.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotation; // disable rotation through physics

		sounds = gameObject.GetComponent<PlayerSoundsController> ();
		animations = gameObject.GetComponent<PlayerAnimationsController> ();
		health = 100.0f;

		airborne = false;
		shooting = false;
		dashing = false;
		jumping = false;
		isHuman = true;
		onLedge = false;
		dead = false;
		running = false;
		stopped = true;
		hangStopped = true;
		pushing = Vector3.zero;

		y = gameObject.GetComponent<BoxCollider> ().bounds.size.y;
		z = gameObject.GetComponent<BoxCollider> ().bounds.size.z;
		moveSpeed = 650.0f;
		ledgeSpeed = 200.0f;
		grabLedgeOffset = 0.5f;
		jumpForce = 32.0f;
		jumpHoldMultiplier = 1.5f;
		jumpHoldDuration = 1.0f;
		changeFormCooldown = 0.5f;
		bulletSpeed = 60.0f;
		attackCooldown = 0.5f;
		dashSpeed = 80.0f;
		dashCooldown = 0.5f;
		dashDuration = 0.1f;
		velocityY = 0.0f;
		moveDirection = Vector3.zero;

		move = new Move (this);
		jump = new Jump (this);
		form = new ChangeForm (this);
		shoot = new Shoot (this);
		dash = new Dash (this);
		reset = new Reset (this);

		healthCanvas.text = new String ('|', 25);

		sounds.PlaySound (PlayerSounds.BACKGROUND_SEA);
		sounds.PlaySound (PlayerSounds.START);
		sounds.PlaySound (PlayerSounds.DRONE_SEA);
    }

	void Update () {
		if (!dead && !cutscene) {
			if (!dashing) {
				move.Input ();
				jump.Input ();
				form.Input ();
			}
			if (isHuman) {
				shoot.Input ();
			} else if (!isHuman && !onLedge) {
				dash.Input ();
			}
			reset.Input ();

			running = moveDirection.magnitude > 0.5f;
			controlAnimation ();
			if (pushing != Vector3.zero) {
				transform.LookAt (pushing);
			}
		}
	}

	void FixedUpdate() {
		if (!dead && !cutscene) {
			airborne = IsAirborne ();
			if (!dashing) {
				move.Action ();
				jump.Action ();
			}
			if (!onLedge) {
				dash.Action ();
			}

			float velF = gameObject.GetComponent<Rigidbody> ().velocity.y;
			if (velF - velocityY > 80.0f && velocityY < 0.0f && velF >= -1f) {
				updateHealth (-((4.0f / 7.0f) * (velF - velocityY) - (250.0f / 7.0f)));
			}
			velocityY = velF;

			if (timeLastHit > 0.0f) {
				timeLastHit -= Time.fixedDeltaTime;
			}

			if (health < 100.0f & timeLastHit < 0.0f) {
				updateHealth (100.0f / (5.0f / Time.fixedDeltaTime));
			}
		}
		else if (dashing) {
			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody> ();
			rigidbody.velocity = Vector3.zero;
			rigidbody.useGravity = true;
			dashing = false;
		}

		reset.Action ();
	}

	void LateUpdate () {
		if (!isHuman) {
			hips.transform.localScale = new Vector3 (1.33f, 1.0f, 1.33f);
			leftLeg.transform.localScale = new Vector3 (1.1f, 0.5f, 1.0f);
			rightLeg.transform.localScale = new Vector3 (1.1f, 0.5f, 1.0f);
			leftFoot.transform.localScale = new Vector3 (1.0f, 1.0f, 0.6f);
			rightFoot.transform.localScale = new Vector3 (1.0f, 1.0f, 0.6f);
			spine.transform.localScale = new Vector3 (1.3f, 1.0f, 1.3f);
			leftArm.transform.localScale = new Vector3 (0.8f, 1.6f, 1.6f);
			rightArm.transform.localScale = new Vector3 (0.8f, 1.6f, 1.6f);
			leftHand.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
			rightHand.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
			gun.SetActive (false);
		} else {
			hips.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			leftLeg.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			rightLeg.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			leftFoot.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			rightFoot.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			spine.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			leftArm.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			rightArm.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			leftHand.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			rightHand.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			gun.SetActive (true);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Shot" && other.gameObject.GetComponent<BulletController> ().owner.tag != "Player") {
			updateHealth (-20.0f);
		} else if (other.gameObject.tag == "Fire") {
			updateHealth (-20.0f);
		} else if (other.gameObject.tag == "Checkpoint") {
			reset.setCheckpoint (other.gameObject.transform.position);
			other.gameObject.SetActive (false);
			sounds.PlaySound (PlayerSounds.CHECKPOINT);
		} else if (other.gameObject.tag == "Water") {
			sounds.PlaySound (PlayerSounds.SPLASH);
			reset.Died ();
		} else if (other.gameObject.tag == "LevelChange") {
			if (!sounds.CheckIfPlaying (PlayerSounds.BACKGROUND_ELECTRIC)) {
				sounds.PlaySound (PlayerSounds.BACKGROUND_ELECTRIC);
			}

			if (!sounds.CheckIfPlaying (PlayerSounds.CITY_INTRO)) {
				sounds.PlaySound (PlayerSounds.CITY_INTRO);
			}
		}
    }

	void OnTriggerStay(Collider other) {
		if (other.gameObject.tag == "Wall") {
			move.CheckOnLedge (other);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "Wall") {
			move.removePlayerOnLedge ();
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Box") {
			if (collision.contacts [0].point.y < collision.gameObject.transform.position.y + collision.gameObject.GetComponent<MeshCollider> ().bounds.size.y / 2) {
				animations.TriggerTransition (animations.PUSH);
				pushing = collision.gameObject.transform.position;
			} else {
				onMetal = true;
			}
		} else if (dashing && collision.gameObject.tag == "Enemy") {
			sounds.PlaySound (PlayerSounds.DASH_IMPACT);
		} else if (collision.gameObject.tag == "Metal") {
			onMetal = true;
		} else {
			RangedController enemy = collision.collider.gameObject.GetComponent<RangedController> ();
			if (enemy != null && enemy.dashing) {
				if (enemy.type == EnemyType.hybrid) {
					updateHealth (-50.0f);
				} else {
					updateHealth (-35.0f);
				}
			}
		}
	}

	void OnCollisionStay(Collision collision) {
		onMetal = false;
		if (collision.gameObject.tag == "Box") {
			if (collision.contacts [0].point.y < collision.gameObject.transform.position.y + collision.gameObject.GetComponent<MeshCollider> ().bounds.size.y / 2) {
				pushing = collision.gameObject.transform.position;
			} else {
				onMetal = true;
			}
		} else if (collision.gameObject.tag == "Metal") {
			onMetal = true;
		}
	}

	void OnCollisionExit (Collision collision) {
		if (collision.gameObject.tag == "Box") {
			if (isHuman) {
				animations.TriggerTransition (animations.RUN_STOP);
			} else {
				animations.TriggerTransition (animations.RUN_STOP_MONKEY);
			}
			pushing = Vector3.zero;
		}
	}

	public void updateHealth (float value) {
		if (!dead) {
			health += value;
			if (health <= 0.0f) {
				healthCanvas.text = "";
			} else {
				healthCanvas.text = new String ('|', (int)Math.Ceiling (health / 4.0f));
			}

			if (value < 0.0f) {
				if (isHuman && !sounds.CheckIfPlaying (PlayerSounds.DAMAGE)) {
					sounds.PlaySound (PlayerSounds.DAMAGE);
				} else if (!isHuman && !sounds.CheckIfPlaying (PlayerSounds.DAMAGE_MONKEY)) {
					sounds.PlaySound (PlayerSounds.DAMAGE_MONKEY);
				}
			}

			if (health <= 20.0f) {
				if (!sounds.CheckIfPlaying (PlayerSounds.HEART_BEAT)) {
					sounds.PlaySound (PlayerSounds.HEART_BEAT);
				}
			}

			//Debug.Log (health);
			if (value < 0.0f) {
				timeLastHit = 3.0f;
			}
			if (health <= 0.0f) {
				health = 100.0f;
				reset.Died ();
			}
		}
	}

	void DebugLookDirection () {
		Debug.DrawRay (transform.position, transform.forward * 35.0f, Color.magenta);
	}

	public bool IsAirborne() {
		int layerMask = 1 << 11;
		layerMask = ~layerMask;
		return !Physics.Raycast (transform.position, -Vector3.up, y / 2 + 0.01f, layerMask);
	}

	public void controlAnimation() {
		if (!airborne) {
			if (running) {
				animations.TriggerTransitionRun (moveDirection, transform.forward, isHuman, false);
				stopped = false;
			} else if (!stopped) {
				if (isHuman) {
					animations.TriggerTransition (animations.RUN_STOP);
				} else {
					animations.TriggerTransition (animations.RUN_STOP_MONKEY);
				}
				stopped = true;
			}
		} else if (onLedge) {
			float direction = Vector3.Dot (Vector3.Cross (moveDirection, transform.forward), transform.up);
			if (direction < 0.0f) {
				animations.TriggerTransitionHang (PlayerAnimationsController.HANG_RIGHT);
				hangStopped = false;
			} else if (direction > 0.0f) {
				animations.TriggerTransitionHang (PlayerAnimationsController.HANG_LEFT);
				hangStopped = false;
			} else if (!hangStopped) {
				animations.TriggerTransition (animations.HANG);
				hangStopped = true;
			}
		}
	}
}
