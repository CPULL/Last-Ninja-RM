using Godot;
using System;
using System.Collections.Generic;

namespace LastNinjaRM;

[Tool]
public partial class Game : Node {
  [Export] public Node3D Player;
  [Export] AnimationTree animTree;
  [Export] AnimationPlayer animPlayer;
  [Export] float Speed;
  [Export] Node3D RoomNode;
  [Export] Room room;
  [Export] RayCast3D RayCastGround, RayCastObstacles, RayCastPickup, RayCastRisk;
  [Export] Label Dbg, LabelLocation;
  

  [ExportGroup("Templates")]
  [Export] PackedScene[] Templates;
  [Export] PackedScene ErrorScene;

  [ExportGroup("Inventory")]
  [Export] TextureRect[] Weapons;
  [Export] TextureRect[] Items;
  [Export] PackedScene[] PickabeItems;
  [Export] Node3D Katana, Nunchaku1, Nunchaku2;


  [ExportGroup("Props")]

  [Export] int anim = 0; // 0 idle; 1 run; 2 jump; 3 grab; 
  [Export] TextureRect PowerBarNinja;
  [Export] public TextureRect PowerBarEnemy;

  [ExportGroup("Audio")]
  [Export] AudioStreamPlayer Music, PlayerSound, OtherSound;
  [Export] AudioStream IntroMusic;
  [Export] AudioStream PickupSound, WaterSplash;
  [Export] AudioStream[] WaterSteps;

  ShaderMaterial PowerBarMaterial;
  public readonly List<Enemy> enemies = new();
  Enemy currentEnemy = null;

  Vector3 rotation = Vector3.Zero;
  Vector3 move = Vector3.Zero;
  bool running = false;
  readonly bool[] hasItem = new bool[ItemEND];
  RandomNumberGenerator rnd = new();
  const float r2d = 180f / Mathf.Pi;

  double jumpTime = 0.8333;
  Rot jumpDir = Rot.None;
  bool jumpNotRunning = false;
  bool jumpStopped = false;

  int usedWeapon = ItemNone; // No weapon

  PlayerStatus status = PlayerStatus.NONE;
  public enum PlayerStatus {
    NONE = -1, Idle, Fight, Jump, Run, Grab, Climb, Fall, Death, FallToDeath
  }


  public override void _Ready() {
    var a = animPlayer.GetAnimation("Idle");
    a.LoopMode = Animation.LoopModeEnum.Pingpong;
    a = animPlayer.GetAnimation("Run");
    a.LoopMode = Animation.LoopModeEnum.Linear;
    a = animPlayer.GetAnimation("Fall");
    a.LoopMode = Animation.LoopModeEnum.Linear;
    a = animPlayer.GetAnimation("Climb");
    a.LoopMode = Animation.LoopModeEnum.Linear;

    usedWeapon = ItemNone;
    Katana.Visible = false;
    Nunchaku1.Visible = false;
    Nunchaku2.Visible = false;
    hasItem[ItemNone] = true; // Just fists are always available
    for (int i = 1; i < hasItem.Length; i++) {
      hasItem[i] = false;
      Weapons[i].Visible = false;
      Items[i].Visible = false;
    }

    GetTree().CreateTimer(.25).Timeout += Startup;
  }

  [Export] TextureRect DebugLogo;
  void Startup() {
    // FIXME Music.Play();
    DebugLogo.Visible = false;
    currentRoom = room.BuildRoom(World.GardenSouth, 0);
    currentEnemy = null;
    LabelLocation.Text = currentRoom.RoomName;
    Health = 100;
    PowerBarMaterial = PowerBarNinja.Material as ShaderMaterial;
    PowerBarMaterial.SetShaderParameter("Value", Health * .01f);
  }

  public override void _UnhandledKeyInput(InputEvent @event) {
    if (@event is not InputEventKey k) return;
    if (k.Keycode == Key.Q && k.Pressed) { currentRoom = room.BuildRoom(World.GardenNorthPoolWall, 0); currentEnemy = null; }
      if (k.Keycode == Key.Escape && k.Pressed) GetTree().Quit();
  }


