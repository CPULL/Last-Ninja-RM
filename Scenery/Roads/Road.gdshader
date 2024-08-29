shader_type spatial;

uniform sampler2D RoadTexture;
uniform sampler2D NormalTexture;

void fragment() {
	vec2 atlas_size = vec2(6,10);
	vec3 positionWS = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec2 wuv = positionWS.xz * 0.2;
 	vec2 uv = vec2(fract(wuv.x), fract(wuv.y));	
	
	vec2 wuv2 = positionWS.xz * 0.3;
 	vec2 uv2 = vec2(fract(wuv2.x), fract(wuv2.y));	

	ALBEDO = texture(RoadTexture, uv).rgb * .4;
	NORMAL_MAP = texture(NormalTexture, uv2).rgb * .5;
}
