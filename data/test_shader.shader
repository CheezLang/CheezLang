#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;

uniform mat4 m_projection;
uniform mat4 m_model;

void main()
{
    gl_Position = m_projection * m_model * vec4(aPos.x, aPos.y, aPos.z, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

uniform vec3 color;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
    FragColor = vec4(color, 1.0f);
}