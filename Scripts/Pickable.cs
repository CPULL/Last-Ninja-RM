using Godot;
using System;

namespace LastNinjaRM;

public partial class Pickable : Node3D {
  [Export] public ItemType ItemType;
  [Export] public MeshInstance3D MeshInstance;
  [Export] public Vector2 MinMaxY;
}
