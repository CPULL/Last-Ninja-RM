using Godot;

namespace LastNinjaRM;

public partial class World {
  public const int OutsideTheGarden = 0;
  public const int GardenEntrance = 1;
  public const int GardenSouth = 2;
  public const int GardenNorth = 3;
  public const int GardenNorthPoolWall = 4;
  public const int GardenPoolWall = 5;
  public const int GardenXYZ = 5;

  public static readonly RoomDef[] Rooms = { // Make it callable to reset the values
    new( "Outside the Garden", new RoomPartDef[] { // ########## 0
      new(PartType.Grass1,  0, 0, 0,        32, 35, 1,    -90, 45, 0     ),
      new(PartType.Road1,   0, 0.001f, 0,   44, 4, 1,     -90, 90, 0     ),

      new(PartType.Grass2,  -5.7f, 0, -6.7f,    1, 1, 1,     0, 0, 0     ),
      new(PartType.Grass2,  -7.2f, 0, -2.73f,   1, 1, 1,     0, 0, 0     ),
      new(PartType.Grass2,  -4.7f, 0, 0.5f,     1, 1, 1,     0, 0, 0     ),
      new(PartType.Grass2,  -8.7f, 0, 1.97f,    1, 1, 1,     0, 0, 0     ),
      new(PartType.Grass2,  -5.68f, 0, 5.37f,   1, 1, 1,     0, 0, 0     ),

      new(PartType.Tree1,  2.6f, 0, -10.9f,   .1f, .1f, .1f,      90, 0, 0       ),
      new(PartType.Tree1,  2.98f, 0, -6.35f,  .12f, .12f, .12f,   90, -75, 0     ),
      new(PartType.Tree1,  3.75f, 0, -1.48f,  .1f, .1f, .1f,      90, 45, 0     ),
      new(PartType.Tree1,  3.37f, 0, 4.5f,    .13f, .15f, .13f,   90, 82, 0     ),

      new(PartType.Gate1,  0, 0, 10.5f,     1, 1, 1,   0, 0, 0     ),
      new(PartType.Fence1,  -7, 0, 10.5f,   1, 1, 1,   0, 0, 0     ),
      new(PartType.Fence1,  7, 0, 10.5f,   1, 1, 1,   0, 0, 0      ),


    }, new Portal[] {
      new(0f, 0, -13.5f, 2,1,    Rot.TR, 1f,   -1, 0),
      new(0f, 0, 16f,    2,1,    Rot.BL, 1f,   GardenEntrance, 0)
    }, System.Array.Empty<ItemDef>(), System.Array.Empty<EnemyDef>()),

    new( "Garden Entrance", new RoomPartDef[] { // ########## 1
      new(PartType.Grass1,  -1, 0, -1,         33, 35, 1,    -90, 45, 0     ),
      new(PartType.Road1,   0, 0.001f, -13,  22, 4, 1,     -90, 90, 0       ),
      new(PartType.Road1,   0, 0.001f, 0,    44, 4, 1,     -90, 0, 0        ),

      new(PartType.Grass2,  -4.09f, 0, -11.49f,  1, 1, 1,     0, 0, 0    ),
      new(PartType.Grass2,  -3.45f, 0, -7.39f,   1, 1, 1,     0, 0, 0    ),
      new(PartType.Grass2,  -3.90f, 0, -3.86f,   1, 1, 1,     0, 0, 0    ),
      new(PartType.Grass2,  -7.71f, 0, -7.85f,   1, 1, 1,     0, 0, 0    ),
      new(PartType.Grass2,  -9.25f, 0, -4.35f,   1, 1, 1,     0, 0, 0    ),

      new(PartType.Tree1,  8f, 0, -7f,        .10f, .11f, .10f,     90, -103, 0   ),
      new(PartType.Tree1,  2.5f, 0, -10.5f,   .10f, .10f, .10f,     90, -17, 0    ),
      new(PartType.Tree1,  3.5f, 0, -5f,      .13f, .15f, .13f,     90, 108, 0    ),

      new(PartType.Tree2,  1.75f, 0, 6.36f,     1f, 1f, 1f,         0, 0, 0     ),
      new(PartType.Tree2,  -3.16f, 0, 7.82f,    1.5f, 1.26f, .8f,   0, 69, 0    ),
      new(PartType.Tree2,  -10.9f, 0, 3.0f,     1.36f, 1f, 1.21f,   0, 20, 0    ),

      new(PartType.Rock1,  5.69f, 0, 3f,      1, 1, 1,          0, 0, 0    ),
      new(PartType.Rock2,  3.36f, 0, 3.1f,    1, 1, 1,          0, 0, 0    ),
      new(PartType.Rock2,  -3.23f, 0, 3.11f,  2, .74f, 1.29f,   0, 18, 0   ),

      new(PartType.Table1,  0f, 0, 3.1f,  1, 1, 1,   0, 0, 0    ),


    }, new Portal[] {
      new(-.1f, 0.1f, -15f,  2f,1,    Rot.TR, 1f,      OutsideTheGarden, 1),
      new(-14.5f, 0, 0f,      1,2,    Rot.TL, 1f,      GardenSouth, 0),
      new(14.5f, 0, 0f,     1.25f,2,  Rot.BR, 1.5f,    GardenNorth, 0)
    }, new ItemDef[] {
      new(ItemType.Katana,    -0.75f, .95f, 3f,     0, 110f, 0,    2f),
    }, System.Array.Empty<EnemyDef>()),

    new( "Garden South", new RoomPartDef[] { // ########## 2
    new(PartType.Grass1,  -1, 0, -1,         33, 35, 1,    -90, 45, 0),
    new(PartType.Road1,           0.325f,   0.001f,  -7.184f,      21.97f, 4.00f, 1.00f,       -90, -180, 0 ),
    new(PartType.Road1,          -7.832f,   0.001f,   4.552f,      27.65f, 6.00f, 1.00f,       -90, 90, 0   ),
    new(PartType.Grass2,         -0.906f,   0.000f, -15.198f,      1.00f, 1.00f, 1.00f,        0, 0, 0      ),
    new(PartType.Grass2,         -0.371f,   0.000f, -10.913f,      1.00f, 1.00f, 1.00f,        0, 0, 0      ),
    new(PartType.Grass2,         -5.555f,   0.000f, -11.913f,      1.00f, 1.00f, 1.00f,        0, 0, 0      ),
    new(PartType.Grass2,        -12.269f,   0.000f,  -5.694f,      1.00f, 1.00f, 1.00f,        0, 0, 0      ),
    new(PartType.Tree1,         -18.295f,   0.000f,   0.889f,      0.10f, 0.10f, 0.10f,        90, 147, 0   ),
    new(PartType.Tree1,         -16.908f,   0.443f,  -3.581f,      0.10f, 0.10f, 0.10f,        90, 65, 0    ),
    new(PartType.Tree2,           7.063f,   0.000f,  -2.292f,      1.00f, 1.00f, 1.00f,        0, 0, 0      ),
    new(PartType.Tree2,           0.238f,   0.000f,  -3.113f,      1.90f, 1.70f, 1.00f,        0, 165, 0    ),
    new(PartType.Tree2,          -4.034f,   1.298f,   3.551f,      1.36f, 1.00f, 1.21f,        0, -22, 0    ),
    new(PartType.Tree2,          -3.779f,   1.298f,  10.348f,      1.36f, 1.00f, 1.21f,        0, -22, 0    ),
    new(PartType.Tree2,           2.473f,   1.298f,   3.260f,      1.36f, 1.00f, 1.21f,        0, -22, 0    ),
    new(PartType.Rock1,          -4.242f,   0.000f,   8.704f,      1.00f, 1.00f, 1.00f,        0, 5, 0      ),
    new(PartType.Rock2,           3.936f,   0.000f,  -3.261f,      2.00f, 0.20f, 2.00f,        0, 0, 0      ),
    new(PartType.Rock2,          -3.682f,   0.000f,   0.006f,      1.95f, 0.74f, 1.29f,        0, 61, 0     ),
    new(PartType.StandingBox,    -5.502f,   0.000f,  -3.900f,      1.00f, 1.00f, 1.00f,        0, 260, 0    ),

    }, new Portal[] {
      new(8f, 0.1f, -7.2f,  2f,2f,    Rot.BR, 2.2f,    GardenEntrance, 1),
      new(-7.84f, 0.1f, 12f,  3,3,    Rot.TR, 1f,      999, 0)
    }, new ItemDef[] { new(ItemType.Nunchaku,  2.7f, .1f, -4.966f,    0,0,0, 1f)  },
    new EnemyDef[] { new(EnemyDef.EnemyType.BadNinjaPunch, -5.58f, 0.017f, -3.818f, -105, 8, 3.8f, 1f) }),

    new( "Garden North", new RoomPartDef[] {  // ########## 3

      new(PartType.Grass1,         -1.000f,   0.000f,  -1.000f,      34.00f, 34.00f, 2.27f,      -90, 45, 0        ),
      new(PartType.Road1,          -8.764f,   0.001f,  -7.184f,      25.34f, 4.00f, 1.00f,       -90, -180, 0      ),
      new(PartType.Road1,           4.000f,   0.029f,   6.455f,      31.372f, 4.00f, 1.00f,      -90, 90, 0        ),
      new(PartType.Fence1,        -17.481f,   0.000f,  -4.731f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Fence1,        -12.419f,   0.000f,  -4.731f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Fence1,         -7.419f,   0.000f,  -4.731f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Grass2,         -2.874f,   0.000f, -16.495f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Grass2,          2.316f,   0.000f, -12.872f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Grass2,         -2.845f,   0.000f, -11.416f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Grass2,         -8.914f,   0.000f, -11.633f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Grass2,         -7.154f,   0.000f, -14.662f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Rock2,          -6.954f,   0.000f,   8.704f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Rock2,           0.863f,   0.000f,  -3.995f,      2.00f, 0.20f, 2.00f,        0, -68, 0,       .3f, .4f, .35f),
      new(PartType.Rock2,           0.449f,  -0.492f,   0.865f,      4.58f, 1.67f, 1.29f,        0, -99, 0         ),
      new(PartType.Tree1,           8.740f,   0.000f,  -2.970f,      0.10f, 0.10f, 0.10f,        90, 147, 0,        .6f, .65f, .55f),
      new(PartType.Tree1,           8.071f,   0.443f,   1.847f,      0.10f, 0.10f, 0.10f,        90, 65, 0         ),
      new(PartType.Tree2,          -9.217f,   0.000f,   2.134f,      1.00f, 1.00f, 1.00f,        0, 0, 0           ),
      new(PartType.Tree2,          -5.531f,   0.601f,  -1.851f,      1.90f, 1.70f, 1.00f,        0, -92, 0         ),
      new(PartType.Tree2,          -6.446f,   1.298f,   3.551f,      1.36f, 1.00f, 1.21f,        0, -22, 0,        .5f, .9f, .1f),
      new(PartType.Tree2,         -16.140f,   1.298f,  -3.009f,      1.36f, 1.00f, 1.21f,        0, -22, 0         ),

    }, new Portal[] {
      new(-17.198f,   0.053f,  -7.205f,     1.96f, 2.00f,   Rot.TL, 2,    GardenEntrance, 2),
      new(  4.000f,   0.053f,  13.486f,     2.00f, 2.00f,   Rot.BL, 2,    GardenNorthPoolWall, 0),
    }, System.Array.Empty<ItemDef>(), System.Array.Empty<EnemyDef>()),

    new( "Garden North Pool", new RoomPartDef[] {  // ########## 4
      new(PartType.Grass1,         -1.000f,   0.000f,  -1.000f,            34.00f, 37.40f, 2.27f,      -90, 45, 0            ),
      new(PartType.Road1,           4.250f,   0.029f,   0.000f,            40.00f, 4.00f, 1.00f,       -90, 90, 0            ),
      new(PartType.Pool,           -3.510f,   0.010f,  -2.043f,            6.00f, 1.00f, 5.00f,        0, 0, 0               ),
      new(PartType.Road1,         -10.112f,   0.005f,   2.880f,            4.20f, 5.00f, 1.00f,        -90, 0, 0             ),
      new(PartType.Wall1,          -2.719f,   1.500f,   1.776f,            10.00f, 3.00f, 1.00f,       0, -170, 0            ),
      new(PartType.Road2,          -2.635f,   2.950f,   3.292f,            10.50f, 3.00f, 1.00f,       -90, 10, 0            ),
      new(PartType.Wall1,         -12.553f,   1.500f,   2.715f,            10.00f, 3.00f, 1.00f,       0, -180, 0            ),
      new(PartType.Road2,         -12.432f,   2.951f,   4.243f,            10.00f, 3.00f, 1.00f,       -90, 0, 0             ),
      new(PartType.Fence1,          7.400f,   0.000f,  -8.691f,            1.00f, 1.00f, 1.00f,        0, 90, 0              ),
      new(PartType.Fence1,          7.400f,   0.000f,   0.403f,            1.00f, 1.00f, 1.00f,        0, 90, 0              ),
      new(PartType.Fence1,          7.400f,   0.000f,  10.001f,            1.00f, 1.00f, 1.00f,        0, 90, 0              ),
      new(PartType.Fence1,          1.914f,   3.000f,   2.221f,            0.40f, 0.50f, 0.50f,        0, 98, 0              ),
      new(PartType.Grass2,         -2.962f,   0.000f, -18.453f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,          0.331f,   0.000f, -14.371f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,        -10.314f,   0.066f, -12.776f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,         -7.077f,   0.200f, -15.035f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Rock3,           0.226f,  -0.079f,  -4.092f,            1.50f, 1.50f, 1.50f,        0, -10, 0             ),
      new(PartType.Rock3,          -1.649f,  -0.079f,  -0.753f,            1.50f, 1.50f, 1.50f,        0, 129, 0             ),
      new(PartType.Rock3,          -5.601f,  -0.079f,   0.070f,            1.50f, 1.50f, 1.50f,        0, -10, 0             ),
      new(PartType.Rock3,          -7.607f,  -0.079f,  -2.478f,            1.50f, 1.50f, 1.50f,        0, 5, 0               ),
      new(PartType.Rock3,          -4.526f,  -0.079f,  -4.544f,            1.50f, 1.50f, 1.50f,        0, 26, 0              ),
      new(PartType.Tree1,          12.154f,   0.000f,  -0.664f,            0.10f, 0.10f, 0.10f,        90, -154, 0           ),
      new(PartType.Tree1,          10.549f,   0.443f,   3.491f,            0.10f, 0.10f, 0.10f,        90, 148, 0            ),
      new(PartType.Tree2,         -13.016f,   0.000f, -11.120f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Tree2,          -3.083f,   0.601f, -15.495f,            1.90f, 1.70f, 1.00f,        0, -92, 0             ),
      new(PartType.WallStair,     -10.325f,   0.000f,   2.695f,            3.00f, 3.00f, 3.00f,        0, 0, 0               ),
    }, new Portal[] {
      new(  4.264f,   0.063f,  -9.600f,            1.96f, 2.00f,        Rot.TR, 2,     GardenNorth, 1),
      new(  4.250f,   0.063f,  15.700f,            2.00f, 2.00f,        Rot.BL, 2,     GardenXYZ, 0),
      new(-13.500f,   2.951f,   4.236f,            1.50f, 1.50f,        Rot.TL, 2f,    GardenPoolWall, 0),
    }, System.Array.Empty<ItemDef>(), System.Array.Empty<EnemyDef>()),

    new( "Garden Pool Wall", new RoomPartDef[] {  // ########## 5
      new(PartType.Grass1,         -1.000f,   0.000f,  -1.000f,            34.00f, 38.00f, 1.00f,      -90, 45, 0            ),
      new(PartType.Wall1,         -11.998f,   1.500f,  -4.042f,            3.00f, 3.00f, 1.00f,        0, -90, 0             ),
      new(PartType.Wall1,          -6.969f,   1.500f,  -5.507f,            10.00f, 3.00f, 1.00f,       0, -180, 0            ),
      new(PartType.Road2,          -7.000f,   2.959f,  -4.000f,            10.00f, 3.00f, 1.00f,       -90, 0, 0             ),
      new(PartType.Wall1,           0.049f,   1.500f,  -4.059f,            5.00f, 3.00f, 1.00f,        0, 146, 0             ),
      new(PartType.Road2,          -0.018f,   2.951f,  -2.546f,            6.50f, 2.62f, 1.00f,        -90, -34, 0           ),
      new(PartType.Wall1,           8.907f,   1.500f,  -2.693f,            14.00f, 3.00f, 1.00f,       0, -180, 0            ),
      new(PartType.Road2,           8.955f,   2.950f,  -1.173f,            14.00f, 3.00f, 1.00f,       -90, 0, 0             ),
      new(PartType.Fence1,        -10.862f,   3.000f,  -3.942f,            0.40f, 0.50f, 0.50f,        0, 92, 0              ),
      new(PartType.Table1,         -3.883f,   0.000f, -10.734f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,          7.158f,   0.000f,  -8.835f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,          0.142f,   0.000f,  -9.466f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,         -6.543f,   0.066f,  -7.965f,            1.00f, 1.00f, 1.00f,        0, 0, 0               ),
      new(PartType.Grass2,        -15.122f,   0.200f,  -7.988f,            1.00f, 1.00f, 1.00f,        0, -34, 0             ),
      new(PartType.Grass2,          8.166f,   0.000f,  -4.049f,            1.00f, 1.00f, 1.00f,        0, 48, 0              ),
      new(PartType.Grass2,          2.391f,   0.000f, -12.281f,            1.00f, 1.00f, 1.00f,        0, -170, 0            ),
      new(PartType.Grass2,         -5.402f,   0.000f, -14.428f,            1.00f, 1.00f, 1.00f,        0, 88, 0              ),
      new(PartType.Grass2,        -10.809f,   0.066f, -11.344f,            1.00f, 1.00f, 1.00f,        0, -89, 0             ),
      new(PartType.Grass2,        -18.139f,   1.939f,  -5.019f,            1.00f, 1.00f, 1.00f,        0, 73, 0              ),
      new(PartType.Grass2,          3.591f,   0.000f,  -6.173f,            1.00f, 1.00f, 1.00f,        0, 36, 0              ),
      new(PartType.Tree2,          -3.566f,   0.000f,   3.316f,            1.00f, 1.93f, 1.22f,        0, -45, 0             ),
      new(PartType.Tree2,           4.792f,   0.601f,   4.762f,            1.90f, 1.70f, 1.00f,        0, 10, 0              ),
      new(PartType.Tree2,         -11.927f,   0.601f,   4.678f,            1.90f, 1.70f, 1.00f,        0, 97, 0              ),
    }, new Portal[] {
      new(14.450f,   2.971f,  -1.187f,            1.50f, 1.50f,       Rot.BR, 2,     GardenNorthPoolWall, 2),
    }, new ItemDef[] {

      new(ItemType.BrassKnuckles, -10.979f,   3.037f,  -3.891f,        0, 17, 0,     1f),

    }, System.Array.Empty<EnemyDef>()),

    new( "DEBUG", new RoomPartDef[] {  // ########## 6
      new(PartType.Grass1,          0.000f,   0.000f,   0.000f,      34.00f, 37.40f, 2.27f,      -90, 45, 0,     .3f, .3f, .3f),
      new(PartType.Road1,           0.000f,   0.001f,   0.000f,      10.00f, 10.00f, 1.00f,      -90, 90, 0            ),
      new(PartType.Road2,           1.000f,   3.000f,   3.000f,       3.00f, 3.00f, 1.00f,       -90, 0, 0,       .1f,.1f,1f),
      new(PartType.WallStair,       1.000f,   0.000f,   1.634f,      3.00f, 3.00f, 3.00f,        0, 180, 0             ),

    }, new Portal[] {
      new(-4.0f,   0f,  -4f,      1f, 1f,   Rot.TL, 1,    GardenXYZ, 0), // FIXME
    }, System.Array.Empty<ItemDef>(), System.Array.Empty<EnemyDef>()),







  };
}


