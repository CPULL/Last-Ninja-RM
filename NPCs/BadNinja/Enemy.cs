using Godot;
using LastNinjaRM;
using System;

namespace LastNinjaRM;

public partial class Enemy : Node3D {
  [Export] AnimationPlayer anim;
  [Export] Vector3 StartLocation;
  [Export] float StartAngle;
  [Export] float AttackDistance;
  [Export] float Speed;
  [Export] float Strenght;
  [Export] RayCast3D RaycastGround;
  [Export] RayCast3D RaycastObstacle;
  [Export] float Health = 100;

  [Export] public Status status = Status.Idle;
  [Export] EnemyType enemyType; // FIXME we need to tweak all code depending on the type
  [Export] Label Dbg;

  bool death = false;
  Game game;
  Node3D player;
  EnemyDef source;
  RandomNumberGenerator rnd = new();
  double forwardTime = 0;
  ShaderMaterial PowerBarMaterial;

  public enum Anim { Idle = 0, Run = 1, Ragdoll = 2, Fight = 3, PunchR = 4, PunchL = 5, Hit1 = 6, Hit2 = 7 };

  StringName AnimIdle = "Idle";
  StringName AnimRun = "Run";
  StringName AnimRagdoll = "Ragdoll";
  StringName AnimFight = "Fight";
  StringName AnimPunchR = "PunchR";
  StringName AnimPunchL = "PunchL";
  StringName AnimHit1 = "Hit1";
  StringName AnimHit2 = "Hit2";
  StringName AnimTurnR = "TurnR";
  StringName AnimTurnL = "TurnL";

  public enum Status { Idle, StartRun, Run, Fight, Punch, BeingHit, Dead, Won }


  /*
   
   State Machine

   Idle -> can only wait to see player -> StartRun
   StartRun -> Run
   Run -> can only wait to be close to the player and moves to Fight, if Hit moves to Hit (or Dead)
   Fight -> can rotate to face the player; waits between shots, the triggers Punch; if player is too far away triggers -> Run; in case Hit  moves to Hit (or Dead)
   Punch -> waits to complete the hit and damages player eventually; in case Hit moves to Hit (or Dead)
   Hit -> waits a short while then checks if the player is close or not and go in Run or Fight
   
   */



  const float distFight = 1.1f;
  const float r2d = 180f / Mathf.Pi;
  bool lastPunchRight = false;
  bool fighting = false; // Do we need it?
  StringName ca = "";
  float waitForPunch = 0;
  float checkHitDelay = 0;
  float beingHitDelay = 0;

  public bool IsPursuing => status == Enemy.Status.Fight || status == Enemy.Status.Run || status == Enemy.Status.StartRun;

  public override void _Ready() {
    //Engine.TimeScale = .5f;

    var a = anim.GetAnimation(AnimIdle);
    a.LoopMode = Animation.LoopModeEnum.Pingpong;
    a = anim.GetAnimation(AnimRun);
    a.LoopMode = Animation.LoopModeEnum.Linear;
    a = anim.GetAnimation(AnimFight);
    a.LoopMode = Animation.LoopModeEnum.Linear;
    a = anim.GetAnimation(AnimTurnR);
    a.LoopMode = Animation.LoopModeEnum.Linear;
    a = anim.GetAnimation(AnimTurnL);
    a.LoopMode = Animation.LoopModeEnum.Linear;
  }
  public void Start(Game g, EnemyDef src) {
    game = g;
    player = g.Player;
    SetAnim(AnimIdle);
    SetStatus(Status.Idle);
    Position = StartLocation;
    RotationDegrees = new(0, StartAngle, 0);

    AttackDistance = src.AttackDistance;
    Speed = src.Speed;
    Strenght = src.Strenght;
    Health = src.Health;
    source = src;
    PowerBarMaterial = game.PowerBarEnemy.Material as ShaderMaterial;
    if (source.Defeated) {
      Position = new(source.DeathPos.X, source.DeathPos.Y, source.DeathPos.Z);
      RotationDegrees = new(0, source.DeathPos.W, 0);
      death = true;
      anim.Play(AnimRagdoll, -1, 200);
      Health = 0;
      PowerBarMaterial?.SetShaderParameter("Value", 0);
    }
    game.RegisterEnemyHitEvent(this);
  }

