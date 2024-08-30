using Godot;

public partial class NinjaHead : TextureRect {
  AtlasTexture atlas;
  const int w = 512;
  const int h = 460;
  RandomNumberGenerator rnd = new();

  Rect2 Look =    new(new(0, 0), new(w, h));
  Rect2 LookL =   new(new(w, 0), new(w, h));
  Rect2 LookR =   new(new(w*2, 0), new(w, h));
  Rect2 LookD =   new(new(0, h), new(w, h));
  Rect2 BUp =     new(new(w, h), new(w, h));
  Rect2 BDwn =    new(new(w*2, h), new(w, h));
  Rect2 Blink1 =  new(new(0, h*2), new(w, h));
  Rect2 Blink2 =  new(new(w, h*2), new(w, h));
  Rect2 UNK = new(new(w * 2, h * 2), new(w, h));

  public override void _Ready() {
    atlas = Texture as AtlasTexture;
    delay = rnd.RandfRange(1, 3);
  }

  double delay = 0;
  int blinkStep = -1;
  int prev = -1;
  public override void _Process(double delta) {
    if (delay > 0) {
      delay -= delta;
      return;
    }
    if (blinkStep > -1) {
      blinkStep++;
      switch (blinkStep) {
        case 1:
          atlas.Region = Blink2;
          delay = .15f;
          break;
        case 2: 
          atlas.Region = Blink1;
          delay = .05f;
          break;

        case 3: 
          atlas.Region = Look;
          delay = 1;
          blinkStep = -1;
          break;
      }
      return;
    }

    int r = rnd.RandiRange(0, 3);
    if (r == prev) r = (r + 1) % 4;
    prev = r;
    switch (r) {
      case 0:
        atlas.Region = Look;
        delay = rnd.RandfRange(1, 3);
        break;

      case 1:
        atlas.Region = LookL;
        delay = rnd.RandfRange(1, 2);
        break;

      case 2:
        atlas.Region = LookR;
        delay = rnd.RandfRange(1, 2);
        break;

      case 3:
        blinkStep = 0;
        atlas.Region = Blink1;
        delay = .07f;
        break;
    }
  }

}