public class RoomDef {
  public string RoomName;
  public RoomParts[] Parts;
  public Portal[] Portals;
  public ItemDef[] Items;
  public EnemyDef[] Enemies;

  // FIXME we need to add the connection points, a Vec3 where the player can enter/exit, linked with a Vec2i with room index, portal index
  // Total can be a v3, float for rotation, int index room, int index portal

  public RoomDef(string n, RoomPartDef[] parts, Portal[] ps, ItemDef[] itms, EnemyDef[] enemies) {
    RoomName = n;
    Parts = new RoomParts[parts.Length];
    for (int i = 0; i < parts.Length; i++) {
      Parts[i] = new RoomParts(parts[i]);
    }
    Portals = ps;
    Items = itms;
    Enemies = enemies;
  }
}

public class Portal {
  public Vector3 Position; // Position for the portal, the player to appear close (according to the rotation)
  public Vector3 Scale; // Scale for the mesh
  public Rot rotation; // Rotation for the player when it will enter
  public float dist; // Distance from the portal for the player when it will enter
  public int RoomIndex, PortalIndex; // Corresponding room and portal
  public MeshInstance3D Mesh;

  public Portal() { }

  public Portal(float x, float y, float z, float sx, float sz, Rot r, float d,  int room, int port) {
    Position = new(x, y, z);
    Scale = new(sx, 1, sz);
    rotation = r;
    dist = d;
    RoomIndex = room;
    PortalIndex = port;
  }
}