  void SetStatus(PlayerStatus s, Anim a) {
    if (status == PlayerStatus.NONE || status == PlayerStatus.Death) return;
    if ((status == PlayerStatus.Fall || status == PlayerStatus.FallToDeath) && s != PlayerStatus.Death) return;

    int an = (int)a;
    if (s == status && anim == (int)a) return;
    
    status = s;
    anim = an;
  }
  public void SetIdleStatus() {
    status = PlayerStatus.Idle;
    anim = (int)Anim.Idle;
    animTree.Set("parameters/TimeScale/scale", 1);
    animTree.Active = true;
  }
  public void SetStatusNONE() {
    status = PlayerStatus.NONE;
    anim = (int)Anim.Idle;
    animTree.Set("parameters/TimeScale/scale", 1);
    animTree.Active = false;
  }

  public override void _Process(double delta) {
    if (Engine.IsEditorHint() || status == PlayerStatus.NONE) return;
    float d = (float)delta;

    // FIXME    var currentAnim = (animTree.Get("parameters/playback").AsGodotObject() as AnimationNodeStateMachinePlayback).GetCurrentNode(); // FIXME to be removed

    if (PowerBarMaterial != null) {
      double val = PowerBarMaterial.GetShaderParameter("Value").AsDouble();
      if (val > Health * .01) {
        val -= delta * (Health <= 0 ? 3 : .5);
        if (val < 0) val = 0;
        PowerBarMaterial.SetShaderParameter("Value", val);
      }
    }

    CheckBadAreas(d);

    switch (status) {
      case PlayerStatus.NONE: return;
      case PlayerStatus.Idle: // Move, start jumping, pickup, switch weapons; check player being hit
        break;
      case PlayerStatus.Fight: // Check hits on enemies and check player being hit
        break;
      case PlayerStatus.Jump: // Wait until we finish the jump; we may stop the forward movement
        HandleJump(d);
        break;
      case PlayerStatus.Run: // Just go in the direction and check for start jumping or fights
        break;
      case PlayerStatus.Grab: // Pick up object, wait for action to complete
        break;
      case PlayerStatus.Climb: // Change input system and allow going up and down (and jump)
        HandleClimbing(d);
        return;
      case PlayerStatus.Fall: // Disable movements, wait to reach the floor doing gravity and playing fall animation
      case PlayerStatus.FallToDeath: // Fall until we reach the ground then transition to death status
        HandleFalling(d);
        return;
      case PlayerStatus.Death: // Just play the death anim and handle respawn and gameover
        Health = 0;
        return;
    }



    if (hitDelay > 0) {
      hitDelay -= delta;
      anim = (int)Anim.Idle;
      return;
    }

    HandleMovement(d);

    if (fightTime > 0) fightTime -= delta;
    if (fightTime <= 0) {
      if (anim >= (int)Anim.FightBase) {
        anim = 0;
        if (usedWeapon == ItemNunchaku) {
          Nunchaku1.Visible = true;
          Nunchaku2.Visible = false;
        }
      }
      if (checkHit) {
        // Check if we hit an enemy
        foreach (var e in enemies) {
          if (e.IsDead()) continue;
          float angle = r2d * move.SignedAngleTo(e.GlobalPosition - Player.GlobalPosition, Vector3.Up);
          float dist = Player.GlobalPosition.DistanceTo(e.GlobalPosition);
          if (Mathf.Abs(angle) < 50 && dist < usedWeapon switch {
            ItemNone => 1.5f,
            ItemKatana => 1.75f,
            ItemNunchaku => 2f,
            ItemKnuckles => 1.5f,
            _ => 1.5f
          }) {
            float damage = usedWeapon switch {
              ItemKatana => 8,
              ItemNunchaku => 4,
              ItemKnuckles => fighting == Anim.KickL || fighting == Anim.KickR ? 3 : 6,
              _ => 1
            };

            e.Hit(damage, fighting == Anim.SwordL || fighting == Anim.SwordR);
            currentEnemy = e;

            // FIXME the amount of damage should be proportional to the weapon and the armor/deficiency of the enemy

          }
        }
        checkHit = false;
      }
    }
  }

