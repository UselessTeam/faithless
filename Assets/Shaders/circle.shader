shader_type canvas_item;

uniform sampler2D colors;
uniform int N;

const float R = 0.75f;
const float SPEED = 4f;

const float TWO_PI = 6.28318530718;
const float HALF_PI = 1.5707963267949;

float frac_at(float theta) {
    float frac = (theta - HALF_PI) / TWO_PI + 1f;
    return mod (frac, 1f);
}

float index_at(float theta) {
    return frac_at(theta) * float(N);
}

vec4 color_at(float theta) {
    return texture(colors,vec2(frac_at(theta), 0));
}

float intensity_at_rand(float value, float t) {
    // return 0f;
    return 0.2f * sin(2f * value + 1.22f * sin(t))
         + 0.2f* sin(3f * value + 1.33f * sin(1.2f * t))
         + 0.3f* sin(5f * value + 1.55f * sin(1.3f * t))
         + 0.3f* sin(7f * value + 1.77f * sin(1.5f * t));
}

float intensity_at_theta(float theta) {
    return max(1.1f * color_at(theta).a - 0.1f, 0f);
}

float intensity_seal(float theta) {
    float value = mod(index_at(theta), 1f);
    value = 1f - 4f * min(value * value, (value - 1f) * (value - 1f));
    value = max(2f * value * value - 1f, 0f);
    return value * value;
}

float ray_d(float r) {
    float p = (R - r);
    p = p * p;
    return p;
}

void fragment() {
    float x = 2f * (UV.x - 0.5f);
    float y = 2f * (UV.y - 0.5f);
    float r = sqrt(x * x + y * y);
    float theta = atan (y, x);
    float abs_intensity = intensity_at_theta(theta);
    abs_intensity *= abs_intensity;
    float thick_intensity = 0.6f + 0.4f * intensity_seal(theta);
    float rand_offset = 0.3f * intensity_at_rand(SPEED * TIME, TIME)
            + 0.35f * intensity_at_rand(theta + SPEED * TIME, TIME)
            + 0.35f * intensity_at_rand(theta - SPEED * TIME, TIME);
    float rand_intensity = 1f + 0.3f * rand_offset;
    float intensity = abs_intensity * thick_intensity * rand_intensity;
    float dd = (1f - 30f * ray_d(r)) * (0.8f + 0.2f * thick_intensity) * abs_intensity;
    intensity += dd;
    vec4 color = color_at(theta + 0.1f * rand_offset);
    if (intensity <= 0.2f) {
        color.a = 0f;
    } else if (0.05f * intensity + 1.02f * dd >= 1f) {
        color.a = 1f;
        color.rgb *= 1.2f;
    } else {
        color.a = max(0, 0.4f * intensity + 0.05f);
    }
    COLOR = color;
}