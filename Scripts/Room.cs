using Godot;

namespace LastNinjaRM;

[Tool]
public partial class Room : Node3D {
  [Export] PackedScene PortalMesh;
  Game game;
  RandomNumberGenerator rnd = new();
  [Export] PackedScene[] EnemyPrefabs;

  public override void _Ready() {
    game = GetParent() as Game;
    if (game == null) GD.PrintErr("Cannot find the Game script!");
  }

  Color black = Color.FromHtml("000F");
  Color white = Color.FromHtml("FFFF");
  Color none = Color.Color8(0, 0, 0, 0);

  public override void _Process(double delta) {
    if (Engine.IsEditorHint()) return;
    if (!drawing) return;
    if (room == null) return;

    time += delta;
    if (time < intervalTime) return;
    time = 0;

    if (partNum < room.Parts.Length - 1) {
      // Draw the part
      partNum++;
      var p = room.Parts[partNum];

      var scene = game.GetScenePack(p.Template).Instantiate() as Node3D;
      AddChild(scene);
      scene.Name = $"{partNum}) {p.Template}";
      scene.Position = p.Position;
      scene.RotationDegrees = p.Rotation;
      scene.Scale = p.Scale;
      scene.EditorDescription = p.Template.ToString();
      if (p.Tint != none) {
        if (scene is MeshInstance3D mi) {
          for (int i = 0; i < mi.Mesh.GetSurfaceCount(); i++) {
            var mat = mi.GetActiveMaterial(i);
            if (mat != null && mat is StandardMaterial3D sm) {
              var m = mat.Duplicate() as StandardMaterial3D;
              m.AlbedoColor = (m.AlbedoColor + p.Tint) / 2;
              mi.SetSurfaceOverrideMaterial(i, m);
            }
          }
        }
        else {
          // Check for sub-nodes
          var meshChilren = scene.FindChildren("*", "MeshInstance3D");
          foreach (var mc in meshChilren) {
            if (mc is MeshInstance3D smi) {
              for (int i = 0; i < smi.Mesh.GetSurfaceCount(); i++) {
                var mat = smi.GetActiveMaterial(i);
                if (mat != null && mat is StandardMaterial3D sm) {
                  var m = mat.Duplicate() as StandardMaterial3D;
                  m.AlbedoColor = (m.AlbedoColor + p.Tint) / 2;
                  smi.SetSurfaceOverrideMaterial(i, m);
                }
              }
            }
          }
        }
      }

      return;
    }
    if (itemNum < room.Items.Length - 1) {
      itemNum++;
      var i = room.Items[itemNum];
      if (i.Collected) return;
      Pickable pick = game.GetScenePack(i.Type).Instantiate() as Pickable;
      pick.Name = $"{itemNum}) {i.Type}";
      MeshInstance3D mesh = null;
      mesh = pick.MeshInstance;
      if (mesh == null) {
        GD.PrintErr($"Pickable has no mesh: {pick}");
        return;
      }
      AddChild(pick);
      pick.Position = i.Position;
      pick.RotationDegrees = i.Rotation;
      pick.Scale = Vector3.One * i.Scale;
      var m = (mesh.GetActiveMaterial(0) as StandardMaterial3D).Duplicate() as StandardMaterial3D;
      m.EmissionEnabled = true;
      m.Emission = black;
      mesh.SetSurfaceOverrideMaterial(0, m);
      Tween t = GetTree().CreateTween();
      t.TweenProperty(m, "emission", black, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", white, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", black, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", white, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", black, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", white, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", white, rnd.RandfRange(0.3f, 0.45f));
      t.TweenProperty(m, "emission", black, rnd.RandfRange(0.3f, 0.45f));
      t.TweenCallback(Callable.From(() => { m.EmissionEnabled = false; }));
      return;
    }

    // Create the meshes for the Portals
    foreach (var p in room.Portals) {
      p.Mesh = PortalMesh.Instantiate() as MeshInstance3D;
      p.Mesh.Name = $"{partNum}) {p}";
      AddChild(p.Mesh);
      p.Mesh.Position = p.Position + Vector3.Up * .01f;
      p.Mesh.Scale = p.Scale;
      p.Mesh.Visible = false;
    }

    // Create the enemies
    game.enemies.Clear();
    foreach (var e in room.Enemies) {
      var prefab = EnemyPrefabs[(int)e.type];
      if (prefab == null) {
        GD.PrintErr($"Missing prefab for enemy: {e.type}!");
        continue;
      }
      var enemy = prefab.Instantiate() as Enemy;
      AddChild(enemy);
      enemy.Start(game, e);
      game.enemies.Add(enemy);
    }



    game.Player.Position = portal.Position + portal.dist * portal.rotation switch {
      Rot.TR => new Vector3(0, 0, 1),
      Rot.T => new Vector3(0.707f, 0, 0.707f),
      Rot.TL => new Vector3(1, 0, 0),
      Rot.L =>  new Vector3(0.707f, 0, -0.707f),
      Rot.BL => new Vector3(0, 0, -1),
      Rot.B =>  new Vector3(-0.707f, 0, -0.707f),
      Rot.BR => new Vector3(-1, 0, 0),
      Rot.R =>  new Vector3(-0.707f, 0, 0.707f),
      _ => new Vector3(0, 0, 0),
    } + Vector3.Up * .05f;
    game.Player.RotationDegrees = new Vector3(0, (float)portal.rotation, 0);
    game.Player.Visible = true;
    game.SetIdleStatus();
    drawing = false;
  }

