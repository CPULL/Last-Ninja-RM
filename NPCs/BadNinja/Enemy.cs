using Godot;
using LastNinjaRM;
using System;

public partial class Enemy : Node3D {
  [Export] AnimationPlayer animPlayer;
  [Export] AnimationTree animTree;
  [Export] Vector3 StartLocation;
  [Export] float StartAngle;
  [Export] float AttackDistance;
  [Export] float Speed;
  [Export] float Strenght;
  [Export] RayCast3D RaycastGround;
  [Export] RayCast3D RaycastObstacle;
  [Export] float Health = 100;

  [Export] public int anim = (int)Anim.Idle;
  [Export] public Status status = Status.Waiting;
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

  public enum Status { Waiting, StartFighitng, Fighting, Dead, Won }

  const float distFight = 1.1f;
  bool punching = false;
  bool beingHit = false;
  const float r2d = 180f / Mathf.Pi;

  public override void _Ready() {
    var a = animPlayer.GetAnimation("Idle");
    a.LoopMode = Animation.LoopModeEnum.Pingpong;
    a = animPlayer.GetAnimation("Run");
    a.LoopMode = Animation.LoopModeEnum.Linear;

    animTree.AnimationFinished += AnimationCompleted;
  }
  public void Start(Game g, EnemyDef src) {
    game = g;
    player = g.Player;
    anim = (int)Anim.Idle;
    status = Status.Waiting;
    Position = StartLocation;
    RotationDegrees = new(0, StartAngle, 0);

    AttackDistance = src.AttackDistance;
    Speed = src.Speed;
    Strenght = src.Strenght;
    Health = 100;
    source = src;
    if (source.Defeated) {
      Position = new(source.DeathPos.X, source.DeathPos.Y, source.DeathPos.Z);
      RotationDegrees = new(0, source.DeathPos.W, 0);
      death = true;
      anim = (int)Anim.Ragdoll;
      Health = 0;
    }
    else {
      PowerBarMaterial = game.PowerBarEnemy.Material as ShaderMaterial;
      PowerBarMaterial.SetShaderParameter("Value", Health * .01f);
    }
    game.RegisterEnemyHitEvent(this);
  }

  [Signal]
  public delegate void HitPlayerEventHandler(float amount);


  private void AnimationCompleted(StringName animName) {
    if (animName == "PunchL" || animName == "PunchR") {
      punching = false;
      anim = (int)Anim.Fight;
    }
    if (animName == "Hit1" || animName == "Hit2") {
      beingHit = false;
      anim = (int)Anim.Fight;
    }
  }


  public override void _Process(double delta) {
    if (death || player == null) return;
    float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
    float angle = r2d * (GlobalTransform.Basis.Z).SignedAngleTo(player.GlobalPosition - GlobalPosition, Vector3.Up);

    float d = (float)delta;

    if (game.IsPlayerDead) status = Status.Won;


    switch (status) {
      case Status.Waiting:
        CheckPlayer(dist, angle);
        break;
      case Status.StartFighitng:
        MoveForward(d);
        break;
      case Status.Fighting:
        if (punching || beingHit) return; // We should not move if we are playing an anim for punching or being hit
        if (dist <= distFight || punchDelay > 0) Fight(d, dist, angle);
        else Run(d, angle);
        break;
      case Status.Dead:
        break;
      case Status.Won:
        break;
    }

    if (hitDelay > 0) {
      hitDelay -= delta;
      if (hitDelay < 0) {
        if (dist <= distFight && angle > -10 && angle < 10) {
          EmitSignal(SignalName.HitPlayer, Strenght);
        }
        anim = (int)Anim.Fight;
      }
    }

    if (PowerBarMaterial != null) {
      double val = PowerBarMaterial.GetShaderParameter("Value").AsDouble();
      if (val > Health * .01) {
        val -= delta * .5;
        PowerBarMaterial.SetShaderParameter("Value", val);
      }
      if (val < 0.005) {
        PowerBarMaterial = null;
        PowerBarMaterial.SetShaderParameter("Value", 0);
      }
    }
  }