  private void CheckBadAreas(float d) {
    if (status == PlayerStatus.NONE || status == PlayerStatus.FallToDeath || status == PlayerStatus.Death) return;

    // Check if we are over a pool, mud, or lava. If we are and we are not jumping we should die
    Vector3 gp = Player.GlobalPosition;
    gp.Y = 5f;
    RayCastGround.GlobalPosition = gp;
    RayCastRisk.GlobalPosition = gp;

    if (status != PlayerStatus.Jump && status != PlayerStatus.Fall && !RayCastGround.IsColliding() && RayCastRisk.IsColliding()) {
      if (HitPlayer(d * 2, false)) {
        SetStatus(PlayerStatus.Death, Anim.Dead);
      }
    }
  }

  Rot direction = Rot.None;
  double fightTime = 0.5;
  bool checkHit = false;

  public enum Anim {
    Idle = 0, Run = 1, Jump = 2, Use = 3, Hit1 = 4, Hit2 = 5, Dead = 6, Climb = 7, Fall = 8, FightBase=10, FistL = 10, FistR = 11, KickL = 12, KickR = 13, SwordL = 14, SwordR = 15
  };
  Anim fighting = Anim.Idle;

  Pickable interactionItem = null;

  float verticalFall = -.45f;
  void HandleFalling(float d) {
    if (status == PlayerStatus.NONE) return;

    var gp = Player.GlobalPosition;
    gp.Y += 1f;
    RayCastObstacles.GlobalPosition = gp;
    RayCastObstacles.GlobalRotationDegrees = new(0, (int)direction, 0);
    gp.X += move.X;
    gp.Z += move.Z;
    gp.Y += 4.5f;
    RayCastGround.GlobalPosition = gp;
    RayCastRisk.GlobalPosition = gp;
    float groundY = -1;
    float riskY = -1;
    move = Player.GlobalTransform.Basis.Z * 1.2f;
    if (RayCastObstacles.IsColliding()) {
      move.X = 0;
      move.Z = 0;
    }
    move.Y = verticalFall;
    verticalFall -= d * 9.812f;
    Player.Position += move * d;
    if (RayCastGround.IsColliding()) groundY = RayCastGround.GetCollisionPoint().Y;
    if (RayCastRisk.IsColliding()) riskY = RayCastRisk.GetCollisionPoint().Y;
    if (groundY > riskY && MathF.Abs(groundY - Player.Position.Y) < .4f) {
      if (verticalFall < -4f) HitPlayer((-verticalFall - 4) * 3, false);
      verticalFall = -.45f;
      Vector3 pos = Player.Position;
      pos.Y = groundY + 0.001f;
      Player.Position = pos;
      if (status == PlayerStatus.FallToDeath) SetStatus(PlayerStatus.Death, Anim.Dead);
      else SetIdleStatus();
      return;
    }
    else if (groundY <= riskY && MathF.Abs(riskY - Player.Position.Y) < .4f) {
      if (verticalFall < -4f) HitPlayer((-verticalFall - 4) * 4, false);
      verticalFall = -.45f;
      Vector3 pos = Player.Position;
      pos.Y = riskY + 0.001f;
      Player.Position = pos;
      if (status == PlayerStatus.FallToDeath) SetStatus(PlayerStatus.Death, Anim.Dead);
      else SetIdleStatus();
      PlayerSound.Stream = WaterSplash;
      PlayerSound.Play();
      return;
    }
    if (Player.Position.Y <= 0) {
      Vector3 pos = Player.Position;
      pos.Y = 0.001f;
      Player.Position = pos;
      verticalFall = -.45f;
      // Check if we have a collider again, if not we die
      gp = Player.GlobalPosition;
      gp.Y += 5.5f;
      RayCastGround.GlobalPosition = gp;
      RayCastRisk.GlobalPosition = gp;
      if (!RayCastGround.IsColliding() && !RayCastRisk.IsColliding()) SetStatus(PlayerStatus.Death, Anim.Dead);
      else SetIdleStatus();
    }
  }

