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
const float SPEED = 0.5f;
const float GLOW_RAD = 20f;

const float TWO_PI = 6.28318530718;

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
    float frac = theta / TWO_PI;
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
    return texture(NOISE_PATTERN, vec2(cos(value), sin(value))).x;
}

float intensity_at_theta(float theta) {
    return max(1.2f * color_at(theta).a - 0.2f, 0f);
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
    if (r <= 0.3f) {
        COLOR = vec4(0f);
    } else {
        r = sqrt(r);
        float theta = acos(x/r);
        if (y < 0f) {
            theta = -theta;
        }
        float abs_intensity = intensity_at_theta(theta);
        float rand_intensity = 0.3f +
             + 0.5f * intensity_at_rand(SPEED * TIME)
             + 0.15f * intensity_at_rand(theta + SPEED * TIME)
             + 0.15f * intensity_at_rand(theta - SPEED * TIME);
        float intensity = abs_intensity * rand_intensity;
        float off = intensity;
        r = r + 0.01f * off;
        vec2 uv = card(r, theta);
        vec4 color = texture(TEXTURE, uv);
        if (color.a > 0.5f) {
            color = (1f + 0.5f * intensity) * color_at(theta);
        } else {
            vec2 p = GLOW_RAD * TEXTURE_PIXEL_SIZE;
            float off_radius = 0.1f * intensity;
            vec4 colorOuter = glow(TEXTURE, card(r + off_radius, theta));
            vec4 colorInter = glow(TEXTURE, card(r - off_radius, theta));
            color = 0.6f * (colorOuter + colorInter);
        }
        COLOR = color;
    }
}