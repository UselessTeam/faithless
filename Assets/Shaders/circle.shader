shader_type canvas_item;

uniform sampler2D colors;
uniform int N;
uniform float seal;

const float R = 0.75f;
const float SPEED = 3f;

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
    return max(1.2f * color_at(theta).a - 0.2f, 0f);
}

float intensity_seal(float theta) {
    float value = mod(index_at(theta), 1f);
    value = 1f - 4f * min(value * value, (value - 1f) * (value - 1f));
    value = max(2f * value * value - 1f, 0f);
    return value * value;
}

float ray_dd(float r) {
    float p = (R - r);
    p = max(1f - 30f * p * p, 0f);
    return p * p;
}

float seal_dd(float v, float r) {
    float value = v * v;
    if (r <= 0f || r >= R) {
        return 0f;
    }
    return (r + seal) / (R + seal) * value;
}

void fragment() {
    float x = 2f * (UV.x - 0.5f);
    float y = 2f * (UV.y - 0.5f);
    float r = sqrt(x * x + y * y);
    float theta = atan (y, x);
    float abs_intensity = 1f - intensity_at_theta(theta);
    abs_intensity = 1f - abs_intensity * abs_intensity;
    float seal_intensity = intensity_seal(theta);
    float frac_ti = 0.3f * (1f - seal) + 0.1f;
    float thick_intensity = 0.6f + frac_ti * seal_intensity;
    float rand_offset = 0.3f * intensity_at_rand(SPEED * TIME, TIME)
            + 0.35f * intensity_at_rand(theta + SPEED * TIME, TIME)
            + 0.35f * intensity_at_rand(theta - SPEED * TIME, TIME);
    float rand_intensity = 0.8f + 0.2f * rand_offset;
    float intensity = abs_intensity * thick_intensity * rand_intensity;
    float s = seal_dd(intensity_seal(theta + 2f * (R - r)), r);
    float dd = (1f - 0.01f * seal) * ray_dd(r) * (0.8f + 0.2f * thick_intensity) * abs_intensity + seal * s;
    intensity += dd;
    vec4 color = color_at(theta + 0.08f * rand_offset);
    if (intensity <= 1f) {
        color.a = 0f;
    } else if (0.08f * intensity + 1f * dd + 0.2f * s * seal >= 0.99f) {
        color.a = 1f;
        color.rgb *= 1.25f;
    } else {
        color.a = max(0, 0.9f * (intensity - 1f) + 0.4f);
    }
    COLOR = color;
}