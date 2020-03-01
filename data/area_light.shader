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
    vec2 uv = vTexCoord * 2 - 1;

    // float dist_from_edge_x = min(vTexCoord.x, 1 - vTexCoord.x);
    // float dist_from_edge_y = min(vTexCoord.y, 1 - vTexCoord.y);
    // float dist_from_edge = smin(dist_from_edge_x, dist_from_edge_y, vSmooth) * 2;
    // dist_from_edge = clamp(dist_from_edge, 0, 1);
    // dist_from_edge = dist_from_edge * dist_from_edge;
    // dist_from_edge = dist_from_edge_y;



    float dist_from_edge = clamp(length(uv), 0, 1);
    float brightness = smoothstep(1, clamp(vSmooth, 0, 0.999), dist_from_edge);
    FragColor = vec4(vColor * brightness, 1);
}