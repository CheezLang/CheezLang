#version 330 core

#shader vertex
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in int aTexIndex;

uniform mat4 m_projection;

out vec4 vColor;
out vec2 vTexCoord;
flat out int vTexIndex;

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

// currently only support 16 texture slots because that is apparently the minimum
// and we currently don't ask OpenGL how many slots are available in the fragment shader.
// We could do that and then, in the shader, write something like
//    uniform sampler2D[MAX_TEXTURE_SLOTS] uTextures;
// and then replace MAX_TEXTURE_SLOTS with the value retrieved from OpenGL.
uniform sampler2D[16] uTextures;

in vec4 vColor;
in vec2 vTexCoord;
flat in int vTexIndex;

void main()
{
    FragColor = texture(uTextures[vTexIndex], vTexCoord) * vColor;

    // output tex id as greyscale value
    // float f = float(vTexIndex) / 16.0f;
    // FragColor = vec4(f, f, f, 1.0f);
}