  void MoveForward(float d) {
    anim = (int)Anim.Run;
    Vector3 forward = GlobalTransform.Basis.Z;
    Position += forward * d * Speed;
    forwardTime += d;
    if (forwardTime > .5) {
      forwardTime = 0;
      status = Status.Fighting;
      game.SetEnemy(this);
    }
  }

  private void Run(float delta, float angle) {
    Dbg.Text = $"Running    {Health}";
    anim = (int)Anim.Run;
    lastPunch = 0;
    punchDelay = 0;
    hitDelay = 0;

    float dirAngle = delta * 2 * angle + RotationDegrees.Y;
    RotationDegrees = new(0, dirAngle, 0);

    // Move in 8 directions only, using the provided speed

    float absAngle = Math.Abs(angle);
    Vector3 move;
    if (absAngle < 10) {
      // Direct to the player
      move = (player.GlobalPosition - GlobalPosition).Normalized() * Speed * delta;
    }
    else {
      float speedMalus = (180f - absAngle) / 180f;
      move = GlobalTransform.Basis.Z.Normalized() * Speed * delta * speedMalus;
    }

    var gp = GlobalPosition;
    gp.X += move.X;
    gp.Z += move.Z;
    gp.Y += .1f;
    RaycastGround.GlobalPosition = gp;
    if (!RaycastGround.IsColliding()) {
      anim = (int)Anim.Idle;
      return; // Not on floor
    }
    // Check obstacles
    RaycastObstacle.GlobalPosition = gp;
    if (RaycastObstacle.IsColliding()) {
      anim = (int)Anim.Idle;
      return; // Over collider
    }

    Position += move;
  }

  float lastPunch = 0; // Delay between punches
  float punchDelay = 0; // Time for a punch to complete
  double hitDelay = 0;
  bool lastPunchRight = false;

  private void Fight(float d, float dist, float angle) {
    anim = (int)Anim.Fight;

    if (angle > 10 || angle < -10) {
      float dirAngle = d * 2.5f * angle + RotationDegrees.Y;
      RotationDegrees = new(0, dirAngle, 0);
      return;
    }

    if (punchDelay > 0) {
      punchDelay -= d;
      return;
    }
    lastPunch -= d;
    if (lastPunch > 0 || punching || beingHit) return;

    if (lastPunchRight) {
      if (rnd.RandiRange(0, 2) == 0) { 
        anim = (int)Anim.PunchR;
        punchDelay = .8333f;
        hitDelay = .2;
      }
      else {
        anim = (int)Anim.PunchL;
        punchDelay = .8333f;
        hitDelay = .2;
        lastPunchRight = false;
      }
    }
    else {
      if (rnd.RandiRange(0, 2) == 0) {
        anim = (int)Anim.PunchL;
        punchDelay = .8333f;
        hitDelay = .2;
      }
      else {
        anim = (int)Anim.PunchR;
        punchDelay = .8333f;
        hitDelay = .2;
        lastPunchRight = true;
      }
    }
    lastPunch = rnd.RandfRange(.1f, .5f);
    Dbg.Text = $"    {Health}";
  }

  public float Hit(float amount, bool strong) {
    if (death) return 0;
    Health -= amount;
    anim = strong ? (int)Anim.Hit2 : (int)Anim.Hit1;

    if (Health <= 0) {
      death = true;
      anim = (int)Anim.Ragdoll;
      source.DeathPos.X = Position.X;
      source.DeathPos.Y = Position.Y;
      source.DeathPos.Z = Position.Z;
      source.DeathPos.W = RotationDegrees.Y;
      source.Defeated = true;
    }

    return Health;
  }


  void CheckPlayer(float dist, float angle) {
    if (dist < 2f) { // We are very close, we ignore the angle
      status = Status.StartFighitng; // We need to move forward to exit the boot, ignoring the colliders
      game.SetEnemy(this);
    }
    if (dist < AttackDistance) {
      if (Mathf.Abs(angle) < 75) {
        status = Status.StartFighitng; // We need to move forward to exit the boot, ignoring the colliders
        game.SetEnemy(this);
      }
    }
  }

  public bool IsDead() => death;


  public enum EnemyType { NinjaPunch, NinjaSword, NinjaNuncheku, NinjaPole, Samurai, Shogun, Cop, CopGun }
}


