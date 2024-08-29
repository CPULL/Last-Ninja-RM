shader_type spatial;

uniform sampler2D Watertxt;
uniform float speed : hint_range(.1, 5, .1) = 1.0;


void fragment() {
	vec2 uv = UV;
	float str = (sin(TIME * 3.0)+1.0)*.01 + 1.0;
	uv.x = (uv.x-.5) * str + .5;
	uv.y = (uv.y-.5) * str + .5;

	ALBEDO = texture(Watertxt, uv).rgb * .3;
	float x = UV.x-.5;
	float y = UV.y-.5;
	if (x*x*x+y*y*y > .25 * str) ALPHA=0.0;
}
