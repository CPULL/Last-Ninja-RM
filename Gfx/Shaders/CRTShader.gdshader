shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, repeat_disable, filter_nearest;
uniform float pixelsWidth : hint_range(160, 1280) = 640;
uniform bool HalfBrite = true;
uniform bool CRT = true;

const int numColors = 50;

const vec3 c64[numColors] = {
	// Normal colors
	// Half brite (black is replaced by pure green)
vec3(0.000, 0.000, 0.000),
vec3(0.133, 0.133, 0.133),
vec3(0.267, 0.267, 0.267),
vec3(0.400, 0.400, 0.400),
vec3(0.533, 0.533, 0.533),
vec3(0.667, 0.667, 0.667),
vec3(0.800, 0.800, 0.800),
vec3(1.000, 1.000, 1.000),
vec3(0.400, 0.267, 0.000),
vec3(0.678, 0.369, 0.180),
vec3(0.867, 0.533, 0.333),
vec3(1.000, 0.753, 0.796),
vec3(0.078, 0.518, 0.078),
vec3(0.000, 0.800, 0.333),
vec3(0.667, 1.000, 0.400),
vec3(0.859, 1.000, 0.643),
vec3(0.533, 0.000, 0.000),
vec3(0.910, 0.000, 0.024),
vec3(1.000, 0.239, 0.200),
vec3(0.800, 0.267, 0.800),
vec3(0.000, 0.000, 0.667),
vec3(0.408, 0.408, 1.000),
vec3(0.000, 0.533, 1.000),
vec3(0.667, 1.000, 0.933),
vec3(0.820, 0.694, 0.000),
vec3(0.933, 0.933, 0.467),
vec3(1.000, 1.000, 0.000),
vec3(0.208, 0.184, 0.290),
vec3(0.311, 0.244, 0.111),
vec3(0.544, 0.389, 0.295),
vec3(0.722, 0.556, 0.456),
vec3(0.925, 0.801, 0.823),
vec3(0.152, 0.371, 0.152),
vec3(0.189, 0.589, 0.356),
vec3(0.678, 0.844, 0.544),
vec3(0.846, 0.917, 0.739),
vec3(0.356, 0.089, 0.089),
vec3(0.610, 0.156, 0.167),
vec3(0.740, 0.359, 0.340),
vec3(0.711, 0.444, 0.711),
vec3(0.111, 0.111, 0.444),
vec3(0.507, 0.507, 0.803),
vec3(0.256, 0.522, 0.756),
vec3(0.767, 0.933, 0.900),
vec3(0.662, 0.599, 0.252),
vec3(0.856, 0.856, 0.622),
vec3(0.833, 0.833, 0.333),
vec3(0.218, 0.206, 0.259),
vec3(0.161, 0.584, 0.157),
vec3(0.349, 0.729, 0.322)
};

void fragment() {
	vec2 uv = SCREEN_UV;
	float pixelsHeight = pixelsWidth * 9.0 / 16.0;
	vec3 col;
	uv.x = floor(pixelsWidth * uv.x) / pixelsWidth;
	float vpos = floor(pixelsHeight * uv.y); 
	uv.y = vpos / pixelsHeight;
	col = texture(SCREEN_TEXTURE, uv).rgb;	

	// Find the closest C64 color, we do 4 passes
	// 1 normal C64 colors, 1 half brite c64 colors, 2 half saturation, half sat and bright
	int colIndex = 0; // 0..15 Normal, 16..31 halfb, 32..47 halfs, 48..63 halved
	float dist = 100000.0;
	vec3 res;
	for(int i=0; i<numColors; i++) {
		float d = distance(col, c64[i]);
		if (d < dist) {
			dist = d;
			res = c64[i];
		}
	}
	for(int i=0; HalfBrite && i<numColors; i++) {
		vec3 half = c64[i] * 0.5;
		float d = distance(col, half);
		if (d < dist) {
			dist = d;
			res = half;
		}
	}
	
	if (CRT && int(vpos) % 4 == 0) res *= 1.2;

	COLOR = vec4(res, 1.0);
}
