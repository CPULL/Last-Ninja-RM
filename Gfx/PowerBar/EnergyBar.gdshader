shader_type canvas_item;

uniform sampler2D BarTxt;
uniform float Value : hint_range(0.0, 1.0, 0.001);
uniform vec4 Col;
const float steps = 19.0;
const vec4 z = vec4(0,0,0,0);
const float uvW = 0.1;

void fragment() {
	float partNum = floor(Value * steps);
	float sub = fract(Value * steps);
	COLOR = z; // Transparent by default

	if (partNum < 1.0) {
		if (UV.x <= 0.1 * 1.0 && UV.y > 1.0 - sub) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 2.0) {
		if (UV.x <= 0.1 * 1.0) COLOR = texture(BarTxt, UV);
		else if (UV.y < 0.5 && UV.x <= 0.1 * (1.0 + sub)) COLOR = texture(BarTxt, UV);
	}

	else if (partNum < 3.0) { // BD
		if (UV.x <= 0.1 * 1.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 2.0 && UV.y <= 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 2.0 && UV.y <= 0.5 + 0.5 * sub) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 4.0) { // BR
		if (UV.x <= 0.1 * 2.0) COLOR = texture(BarTxt, UV);
		else if (UV.y >= 0.5 && UV.x <= 0.1 * (2.0 + sub)) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 5.0) { // TU
		if (UV.x <= 0.1 * 2.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 3.0 && UV.y > 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 3.0 && UV.y > 0.5 - 0.5 * sub) COLOR = texture(BarTxt, UV);

	}
	else if (partNum < 6.0) { // TR
		if (UV.x <= 0.1 * 3.0) COLOR = texture(BarTxt, UV);
		else if (UV.y < 0.5 && UV.x <= 0.1 * (3.0 + sub)) COLOR = texture(BarTxt, UV);
	}


	else if (partNum < 7.0) { // BD
		if (UV.x <= 0.1 * 3.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 4.0 && UV.y <= 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 4.0 && UV.y <= 0.5 + 0.5 * sub) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 8.0) { // BR
		if (UV.x <= 0.1 * 4.0) COLOR = texture(BarTxt, UV);
		else if (UV.y >= 0.5 && UV.x <= 0.1 * (4.0 + sub)) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 9.0) { // TU
		if (UV.x <= 0.1 * 4.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 5.0 && UV.y > 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 5.0 && UV.y > 0.5 - 0.5 * sub) COLOR = texture(BarTxt, UV);

	}
	else if (partNum < 10.0) { // TR
		if (UV.x <= 0.1 * 5.0) COLOR = texture(BarTxt, UV);
		else if (UV.y < 0.5 && UV.x <= 0.1 * (5.0 + sub)) COLOR = texture(BarTxt, UV);
	}


	else if (partNum < 11.0) { // BD
		if (UV.x <= 0.1 * 5.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 6.0 && UV.y <= 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 6.0 && UV.y <= 0.5 + 0.5 * sub) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 12.0) { // BR
		if (UV.x <= 0.1 * 6.0) COLOR = texture(BarTxt, UV);
		else if (UV.y >= 0.5 && UV.x <= 0.1 * (6.0 + sub)) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 13.0) { // TU
		if (UV.x <= 0.1 * 6.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 7.0 && UV.y > 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 7.0 && UV.y > 0.5 - 0.5 * sub) COLOR = texture(BarTxt, UV);

	}
	else if (partNum < 14.0) { // TR
		if (UV.x <= 0.1 * 7.0) COLOR = texture(BarTxt, UV);
		else if (UV.y < 0.5 && UV.x <= 0.1 * (7.0 + sub)) COLOR = texture(BarTxt, UV);
	}


	else if (partNum < 15.0) { // BD
		if (UV.x <= 0.1 * 7.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 8.0 && UV.y <= 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 8.0 && UV.y <= 0.5 + 0.5 * sub) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 16.0) { // BR
		if (UV.x <= 0.1 * 8.0) COLOR = texture(BarTxt, UV);
		else if (UV.y >= 0.5 && UV.x <= 0.1 * (8.0 + sub)) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 17.0) { // TU
		if (UV.x <= 0.1 * 8.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 9.0 && UV.y > 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 9.0 && UV.y > 0.5 - 0.5 * sub) COLOR = texture(BarTxt, UV);

	}
	else if (partNum < 18.0) { // TR
		if (UV.x <= 0.1 * 9.0) COLOR = texture(BarTxt, UV);
		else if (UV.y < 0.5 && UV.x <= 0.1 * (9.0 + sub)) COLOR = texture(BarTxt, UV);
	}
	else if (partNum < 19.0) {
		if (UV.x <= 0.1 * 9.0) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 10.0 && UV.y <= 0.5) COLOR = texture(BarTxt, UV);
		else if (UV.x <= 0.1 * 10.0 && UV.y <= 0.5 + 0.5 * sub) COLOR = texture(BarTxt, UV);
	}
	else COLOR = texture(BarTxt, UV); // Case of 100%
}
