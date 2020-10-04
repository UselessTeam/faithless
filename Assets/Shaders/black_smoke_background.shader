shader_type canvas_item;

const float DISPLACEMENT = 20f;
const float SPEED = .1f;
const float SCALE = 0.1f;
const float OFF = 10f;

vec4 displaced(float x, float y, vec2 _uv, sampler2D t) {
    vec2 displacement = vec2(x, y);
    vec2 uv = _uv + displacement;
    if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f) {
        return vec4(0f);
    }
    return texture(t, uv) / 4f;
}

float rand2(float value) {
    return sin(value + 1.5f * sin(0.71*value));
}

float rand(float v) {
    return 0.2f * sin(v) + 0.3f * rand2(0.811f*v) + 0.5f * rand2(1.237f*v);
}

void fragment() {
    float scale_x = SCALE / SCREEN_PIXEL_SIZE.x;
    float scale_y = SCALE / SCREEN_PIXEL_SIZE.y;
    vec2 d = DISPLACEMENT * SCREEN_PIXEL_SIZE;
    float up = d.y * rand(scale_y * (- SCREEN_UV.y + SPEED * TIME));
    float down = d.y * rand(scale_y * (1f + SCREEN_UV.y + SPEED * TIME));
    float left = d.x * rand(scale_x * (3f + SCREEN_UV.x + SPEED * TIME));
    float right = d.x * rand(scale_x * (10f - SCREEN_UV.x + SPEED * TIME));
    COLOR = displaced(up, left, UV, TEXTURE)
        + displaced(down, left, UV, TEXTURE)
        + displaced(down, right, UV, TEXTURE)
        + displaced(up, right, UV, TEXTURE);
}