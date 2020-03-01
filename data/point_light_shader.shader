#version 330 core

#define PI 3.1415

#shader vertex
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aLightPos;
layout (location = 2) in vec3 aColor;
layout (location = 3) in vec4 aParams;

uniform mat4 m_projection;
uniform mat4 m_projection_inv;

out vec2 vWorldPos;
out vec2 vLightPos;
out vec3 vColor;
out float vRadius;
out float vSmoothness;
out float vOpen;
out float vDirection;

float radians(float deg) {
    return deg / 180.0 * PI;
}

float mod2(float a, float b) {
    return mod(mod(a, b) + b, b);
}

void main()
{
    vWorldPos = (m_projection_inv * vec4(aPos, 0.0, 1.0)).xy;
    vLightPos = aLightPos;
    vColor = aColor;
    vRadius = aParams.x;
    vSmoothness = clamp(aParams.y, 0, 0.99) * vRadius;
    vOpen = radians(aParams.z * 0.5); //radians(mod(aParams.z + 360.0, 360.0));
    vDirection = radians(mod2(aParams.w, 360.0));

    gl_Position = vec4(aPos, 0.0, 1.0);
    gl_Position.w = 1.0f;
}

#shader fragment

layout(location = 0) out vec4 FragColor;

in vec2 vWorldPos;
in vec2 vLightPos;
in vec3 vColor;
in float vRadius;
in float vSmoothness;
in float vOpen;
in float vDirection;

// polynomial smooth min (k = 0.1);
float smin( float a, float b, float k )
{
    float res = exp2( -k*a ) + exp2( -k*b );
    return -log2( res )/k;
}

void main()
{
    float radius = abs(vRadius);

    // angle
    vec2 dir = normalize(vWorldPos - vLightPos);

    float arc_length = (radius - vSmoothness) / (2.0 * PI * radius) * 2.0 * PI;
    float angle_inner = clamp(vOpen - arc_length, 0, 10000);

    float angle = 0.0;
    angle = atan(dir.y, dir.x);

    float angle_diff = min(abs(angle - vDirection), abs(angle + 2.0 * PI - vDirection));

    float d1 = smoothstep(vOpen, angle_inner, angle_diff);

    // dist
    float d2 = smoothstep(radius, vSmoothness, distance(vWorldPos, vLightPos));

    float d = clamp(smin(d1, d2, 3), 0, 1);
    d *= d;
    FragColor = vec4(clamp(vColor, 0, 100000000) * d, 1.0);



    if (vRadius <= 0.0)
    {
        float a = 1.0;
        float b = 0.0;
        float c = 0.0;
        d = distance(vWorldPos, vLightPos);
        FragColor = vec4(clamp(vColor, 0, 100000000) / (a * d * d + b * d + c) * clamp(d1, 0, 1), 1);
    }
    
    // FragColor = vec4(clamp(d1, 0, 1));
}