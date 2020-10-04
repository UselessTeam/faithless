shader_type canvas_item;

const float DISPLACEMENT = 5.f;
const float SPEED = .1f;
const float SCALE = 5000f;

vec4 displaced(float x, float y, vec2 d, vec2 _uv, sampler2D t) {
    vec2 displacement = vec2(d.x * x, d.y * y);
    vec2 uv = _uv + displacement;
    if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f) {
        return vec4(0f);
    }
    return texture(t, uv) / 4f;
}

float rand2(float value) {
    return sin(value + sin(value));
}

float rand(float v) {
    return 0.2f * sin(v) + 0.3f * sin(0.8f*v) + 0.5f * rand2(1.2f*v);
}

void fragment() {
    float scale_x = SCALE * TEXTURE_PIXEL_SIZE.x;
    float scale_y = SCALE * TEXTURE_PIXEL_SIZE.y;
    float up = rand(scale_y * (- UV.y + SPEED * TIME));
    float down = rand(scale_y * (1f + UV.y + SPEED * TIME));
    float left = rand(scale_x * (3f + UV.x + SPEED * TIME));
    float right = rand(scale_x * (10f - UV.x + SPEED * TIME));
    vec2 d = DISPLACEMENT * TEXTURE_PIXEL_SIZE;
    COLOR = displaced(up, left, d, UV, TEXTURE)
        + displaced(down, left, d, UV, TEXTURE)
        + displaced(down, right, d, UV, TEXTURE)
        + displaced(up, right, d, UV, TEXTURE);
}