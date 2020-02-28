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

uniform sampler2D input;

void main()
{
    vec4 col = texture(input, vTexCoord);
    float brightness = (col.x + col.y + col.z) / 3.0f;
    FragColor = vec4(brightness, brightness, brightness, 1);
}