  void HandleClimbing(float d) {
    if (status == PlayerStatus.NONE) return;
    // Switch weapons
    if (Input.IsActionJustPressed("Select")) SwitchWeapon();

    bool up = Input.IsActionPressed("Up");
    bool down = Input.IsActionPressed("Down");
    bool jumpStart = Input.IsActionJustPressed("Jump");

    if (up) {
      var pos = Player.Position;
      pos.Y += Speed * d * .5f;
      Player.Position = pos;
      animTree.Set("parameters/TimeScale/scale", 3);
      if (Player.Position.Y > interactionItem.MinMaxY.Y) {
        SetStatus(PlayerStatus.Idle, Anim.Idle);
        anim = 0;
        pos = interactionItem.Position + interactionItem.GlobalTransform.Basis.Z * .1f;
        pos.Y = 3f; // FIXME make it variable
        Player.Position = pos;
        animTree.Set("parameters/TimeScale/scale", 1);
      }
    }
    else if (down) {
      var pos = Player.Position;
      pos.Y -= Speed * d * .5f;
      Player.Position = pos;
      animTree.Set("parameters/TimeScale/scale", -3);
      if (Player.Position.Y < interactionItem.MinMaxY.X) {
        SetStatus(PlayerStatus.Idle, Anim.Idle);
        anim = 0;
        pos = interactionItem.Position - interactionItem.GlobalTransform.Basis.Z * .2f;
        pos.Y = interactionItem.MinMaxY.X;
        Player.Position = pos;
        animTree.Set("parameters/TimeScale/scale", 1);
      }
    }
    else animTree.Set("parameters/TimeScale/scale", 0);

    if (jumpStart) {
      SetStatus(PlayerStatus.Idle, Anim.Idle);
      anim = 0;
      var pos = interactionItem.Position - interactionItem.GlobalTransform.Basis.Z * .2f;
      pos.Y = interactionItem.MinMaxY.X;
      Player.Position = pos;
      animTree.Set("parameters/TimeScale/scale", 1);
    }
  }

  void HandleJump(float d) {
    if (status == PlayerStatus.NONE) return;

    bool jumpPress = Input.IsActionPressed("Jump");
    bool up = Input.IsActionPressed("Up");
    bool down = Input.IsActionPressed("Down");
    bool left = Input.IsActionPressed("Left");
    bool right = Input.IsActionPressed("Right");
    bool anyDir = up || down || left || right;

    jumpTime -= d;
    if (jumpTime <= 0) SetStatus(PlayerStatus.Idle, anyDir ? Anim.Run : Anim.Idle);
    else SetStatus(PlayerStatus.Jump, Anim.Jump);
    if (!jumpPress) jumpStopped = true;

    if (!jumpStopped) {
      move.X = 0;
      move.Z = 0;
      direction = jumpDir;
      if (CheckWalkable(d, out float resY)) {
        // We play the anim, and we should jump for a certain amount of time, we can avoid traps and non-walkable parts, but not obstacles
        Vector3 pos = Player.Position + move * 2;
        pos.Y = resY;
        Player.Position = pos;
        rotation.Y = (int)direction;
        Player.RotationDegrees = rotation;
      }
    }
  }

