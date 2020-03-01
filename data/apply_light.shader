#version 330 core

#shader vertex
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    vTexCoord = aTexCoord;
    gl_Position = vec4(aPos, 0.0, 1.0);
}

#shader fragment
layout(location = 0) out vec4 FragColor;

in vec2 vTexCoord;

uniform sampler2D uColorTex;
uniform sampler2D uLightMap;

void main()
{
    vec4 color = texture(uColorTex, vTexCoord);
    vec4 light = texture(uLightMap, vTexCoord);
    FragColor = color * light;

    // FragColor = color;
    // FragColor = light;
    // FragColor = vec4(vTexCoord, 0.0, 1.0);
}