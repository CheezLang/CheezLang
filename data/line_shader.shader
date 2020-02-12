#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;
layout (location = 1) in float aTexCoord;

uniform mat4 m_projection;
uniform mat4 m_model;

out float vTexCoord;

void main()
{
    vTexCoord = aTexCoord;
    gl_Position = m_projection * m_model * vec4(aPos.x, aPos.y, aPos.z, 1.0);
    gl_Position.w = 1.0f;

    // gl_Position = vec4(aPos, 0);
}

#shader fragment
layout(location = 0) out vec4 FragColor;

uniform vec3 color1;
uniform vec3 color2;

in float vTexCoord;

void main()
{
    FragColor = vec4(mix(color1, color2, vTexCoord), 1.0f);
    // FragColor = vec4(color1, 1.0f);
    // FragColor = vec4(1.0f, 0.0f, 1.0f, 1.0f);
}