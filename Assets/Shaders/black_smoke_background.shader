shader_type canvas_item;

uniform sampler2D NOISE_PATTERN;

const float DISPLACEMENT = 20.f;
const float SPEED = 10f;

vec4 displaced(float x, float y, vec2 d, vec2 _uv, sampler2D t) {
    vec2 displacement = vec2(d.x * x, d.y * y);
    vec2 uv = _uv + displacement;
    if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f) {
        return vec4(0f);
    }
    return texture(t, uv) / 4f;
}

void fragment() {
    vec2 a_push = TEXTURE_PIXEL_SIZE * vec2(SPEED * TIME, 0.2f * SPEED * TIME);
    vec2 b_push = TEXTURE_PIXEL_SIZE * vec2(0.2f * SPEED * TIME, -SPEED * TIME);
    float up = texture(NOISE_PATTERN, UV + a_push).x - 0.5f;
    float down = texture(NOISE_PATTERN, UV - a_push).x - 0.5f;
    float left = texture(NOISE_PATTERN, UV + b_push).x - 0.5f;
    float right = texture(NOISE_PATTERN, UV - b_push).x - 0.5f;
    vec2 d = DISPLACEMENT * TEXTURE_PIXEL_SIZE;
    COLOR = displaced(up, left, d, UV, TEXTURE)
        + displaced(left, down, d, UV, TEXTURE)
        + displaced(down, right, d, UV, TEXTURE)
        + displaced(right, up, d, UV, TEXTURE);
}