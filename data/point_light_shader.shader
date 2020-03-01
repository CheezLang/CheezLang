#version 330 core

#shader vertex
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aLightPos;
layout (location = 2) in vec3 aColor;

uniform mat4 m_projection;
uniform mat4 m_projection_inv;

out vec2 vWorldPos;
out vec2 vLightPos;
out vec3 vColor;

void main()
{
    vWorldPos = (m_projection_inv * vec4(aPos, 0.0, 1.0)).xy;
    vLightPos = aLightPos;
    vColor = aColor;
    gl_Position = vec4(aPos, 0.0, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

in vec2 vWorldPos;
in vec2 vLightPos;
in vec3 vColor;

void main()
{
    float d = distance(vWorldPos, vLightPos);

    float a = 0.5;
    float b = 0.0;
    float c = 1.0;
    float brightness = 1.0 / (a * d * d + b * d + c);
    FragColor = vec4(vColor * brightness, 1);
}