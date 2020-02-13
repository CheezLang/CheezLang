#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;

uniform mat4 m_projection;

out vec4 vColor;

void main()
{
    vColor = aColor;
    gl_Position = m_projection * vec4(aPos.x, aPos.y, aPos.z, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

in vec4 vColor;

void main()
{
    FragColor = vColor;
}