  RoomDef room;
  bool drawing = false;
  double intervalTime = 1;
  double time = -.25;
  int partNum = -1;
  int itemNum = -1;
  Portal portal;

  public void ReBuildRoom() {
    Node3D room = EditorInterface.Singleton.GetEditedSceneRoot().GetNode("Room") as Node3D;
    var strIndex = room.EditorDescription;
    if (!int.TryParse(strIndex, out int index)) {
      GD.PrintErr("Missing index of room in Editor Description! Found: " +strIndex);
      return;
    }

    var def = World.Rooms[index];
    GD.Print("Redrawing room: " + def.RoomName);
    game.SetIdleStatus();

    foreach (var c in room.GetChildren()) {
      c.QueueFree();
    }

    game = room.GetParent() as Game;

    foreach (var p in def.Parts){ 
      // Draw the part
      var scene = game.GetScenePack(p.Template).Instantiate() as Node3D;
      room.AddChild(scene);
      scene.Position = p.Position;
      scene.RotationDegrees = p.Rotation;
      scene.Scale = p.Scale;
      scene.Owner = room.Owner;
      scene.Name = p.Template.ToString();
      scene.EditorDescription = p.Template.ToString();

      if (p.Tint != none) {
        if (scene is MeshInstance3D mi) {
          for (int i = 0; i < mi.Mesh.GetSurfaceCount(); i++) {
            var mat = mi.GetActiveMaterial(i);
            if (mat != null && mat is StandardMaterial3D sm) {
              var m = mat.Duplicate() as StandardMaterial3D;
              m.AlbedoColor = (m.AlbedoColor + p.Tint) / 2;
              mi.SetSurfaceOverrideMaterial(i, m);
            }
          }
        }
        else {
          // Check for sub-nodes
          var meshChilren = scene.FindChildren("*", "MeshInstance3D");
          foreach (var mc in meshChilren) {
            if (mc is MeshInstance3D smi) {
              for (int i = 0; i < smi.Mesh.GetSurfaceCount(); i++) {
                var mat = smi.GetActiveMaterial(i);
                if (mat != null && mat is StandardMaterial3D sm) {
                  var m = mat.Duplicate() as StandardMaterial3D;
                  m.AlbedoColor = (m.AlbedoColor + p.Tint) / 2;
                  smi.SetSurfaceOverrideMaterial(i, m);
                }
              }
            }
          }
        }
      }
    }
    foreach (var i in def.Items) {
      if (i.Collected) return;
      Node3D pick = game.GetScenePack(i.Type).Instantiate() as Node3D;
      if (pick == null) {
        GD.PrintErr($"Pickable is null: {i.Type}");
        continue;
      }

      room.AddChild(pick);
      pick.Position = i.Position;
      pick.RotationDegrees = i.Rotation;
      pick.Scale = Vector3.One * i.Scale;
      pick.Owner = room.Owner;
      pick.Name = i.Type.ToString();
      pick.EditorDescription = i.Type.ToString();
    }

    // Create the meshes for the Portals
    int pnum = 0;
    foreach (var p in def.Portals) {
      if (PortalMesh == null) break;
      p.Mesh = PortalMesh.Instantiate() as MeshInstance3D;
      room.AddChild(p.Mesh);
      p.Mesh.Position = p.Position + Vector3.Up * .01f;
      p.Mesh.Scale = p.Scale;
      p.Mesh.Visible = true;
      p.Mesh.Name = "Portal " + pnum++;
      p.Mesh.Owner = room.Owner;
      p.Mesh.EditorDescription = "Portal";
    }

    // Create the enemies
    game.enemies.Clear();
    foreach (var e in def.Enemies) {
      var prefab = EnemyPrefabs[(int)e.type];
      if (prefab == null) {
        GD.PrintErr($"Missing prefab for enemy: {e.type}!");
        continue;
      }
      var enemy = prefab.Instantiate() as Enemy;
      room.AddChild(enemy);
      enemy.Start(game, e);
      game.enemies.Add(enemy);
    }

    if (Engine.IsEditorHint()) return;

    game.Player.Position = portal.Position + portal.dist * portal.rotation switch {
      Rot.TR => new Vector3(0, 0, 1),
      Rot.T => new Vector3(0.707f, 0, 0.707f),
      Rot.TL => new Vector3(1, 0, 0),
      Rot.L => new Vector3(0.707f, 0, -0.707f),
      Rot.BL => new Vector3(0, 0, -1),
      Rot.B => new Vector3(-0.707f, 0, -0.707f),
      Rot.BR => new Vector3(-1, 0, 0),
      Rot.R => new Vector3(-0.707f, 0, 0.707f),
      _ => new Vector3(0, 0, 0),
    } + Vector3.Up * .05f;
    game.Player.RotationDegrees = new Vector3(0, (float)portal.rotation, 0);
    game.SetIdleStatus();
  }

  public RoomDef BuildRoom(int index, int portal) {
    if (index < 0 || index >= World.Rooms.Length) return room;
    room = World.Rooms[index];
    game.Player.Visible = false;
    game.SetStatusNONE();
    // Clean up previous
    foreach (var item in GetChildren()) {
      item.Free();
    }

    intervalTime = 1 / (room.Parts.Length + 3 * room.Items.Length);
    this.portal = room.Portals[portal];
    partNum = -1;
    itemNum = -1;
    drawing = true;

    return room;
  }

  internal void SetCollected(ItemType itemType) {
    foreach (var item in room.Items) {
      if (!item.Collected && item.Type == itemType) {
        item.Collected = true;
      }
    }
  }
}