  void HandleMovement(float d) {
    if (status == PlayerStatus.NONE) return;

    // Switch weapons
    if (Input.IsActionJustPressed("Select")) SwitchWeapon();


    bool up = Input.IsActionPressed("Up");
    bool down = Input.IsActionPressed("Down");
    bool left = Input.IsActionPressed("Left");
    bool right = Input.IsActionPressed("Right");

    bool fight = Input.IsActionPressed("Fight");
    bool fightP = Input.IsActionJustReleased("Fight");



    bool jumpStart = Input.IsActionJustPressed("Jump");
    bool jumpPress = Input.IsActionPressed("Jump");

    bool use = Input.IsActionJustPressed("Use");


    bool anyDir = up || down || left || right;

    /*
     
    Fight -> Always precedence

    Only move => move
    Just jump => climb (do not jump in place)
    Jump while moving => long jump
    Jump and then move (1/10 of sec) => short jump

 */

    if (fight) {
      SetStatus(PlayerStatus.Idle, Anim.Idle);
      bool doit = false;
      if (up) {
        if (usedWeapon == ItemKatana) fighting = Anim.SwordL;
        else if (usedWeapon == ItemNunchaku) { fighting = Anim.SwordL; Nunchaku1.Visible = false; Nunchaku2.Visible = true; }
        else fighting = Anim.FistL;
        doit = true;
      }
      else if (left) {
        fighting = Anim.KickL;
        doit = true;
      }
      else if (down) {
        fighting = Anim.KickR;
        doit = true;
      }
      else if (right) {
        if (usedWeapon == ItemKatana) fighting = Anim.SwordR;
        else if (usedWeapon == ItemNunchaku) { fighting = Anim.SwordR; Nunchaku1.Visible = false; Nunchaku2.Visible = true; }
        else fighting = Anim.FistR;
        doit = true;
      }
      if (doit && fightTime <= 0) {
        if (usedWeapon == ItemNunchaku) fightTime = .4;
        else fightTime = .5;
        checkHit = true;
        SetStatus(PlayerStatus.Fight, fighting);
      }
      return;
    }



    else if (fightP && !anyDir) {
      SetStatus(PlayerStatus.Idle, Anim.Idle);
      if (rnd.RandiRange(0, 10) < 5) {
        if (usedWeapon == ItemKatana) fighting = Anim.SwordR;
        else if (usedWeapon == ItemNunchaku) { fighting = Anim.SwordR; Nunchaku1.Visible = false; Nunchaku2.Visible = true; }
        else fighting = Anim.FistR;
      }
      else {
        if (usedWeapon == ItemKatana) fighting = Anim.SwordL;
        else if (usedWeapon == ItemNunchaku) { fighting = Anim.SwordL; Nunchaku1.Visible = false; Nunchaku2.Visible = true; }
        else fighting = Anim.FistL;
      }
      if (fightTime <= 0) {
        SetStatus(PlayerStatus.Fight, fighting);
        fightTime = .5;
        checkHit = true;
        return;
      }
    }

    /*
     jump alone -> jump in the faced direction
     jump and move -> jump in the faced direction
     when releasing the jump te jump should stop
     
     
     */



    else {
      if (jumpStart) {
        // We should check if we can climb
        if (Pickup()) return; // Something was pickable

        jumpStopped = false;
        jumpTime = .84;
        SetStatus(PlayerStatus.Jump, Anim.Jump);
        CalculateDirection(up, down, left, right);
        if (direction == Rot.None) jumpDir = (Rot)Player.RotationDegrees.Y;
        else jumpDir = direction;
        direction = jumpDir;
        if (CheckWalkable(d, out float resY)) {
          Vector3 pos = Player.Position + move;
          pos.Y = resY;
          Player.Position = pos;
        }
      }
      else if (anyDir) {
        CalculateDirection(up, down, left, right);
        move.X = 0;
        move.Z = 0;
        if (CheckWalkable(d, out float resY)) {
          SetStatus(PlayerStatus.Run, Anim.Run);
          rotation.Y = (int)direction;
        } else SetStatus(PlayerStatus.Idle, Anim.Idle);
        rotation.Y = CheckEnemiesFighting(rotation.Y);
        Player.RotationDegrees = rotation;
        Vector3 pos = Player.Position + move;
        pos.Y = resY;
        Player.Position = pos;
      }
      else if (use) {
        Pickup();
        // FIXME we may want some different anims depending on the type of object
        return;
      }
      else {
        SetStatus(PlayerStatus.Idle, Anim.Idle);
      }
    }

  }

  public void PlayerStepDone() {
    var gp = Player.GlobalPosition;
    gp.Y += 5.5f;
    RayCastGround.GlobalPosition = gp;
    RayCastRisk.GlobalPosition = gp;
    float groundY = -1;
    float riskY = -1;
    if (RayCastGround.IsColliding()) groundY = RayCastGround.GetCollisionPoint().Y;
    if (RayCastRisk.IsColliding()) riskY = RayCastRisk.GetCollisionPoint().Y;

    if (groundY > riskY) { // Normal step sound
    }
    else { // Water sound FIXME we can do lava or other bad sounds in future
      PlayerSound.Stream = WaterSteps[rnd.RandiRange(0, 3)];
      PlayerSound.Play();
    }

  }

