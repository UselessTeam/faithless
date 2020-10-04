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

const float R = 0.75f;
const float SPEED = 4f;

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
    if(frac < 0f) {
        frac += 1f;
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

float intensity_at_rand(float value, float t) {
    return 0f;
    // return 0.2f * sin(2f * value + 1.22f * sin(t))
    //      + 0.2f* sin(3f * value + 1.33f * sin(1.2f * t))
    //      + 0.3f* sin(5f * value + 1.55f * sin(1.3f * t))
    //      + 0.3f* sin(7f * value + 1.77f * sin(1.5f * t));
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

void fragment() {
    float x = 2f * (UV.x - 0.5f);
    float y = 2f * (UV.y - 0.5f);
    float r = x * x + y * y;
    if (r <= 0.3f || r >= 1.5f) {
        COLOR = vec4(0f);
    } else {
        r = sqrt(r);
        float theta = acos(x/r);
        if (y < 0f) {
            theta = -theta;
        }
        float abs_intensity = intensity_at_theta(theta);
        float thick_intensity = 0.7f + 0.3f * intensity_seal(theta);
        float rand_intensity = 1f +
             + 0.2f * intensity_at_rand(SPEED * TIME, TIME)
             + 0.15f * intensity_at_rand(theta + SPEED * TIME, TIME)
             + 0.15f * intensity_at_rand(theta - SPEED * TIME, TIME);
        float intensity = abs_intensity * thick_intensity * rand_intensity;
        float dd = (1f - 30f * ray_d(r)) * (0.8f + 0.2f * thick_intensity) * abs_intensity;
        intensity += dd;
        vec4 color = color_at(theta);
        if (0.1f * intensity + dd >= 1.05f) {
            color.a = 1f;
            color.rgb *= 1.2f;
        } else {
            color.a = max(0, 0.4f * intensity + 0.05f);
        }
        COLOR = color;
    }
}