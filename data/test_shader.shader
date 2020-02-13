#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in float aTexIndex;

uniform mat4 m_projection;

out vec4 vColor;
out vec2 vTexCoord;
out float vTexIndex;

void main()
{
    vColor = aColor;
    vTexCoord = aTexCoord;
    vTexIndex = aTexIndex;
    gl_Position = m_projection * vec4(aPos.x, aPos.y, aPos.z, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment
layout(location = 0) out vec4 FragColor;

uniform sampler2D u_texture;

in vec4 vColor;
in vec2 vTexCoord;
in float vTexIndex;

void main()
{
    // vec2 uv = mix(u_sub.xy, u_sub.zw, vTexCoord);
    // FragColor = texture(u_texture, uv) * vec4(color, 1.0f);
    // FragColor = vec4(vTexCoord, 0.0f, 1.0f);
    // FragColor = u_sub;
    FragColor = vColor;
    //FragColor = vec4(vTexCoord, 0.0f, 1.0f);

    // if (FragColor.a < 0.5f) {
    //     discard;
    // }
}