  protected override void Dispose(bool disposing) {
    game.RemoveEnemyHitEvent(this);
    base.Dispose(disposing);
  }

  [Signal]
  public delegate void HitPlayerEventHandler(float amount);




  void SetAnim(StringName a)  {
    if (ca == a) return;
    ca = a;
    anim.Play(a);
  }

  void SetStatus(Status s) {
    if (s == status) return;
    switch (s) {
      case Status.Idle:
        waitForPunch = .5f;
        SetAnim(AnimIdle);
        break;
      case Status.StartRun:
        waitForPunch = .2f;
        SetAnim(AnimRun);
        break;
      case Status.Run:
        waitForPunch = .2f;
        SetAnim(AnimRun);
        break;
      case Status.Fight:
        SetAnim(AnimFight);
        break;
      case Status.Punch:
        break;
      case Status.BeingHit:
        waitForPunch = rnd.RandfRange(.1f, .3f);
        break;
      case Status.Dead:
        SetAnim(AnimRagdoll);
        break;
      case Status.Won:
        break;
    }
    status = s;
  }

  public override void _Process(double delta) {
    Dbg.Text = $"{status} {ca}  P{waitForPunch:F1} H{checkHitDelay:F1} B{beingHitDelay:F1}";

    if (player == null || status == Status.Won) return;

    // Always get angle and distance between Player and Enemy
    float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
    float angle = r2d * (GlobalTransform.Basis.Z).SignedAngleTo(player.GlobalPosition - GlobalPosition, Vector3.Up);
    float d = (float)delta;

    if (game.IsPlayerDead) SetStatus(Status.Won);

    // Update health progress bar
    if (PowerBarMaterial != null) {
      float val = (float)(PowerBarMaterial.GetShaderParameter("Value").AsDouble() * 100f);
      if (Mathf.Abs(val - Health) > 1) {
        if (val < Health) {
          PowerBarMaterial.SetShaderParameter("Value", Health * .01);
        }
        if (val > Health) {
          val -= d * 50f;
          PowerBarMaterial.SetShaderParameter("Value", val * .01);
        }
      }
      if (val < 1f) {
        PowerBarMaterial.SetShaderParameter("Value", 0);
      }
    }


    switch (status) { // In all statuses if the enemy is Hit then there is the transition to Hit (or Dead)
      case Status.Idle: // can only wait to see player -> run
        CheckPlayer(dist, angle);
        break;
      case Status.StartRun: // Move forward to exit the boot, then switch to the Run status
        MoveForward(d);
        break;
      case Status.Run: // Try to reach the player, if close enough -> Fight
        if (dist <= distFight) SetStatus(Status.Fight);
        else Run(d, angle);
        break;
      case Status.Fight: // Can rotate to face the player; waits between shots, the triggers Punch; if player is too far away triggers -> Run
        Fight(d, dist, angle);
        break;
      case Status.Punch: // Wait for the expected time and check if we hit the player
        Punch(d, dist, angle);
        break;
      case Status.BeingHit: // Just wait for the anim to complete
        beingHitDelay -= d;
        if (beingHitDelay < 0) SetStatus(Status.Fight);
        break;
      case Status.Dead:
        return;
      case Status.Won:
        return;
    }
  }




  // ************************************************************************************************************************


  void CheckPlayer(float dist, float angle) {
    if (dist < 2f) { // We are very close, we ignore the angle
      SetStatus(Status.StartRun); // We need to move forward to exit the boot, ignoring the colliders
      game.SetEnemy(this);
    }
    if (dist < AttackDistance) {
      if (Mathf.Abs(angle) < 75) {
        SetStatus(Status.StartRun); // We need to move forward to exit the boot, ignoring the colliders
        game.SetEnemy(this);
      }
    }
  }

  void MoveForward(float d) {
    Vector3 forward = GlobalTransform.Basis.Z;
    Position += forward * d * Speed;
    forwardTime += d;
    if (forwardTime > .5) {
      forwardTime = 0;
      SetStatus(Status.Run);
      game.SetEnemy(this);
    }
  }