  private float CheckEnemiesFighting(float yAngle) {

    animTree.Set("parameters/TimeScale/scale", 1);
    if (currentEnemy == null) { Dbg.Text = $"{yAngle:n0} no enemy"; return yAngle; } // No enemies
    if (currentEnemy.status != Enemy.Status.Fighting && currentEnemy.status != Enemy.Status.StartFighitng) { Dbg.Text = $"{yAngle:n0} no fight"; return yAngle; } // No fighting
    if (Player.Position.DistanceTo(currentEnemy.Position) > 5f) { Dbg.Text = $"{yAngle:n0} far away"; return yAngle; } // Far away

    // Calculate the angle that is going to the enemy
    float angle = 235 + r2d * Player.GlobalPosition.SignedAngleTo(currentEnemy.GlobalPosition - Player.GlobalPosition, Vector3.Up);

    // Round it to the 8 directions
    angle = (int)(angle / 45) * 45f;

    Dbg.Text = $"{angle:n0}";

    animTree.Set("parameters/TimeScale/scale", -1); // Walk backward
    return angle;
  }

  private void CalculateDirection(bool up, bool down, bool left, bool right) {
    direction = Rot.None;
    if (up) {
      if (left) direction = Rot.T;
      else if (right) direction = Rot.R;
      else direction = Rot.TR;
    }
    if (down) {
      if (left) direction = Rot.L;
      else if (right) direction = Rot.B;
      else direction = Rot.BL;
    }
    if (left) {
      if (up) direction = Rot.T;
      else if (down) direction = Rot.L;
      else direction = Rot.TL;
    }
    if (right) {
      if (up) direction = Rot.R;
      else if (down) direction = Rot.B;
      else direction = Rot.BR;
    }
  }

  void SwitchWeapon() {
    for (int i = 0; i < ItemEND; i++) {
      int index = (usedWeapon + i) % ItemEND;
      if (index == usedWeapon) continue;
      if (hasItem[index]) {
        Weapons[usedWeapon].Visible = false;
        usedWeapon = index;
        Weapons[usedWeapon].Visible = true;
        break;
      }
    }
    Katana.Visible = usedWeapon == ItemKatana;
    Nunchaku1.Visible = usedWeapon == ItemNunchaku;
    Nunchaku2.Visible = false;
  }

