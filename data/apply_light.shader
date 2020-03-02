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
uniform float uExposure;

void main()
{
    const float gamma = 2.2;

    vec3 color = texture(uColorTex, vTexCoord).rgb;
    vec3 light = texture(uLightMap, vTexCoord).rgb;
    vec3 hdr_color = color * light;
    vec3 ldr_color = vec3(0);

    ldr_color = hdr_color;

    // reinhard tone mapping
    // ldr_color = hdr_color / (hdr_color + vec3(1.0));

    // exposure tone mapping
    ldr_color = vec3(1.0) - exp(-hdr_color * uExposure);

    // gamma correction
    ldr_color = pow(ldr_color, vec3(1.0 / gamma));

    ldr_color = light;
    FragColor = vec4(ldr_color, 1.0);
}