public class RoomPartDef{
  public PartType t;
  public float px, py, pz;
  public float sx, sy, sz;
  public float rx, ry, rz;
  public float cr, cg, cb;

  public RoomPartDef() { }

  public RoomPartDef(PartType t, float px, float py, float pz, float sx, float sy, float sz, float rx, float ry, float rz, float cr, float cg, float cb) {
    this.t = t;
    this.px=px; this.py=py; this.pz=pz;
    this.sx=sx; this.sy=sy; this.sz=sz;
    this.rx=rx; this.ry=ry; this.rz=rz;
    this.cr=cr; this.cg=cg; this.cb=cb;
  }
  public RoomPartDef(PartType t, float px, float py, float pz, float sx, float sy, float sz, float rx, float ry, float rz) {
    this.t = t;
    this.px=px; this.py=py; this.pz=pz;
    this.sx=sx; this.sy=sy; this.sz=sz;
    this.rx=rx; this.ry=ry; this.rz=rz;
    cr=-1; cg=-1; cb=-1;
  }
}

public partial class RoomParts : Node {
  public PartType Template;
  public Vector3 Position;
  public Vector3 Scale;
  public Vector3 Rotation;
  public Color Tint;
  public RoomParts() { }
  public RoomParts(RoomPartDef d) {
    Template = d.t;
    Position = new(d.px, d.py, d.pz);
    Scale = new(d.sx, d.sy, d.sz);
    Rotation = new(d.rx, d.ry, d.rz);
    if (d.cr != -1) Tint = new(d.cr, d.cg, d.cb);
    else Tint = Color.Color8(0, 0, 0, 0);
  }
}