  private bool CheckWalkable(float d, out float resY) {
    resY = 0;
    if (direction == Rot.None) return false;
    var portal = OverPortal();
    if (portal != null && portal.RoomIndex != -1) {
      // Teleport
      currentRoom = room.BuildRoom(portal.RoomIndex, portal.PortalIndex);
      currentEnemy = null;
      LabelLocation.Text = currentRoom.RoomName;
      return false;
    }
    switch (direction) {
      case Rot.TR:
        move.X = 0;
        move.Z = 1;
        break;
      case Rot.T:
        move.X = 0.7071f;
        move.Z = 0.7071f;
        break;
      case Rot.TL:
        move.X = 1;
        move.Z = 0;
        break;
      case Rot.L:
        move.X = 0.7071f;
        move.Z = -0.7071f;
        break;
      case Rot.BL:
        move.X = 0;
        move.Z = -1;
        break;
      case Rot.B:
        move.X = -0.7071f;
        move.Z = -0.7071f;
        break;
      case Rot.BR:
        move.X = -1;
        move.Z = 0;
        break;
      case Rot.R:
        move.X = -0.7071f;
        move.Z = 0.7071f;
        break;
      case Rot.None:
        break;
    }
    move *= Speed * d;
    float x = move.X + Player.Position.X;
    float y = move.Z + Player.Position.Z;

    // Check if we are on a road and not over and obstacle
    var gp = Player.GlobalPosition;
    gp.Y += 1f;
    RayCastObstacles.GlobalPosition = gp;
    RayCastObstacles.GlobalRotationDegrees = new(0, (int)direction, 0);
    gp.X += move.X;
    gp.Z += move.Z;
    gp.Y = 4.5f;
    RayCastGround.GlobalPosition = gp;
    RayCastRisk.GlobalPosition = gp;
    bool ground = RayCastGround.IsColliding();
    bool risk = RayCastRisk.IsColliding();
    if (ground || risk) {
      // Check obstacles
      bool enemy = false;
      float dist, angle; // Used for the enemies
      // We have to check if we are moving forward the enemy or not
      foreach(var e in enemies) {
        angle = r2d * move.SignedAngleTo(e.GlobalPosition - Player.GlobalPosition, Vector3.Up);
        dist = Player.GlobalPosition.DistanceTo(e.GlobalPosition);
        if (Mathf.Abs(angle) < 50 && dist < 1.5f) {
          enemy = true; // We are not moving away
          rotation.Y = (int)direction; // But we should be able to turn no matter what
          Player.RotationDegrees = rotation;
          break;
        }
      }
      if (enemy) {
        move = Vector3.Zero;
        return false;
      }

      float groundY = -1;
      float riskY = -1;
      if (ground) groundY = RayCastGround.GetCollisionPoint().Y;
      if (risk) riskY = RayCastRisk.GetCollisionPoint().Y;
      if (RayCastObstacles.IsColliding()) {  // To check if we are over a collider
        move = Vector3.Zero;
        resY = Player.Position.Y;
        return false;
      }

      if (groundY > riskY) {
        resY = groundY;
        if (Mathf.Abs(resY - Player.Position.Y) < .6f) {
          return true; // No obstacles and no enemy and not too much difference in terrain level
        }
        if (status != PlayerStatus.Jump && Player.Position.Y > resY + .6f) { // Fall
          SetStatus(PlayerStatus.Fall, Anim.Fall);
          resY = Player.Position.Y;
          return false;
        }
        else resY = Player.Position.Y;
        return true;
      }
      else {
        resY = riskY;
        if (Mathf.Abs(resY - Player.Position.Y) < .6f) {
          return true; // No obstacles and no enemy and not too much difference in terrain level
        }
        if (status != PlayerStatus.Jump && Player.Position.Y > resY + .6f) { // Fall
          SetStatus(PlayerStatus.Fall, Anim.Fall);
          resY = Player.Position.Y;
          return false;
        }
        else resY = Player.Position.Y;
        return true;
      }
    }

    // Check if we should fall to death
    if (Player.Position.Y > 2f) {
      GD.PrintErr("WARNING: Player is over an emmpty place!"); // FIXME
      SetStatus(PlayerStatus.FallToDeath, Anim.Fall);
      resY = Player.Position.Y;
      return false;
    }

    move = Vector3.Zero;
    return false;
  }

  Portal OverPortal() {
    foreach (var p in currentRoom?.Portals) {
      float dist = Player.GlobalPosition.DistanceTo(p.Position);
      if (p.Mesh == null) return null;
      bool over = (dist < 3) && p.RoomIndex != -1;
      p.Mesh.Visible = over; //  true || over; // FIMXE
      if (over && RayCastPickup.IsColliding()) {
        return p;
      }
    }
    return null;
  }
  


  RoomDef currentRoom = null;

  public PackedScene GetScenePack(PartType t) {
    try {
      return Templates[(int)t];
    }
    catch (Exception) {
      return ErrorScene;
    }
  }

  internal PackedScene GetScenePack(ItemType t) {
    return t switch {
      ItemType.Katana => PickabeItems[0],
      ItemType.Nunchaku => PickabeItems[1],
      ItemType.BrassKnuckles => PickabeItems[2],
      ItemType.Shuriken => PickabeItems[3],
      _ => ErrorScene
    };
  }

  Pickable toBePicked = null;

