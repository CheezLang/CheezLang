#version 330 core

#shader vertex
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aColor;
layout (location = 3) in float aSmooth;

uniform mat4 m_projection;

out vec2 vTexCoord;
out vec3 vColor;
out float vSmooth;

void main()
{
    vTexCoord = aTexCoord;
    vColor = aColor;
    vSmooth = aSmooth;
    gl_Position = m_projection * vec4(aPos, 0.0, 1.0);
    gl_Position.w = 1.0;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

in vec2 vTexCoord;
in vec3 vColor;
in float vSmooth;

// polynomial smooth min (k = 0.1);
float smin( float a, float b, float k)
{
    float h = max( k-abs(a-b), 0.0 )/k;
    return min(a, b) - h*h*k*(1.0/4.0);
}

void main()
{
    float scale = vSmooth;

    vec2 uv = (vTexCoord * 2 - 1) / scale;

    FragColor = vec4(uv, 0, 1);

    float distance = 0.0;

    if (uv.x >= -1 && uv.x < 1 && uv.y >= -1 && uv.y < 1) {
        distance = 1.0;
    } else {
        float s2 = 1 / ((1 / scale) - 1);
        uv = clamp((abs(uv) - 1) * s2, 0, 1);
        distance = 1 - length(uv);
    }

    float circle_dist = 1 - length(vTexCoord * 2 - 1);
    // distance = smin(distance, circle_dist, 0.1);

    float brightness = smoothstep(0, 1, distance);
    FragColor = vec4(vColor * brightness, 1);
}