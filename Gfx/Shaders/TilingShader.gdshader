shader_type spatial;

uniform sampler2D Albedo1;
uniform sampler2D Albedo2;
uniform sampler2D NormalMap;
uniform float normalStrenght = 1.0;


void fragment() {
	vec2 atlas_size = vec2(6,10);
	vec3 positionWS = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec2 wuv = positionWS.xz * 0.2;
 	vec2 uv = vec2(fract(wuv.x), fract(wuv.y));

	vec2 wuv2 = positionWS.xz * 0.3;
 	vec2 uv2 = vec2(fract(wuv2.x), fract(wuv2.y));


	ALBEDO = (texture(Albedo1, uv).rgb + texture(Albedo2, uv2).rgb) * 0.2;
	NORMAL_MAP = texture(NormalMap, uv2).rgb * normalStrenght;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
