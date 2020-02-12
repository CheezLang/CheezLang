#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 m_projection;
uniform mat4 m_model;

out vec2 vTexCoord;

void main()
{
    vTexCoord = aTexCoord;
    gl_Position = m_projection * m_model * vec4(aPos.x, aPos.y, aPos.z, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

uniform vec3 color;
uniform vec4 u_sub;
uniform sampler2D u_texture;

in vec2 vTexCoord;

void main()
{
    vec2 uv = mix(u_sub.xy, u_sub.zw, vTexCoord);
    FragColor = texture(u_texture, uv) * vec4(color, 1.0f);
    FragColor = vec4(color, 1.0f);
    // FragColor = vec4(vTexCoord, 0.0f, 1.0f);
    // FragColor = u_sub;

    // if (FragColor.a < 0.5f) {
    //     discard;
    // }
}