public enum PartType {
  Road1, Road2, Road3, Road4,
  Grass1, Grass2, Grass3, Grass4,
  Bush1, Bush2, Bush3, Bush4,
  Tree1, Tree2, Tree3, Tree4,
  Rock1, Rock2, Rock3, Rock4,
  Gate1, Gate2, Gate3, Gate4,
  Fence1, Fence2, Fence3, Fence4,
  Table1, Table2, Table3, Table4,
  StandingBox, LightPole,
  Pool,
  Wall1, Wall2, Wall3, Wall4,
  WallStair
}

public class ItemDef {
  public ItemType Type;
  public Vector3 Position;
  public Vector3 Rotation;
  public float Scale;
  public bool Collected = false;

  public ItemDef(ItemType t, float x, float y, float z, float rx, float ry, float rz, float s) {
    Type = t;
    Position = new(x, y, z);
    Rotation = new(rx, ry, rz);
    Scale = s;
  }
}

public class EnemyDef {
  public EnemyType type;
  public Vector3 Position;
  public float Rotation;
  public float AttackDistance;
  public float Speed;
  public float Strenght;
  public bool Defeated;
  public Vector4 DeathPos;

  public EnemyDef(EnemyType t, float x, float y, float z, int r, int ad, float sp, float st) {
    type = t;
    Position = new(x, y, z);
    Rotation = r;
    AttackDistance = ad;
    Speed = sp;
    Strenght = st;
    Defeated = false;
  }

  public enum EnemyType { BadNinjaPunch };
}

public enum ItemType {
  None, Katana, Nunchaku, Shuriken, WallStair, BrassKnuckles, Staff
}