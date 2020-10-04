shader_type canvas_item;

uniform sampler2D NOISE_PATTERN;
uniform vec4 color_0 : hint_color;
uniform vec4 color_1 : hint_color;
uniform vec4 color_2 : hint_color;
uniform vec4 color_3 : hint_color;
uniform vec4 color_4 : hint_color;
uniform vec4 color_5 : hint_color;
uniform vec4 color_6 : hint_color;
uniform vec4 color_7 : hint_color;
uniform int N;

const float DISPLACEMENT = 100.f;
const float SPEED = 0.8f;
const float GLOW_RAD = 20f;

const float R = 0.7f;

const float TWO_PI = 6.28318530718;
const float HALF_PI = 1.5707963267949;

// Hack because Godot doesn't support array export (yet)
vec4 color_get(int id) {
    if (id >= N || id < 0) {
        return vec4(0);
    }
    if (id==0) { return color_0; }
    if (id==1) { return color_1; }
    if (id==2) { return color_2; }
    if (id==3) { return color_3; }
    if (id==4) { return color_4; }
    if (id==5) { return color_5; }
    if (id==6) { return color_6; }
    if (id==7) { return color_7; }
    return vec4(0);
}

float index_at(float theta) {
    float frac = (theta - HALF_PI) / TWO_PI;
    while(frac < 0f) {
        frac += 1f;
    }
    while(frac >= 1f) {
        frac -= 1f;
    }
    return frac * float(N);
}

vec4 color_at(float theta) {
    float index = index_at(theta);
    int i = int(index);
    int j = i+1;
    if(j == N) {
        j = 0;
    }
    float frac = index - float(i);
    return (1f - frac) * color_get(i) + frac * color_get(j);
}

vec4 displaced(float x, float y, vec2 d, vec2 _uv, sampler2D t) {
    vec2 displacement = vec2(d.x * x, d.y * y);
    vec2 uv = _uv + displacement;
    if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f) {
        return vec4(0f);
    }
    return texture(t, uv) / 4f;
}

float intensity_at_rand(float value) {
    return texture(NOISE_PATTERN, vec2(0.5f + 0.5f * cos(value), 0.5f + 0.5f * sin(value))).x;
}

float intensity_at_theta(float theta) {
    return min(1f, max(1.2f * color_at(theta).a - 0.2f, 0f));
}

float intensity_seal(float theta) {
    float value = index_at(theta);
    value -= float(int(value));
    value = 1f - 4f * min(value * value, (value - 1f) * (value - 1f));
    return max(2f * value * value - 1f, 0f);
}

float ray_d(float r) {
    float p = (R - r);
    p = p * p;
    return p;
}

vec4 glow(sampler2D _texture, vec2 uv) {
    return texture(_texture, uv);
}

vec2 card(float r, float theta) {
    return vec2(0.5f * r * cos(theta) + 0.5f, 0.5f * r * sin(theta) + 0.5f);
}

void fragment() {
    float x = 2f * (UV.x - 0.5f);
    float y = 2f * (UV.y - 0.5f);
    float r = x * x + y * y;
    if (r <= 0.1f || r >= 2f) {
        COLOR = vec4(0f);
    } else {
        r = sqrt(r);
        float theta = acos(x/r);
        if (y < 0f) {
            theta = -theta;
        }
        float abs_intensity = intensity_at_theta(theta);
        float thick_intensity = 0.7f + 0.3f * intensity_seal(theta);
        float rand_intensity = 0.1f +
             + 0.4f * intensity_at_rand(SPEED * TIME)
             + 0.25f * intensity_at_rand(theta + SPEED * TIME)
             + 0.25f * intensity_at_rand(theta - SPEED * TIME);
        float intensity = abs_intensity * thick_intensity * rand_intensity;
        float d = (1f - 30f * ray_d(r));
        float dd = d * (0.8f + 0.2f * thick_intensity) * abs_intensity;
        intensity += dd;
        float off = intensity;
        r = r + 0.01f * off;
        vec2 uv = card(r, theta);
        vec4 color = color_at(theta);
        if (0.1f * intensity + dd >= 1.05f) {
            color.a = 1f;
            color.rgb *= 1.2f;
        } else if (intensity <= 0.5f) {
            color = vec4(0);
        } else {
            color = color_at(theta);
            color.a = 0.6f * intensity - 0.1f;
        }
        COLOR = color;
    }
}