  bool Pickup() {
    // We should check if we can climb or pick something, depending on the type of the Pickable
    RayCastPickup.GlobalPosition = Player.GlobalPosition;
    toBePicked = null;
    if (RayCastPickup.IsColliding()) {
      // We are over an object, we should find what it is
      var obj = RayCastPickup.GetCollider();
      if (obj is Area3D area && area.GetParent() is Pickable pick) {
        switch (pick.ItemType) {
          case ItemType.None:
            GD.PrintErr("Pickable without type!");
            break;
          case ItemType.Katana:
          case ItemType.Nunchaku:
          case ItemType.Shuriken:
          case ItemType.BrassKnuckles:
            SetStatus(PlayerStatus.Grab, Anim.Use);
            toBePicked = pick;
            GetTree().CreateTimer(.4).Timeout += ExecutePickup;
            return true;
          case ItemType.WallStair:
            SetStatus(PlayerStatus.Climb, Anim.Climb);
            var pos = pick.Position - pick.GlobalTransform.Basis.Z * .1f;
            pos.Y = Player.Position.Y;
            if (pos.Y < pick.MinMaxY.X) pos.Y = pick.MinMaxY.X;
            if (pos.Y > pick.MinMaxY.Y) pos.Y = pick.MinMaxY.Y;
            Player.Position = pos;
            Player.Rotation = pick.Rotation;
            interactionItem = pick;
            return true;
        }
      }
      GD.Print("No Pickable!");
    }
    return false;
  }


  void ExecutePickup() {
    if (toBePicked == null) return;
    PlayerSound.Stream = PickupSound;
    PlayerSound.Play();
    switch (toBePicked.ItemType) {
      case ItemType.None: break;

      case ItemType.Katana:
        room.SetCollected(toBePicked.ItemType);
        hasItem[ItemKatana] = true;
        Items[ItemKatana].Visible = true;
        break;

      case ItemType.Nunchaku:
        room.SetCollected(toBePicked.ItemType);
        hasItem[ItemNunchaku] = true;
        Items[ItemNunchaku].Visible = true;
        break;

      case ItemType.Shuriken:
        room.SetCollected(toBePicked.ItemType);
        hasItem[ItemShuriken] = true;
        Items[ItemShuriken].Visible = true;
        break;

      case ItemType.Staff:
        room.SetCollected(toBePicked.ItemType);
        hasItem[ItemStaff] = true;
        Items[ItemStaff].Visible = true;
        break;

      case ItemType.BrassKnuckles:
        room.SetCollected(toBePicked.ItemType);
        hasItem[ItemKnuckles] = true;
        Items[ItemKnuckles].Visible = true;
        break;
    }
    toBePicked.Free();
    toBePicked = null;
  }


  const int ItemNone = 0;
  const int ItemKatana = 1;
  const int ItemNunchaku = 2;
  const int ItemShuriken = 3;
  const int ItemStaff = 4;
  const int ItemKnuckles = 5;
  const int ItemEND = 6;

  double hitDelay = 0;
  float Health = 100;
  int lives = 3; // FIXME we should show them somewhere
  public bool HitPlayer(float strenght, bool handleHit = true) {
    // Reduce health, check if dead
    Health -= strenght;
    if (Health <= 0) {
      Health = 0;
      PowerBarMaterial.SetShaderParameter("Value", 0);
      if (handleHit) hitDelay = 2;
      SetStatus(PlayerStatus.Death, Anim.Dead);
      return true;
    }
    if (handleHit) hitDelay = .5;
    SetStatus(PlayerStatus.Idle, rnd.RandiRange(0, 1) == 0 ? Anim.Hit1 : Anim.Hit2);
    // Play a hit animation stopping controls for half the anim
    return false;
  }

  internal void SetEnemy(Enemy enemy) {
    currentEnemy = enemy;
  }


}

public enum Rot { None = -1, TR = 0, T = 45, TL = 90, L = 135, BL = 180, B = 225, BR = 270, R = 315 };

/*
 Layer 1 => All objects
 Layer 2 => Roads and navmesh
 Layer 3 => Obstacles
 Layer 4 => Areas to interact
 Layer 5 => Navmesh but dangerous (pools, mugd, lava, etc.)
 
 */