  void Run(float delta, float angle) {
    waitForPunch = 0.1f; // Reset the wait

    float dirAngle = delta * 2 * angle + RotationDegrees.Y;
    RotationDegrees = new(0, dirAngle, 0);
    float absAngle = Math.Abs(angle);
    Vector3 move;
    if (absAngle < 10) {
      // Direct to the player
      move = (player.GlobalPosition - GlobalPosition).Normalized() * Speed * delta;
    }
    else {
      // Move forward
      float speedMalus = (180f - absAngle) / 180f;
      move = GlobalTransform.Basis.Z.Normalized() * Speed * delta * speedMalus;
    }

    var gp = GlobalPosition;
    gp.X += move.X;
    gp.Z += move.Z;
    gp.Y += .1f;
    RaycastGround.GlobalPosition = gp;
    if (!RaycastGround.IsColliding()) {
      var rot = RotationDegrees;
      rot.Y += 180;
      RotationDegrees = rot;
      return; // Not on floor, bump back
    }
    // Check obstacles
    RaycastObstacle.GlobalPosition = gp;
    if (RaycastObstacle.IsColliding()) {
      // Try to turn around following the forward axys of the collider
      if (RaycastObstacle.GetCollider() is Node3D c) {
        var rot = c.RotationDegrees;
        rot.Y -= 90;
        GlobalRotationDegrees = rot;
        SetStatus(Status.StartRun);
        return;
      }

      SetAnim(AnimIdle);
      return; // Over collider, we cannot walk
    }

    Position += move;
  }

  void Fight(float d, float dist, float angle) {
    if (dist > distFight) {
      SetStatus(Status.Run);
      return;
    }

    if (angle > 10 || angle < -10) {
      float dirAngle = d * 3.5f * angle + RotationDegrees.Y;
      RotationDegrees = new(0, dirAngle, 0);
      if (angle > 0) SetAnim(AnimTurnR);
      else SetAnim(AnimTurnR);
      return;
    }
    SetAnim(AnimFight);

    if (waitForPunch > 0) { // Wait between punches
      waitForPunch -= d;
      return;
    }

    if (lastPunchRight) {
      if (rnd.RandiRange(0, 2) == 0) {
        SetAnim(AnimPunchR);
      }
      else {
        SetAnim(AnimPunchL);
        lastPunchRight = false;
      }
    }
    else {
      if (rnd.RandiRange(0, 2) == 0) {
        SetAnim(AnimPunchL);
      }
      else {
        SetAnim(AnimPunchR);
        lastPunchRight = true;
      }
    }
    waitForPunch = rnd.RandfRange(.8333f, 1f);
    checkHitDelay = .2f;
    SetStatus(Status.Punch);
  }

  void Punch(float d, float dist, float angle) {
    checkHitDelay -= d;
    if (checkHitDelay <= 0) {
      if (dist <= distFight && angle > -10 && angle < 10) {
        EmitSignal(SignalName.HitPlayer, Strenght);
      }
      SetStatus(Status.Fight);
    }
  }

  public float HitEnemy(float amount, bool strong) {
    if (death) return 0;


    amount *= 15;

    if (beingHitDelay > 0) {
      Health -= amount * .25f; // If already hit gets less damage
    }
    else Health -= amount;
    SetAnim(rnd.RandiRange(0, 1) == 0 ? AnimHit2 : AnimHit1);

    if (Health <= 0) {
      death = true;
      SetStatus(Status.Dead);
      source.DeathPos.X = Position.X;
      source.DeathPos.Y = Position.Y;
      source.DeathPos.Z = Position.Z;
      source.DeathPos.W = RotationDegrees.Y;
      source.Defeated = true;
      Health = 0;
      source.Health = 0;
      return 0;
    }
    else if (beingHitDelay <= 0) {
      beingHitDelay = 1.15f;
    }
    SetStatus(Status.BeingHit);

    source.Health = Health;
    return Health;
  }



  public bool IsDead() => death;


  public enum EnemyType { NinjaPunch, NinjaSword, NinjaNuncheku, NinjaPole, Samurai, Shogun, Cop, CopGun }
}


