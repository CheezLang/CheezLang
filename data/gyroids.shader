#version 330 core

#shader vertex
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    vTexCoord = aTexCoord;
    gl_Position = vec4(aPos, 0, 1.0);
}

#shader fragment

#define STEP_SIZE 0.1
#define MAX_STEPS 150
#define MAX_DIST 100.
#define SURF_DIST .0001
#define PI 3.1415


layout(location = 0) out vec4 FragColor;

in vec2 vTexCoord;

uniform vec2 uMousePos;
uniform vec2 uTexCoordScale;
uniform float uAspectRatio;
uniform float uZoom;
uniform float uTime;
uniform float[20] uValues;

#define S(a, b, t) smoothstep(a, b, t)
#define COLOR vec3(1, 0.4, 0.1)

mat2 Rot(float a) {
    float s = sin(a);
    float c = cos(a);
    return mat2(c, -s, s, c);
}

mat3 RotX(float a) {
    float s = sin(a);
    float c = cos(a);
    return mat3(
        1, 0, 0,
        0, c, -s,
        0, s, c
    );
}

mat3 RotY(float a) {
    float s = sin(a);
    float c = cos(a);
    return mat3(
        c, 0, -s,
        0, 1, 0,
        s, 0, c
    );
}

float sdBox(vec3 p, vec3 s) {
    p = abs(p)-s;
    return length(max(p, 0.))+min(max(p.x, max(p.y, p.z)), 0.);
}

float sdGyroid(vec3 p, float scale, float thickness, float bias) {
    p *= scale;
    return abs(dot(sin(p), cos(p.zxy))-bias)/scale - thickness;
}

float sdPoly(vec3 p) {
    float x = p.x;
    float y = p.y;
    float z = p.z;
    return
        uValues[0] +
        uValues[1] * x +
        uValues[2] * y +
        uValues[3] * z +
        uValues[4] * x * x +
        uValues[5] * x * y +
        uValues[6] * x * z +
        uValues[7] * y * y +
        uValues[8] * y * z +
        uValues[9] * z * z +
        uValues[10] * x * x * x +
        uValues[11] * x * x * y +
        uValues[12] * x * x * z +
        uValues[13] * x * y * y +
        uValues[14] * x * y * z +
        uValues[15] * x * z * z +
        uValues[16] * y * y * y +
        uValues[17] * y * y * z +
        uValues[18] * y * z * z +
        uValues[19] * z * z * z;
}

vec3 Transform(vec3 p) {
    p.xy *= Rot(p.z * 0.15);
    p.z += uTime * 0.1;
    p.y -= 0.3;
    return p;
}

float GetDist(vec3 p) {
    // return sdPoly(p * 10);
    // return max(box, poly);
    p = Transform(p);

    float g1 = sdGyroid(p, 5.23, 0.03, 1.4);
    float g2 = sdGyroid(p, 10.76, 0.03, 0.3);
    float g3 = sdGyroid(p, 20.76, 0.03, 0.3);
    float g4 = sdGyroid(p, 35.76, 0.03, 0.3);
    float g5 = sdGyroid(p, 60.76, 0.03, 0.3);
    float g6 = sdGyroid(p, 110.76, 0.03, 0.3);
    float g7 = sdGyroid(p, 200.76, 0.03, 0.3);

    // float g = min(g1, g2);   // union
    // float g = max(g1, -g2);  // subtraction
    g1 -= g2 * 0.4;
    g1 -= g3 * 0.3;
    g1 -= g4 * 0.2;
    g1 += g5 * 0.2;
    g1 += g6 * 0.3;
    g1 -= g7 * 0.3;
    
    return g1 * 0.8;
    float box = sdBox(p, vec3(2));
    float d = max(box, g1 * 0.8);
    return d;
}

float RayMarch1(vec3 ro, vec3 rd) {
    float t=0.;
    
    float prev_dist = GetDist(ro);
    float prev_t = 0.0;
    t += STEP_SIZE;

    float dist;

    for(int i=0; true; i++) {
        if (i >= MAX_STEPS) {
            return MAX_DIST + 1.0;
        }
        vec3 p = ro + rd * t;
        dist = GetDist(p);

        if (prev_dist * dist <= 0.0) {
            break;
        }

        prev_dist = dist;
        prev_t = t;
        // t += dist * 0.5;
        // t += STEP_SIZE;
        t += clamp(dist, STEP_SIZE, STEP_SIZE * 5);


        if(t > MAX_DIST)
            return t;
    }

    if (prev_dist > 0) {
        float tmp = prev_dist;
        prev_dist = dist;
        dist = tmp;
        tmp = prev_t;
        prev_t = t;
        t = tmp;
    }

    for(int i=0; abs(dist) >= SURF_DIST; i++) {
        if (i >= MAX_STEPS * 2) {
            return MAX_DIST + 1.0;
        }

        float tt = (t + prev_t) * 0.5;
        vec3 p = ro + rd * tt;
        dist = GetDist(p);

        if (dist < 0) {
            prev_t = tt;
        } else {
            t = tt;
        }
    }
    
    return t;
}

float RayMarch2(vec3 ro, vec3 rd) {
    float t=0.;
    
    for(int i=0; i<MAX_STEPS; i++) {
        vec3 p = ro + rd*t;
        float dS = GetDist(p) * 0.75;
        t += dS;
        if(t>MAX_DIST || abs(dS)<SURF_DIST) break;
    }
    
    return t;
}

float RayMarch(vec3 ro, vec3 rd) {
    return RayMarch2(ro, rd);
}

vec3 GetNormal(vec3 p) {
    float d = GetDist(p);
    vec2 e = vec2(.01, 0);
    
    vec3 n = d - vec3(
        GetDist(p-e.xyy),
        GetDist(p-e.yxy),
        GetDist(p-e.yyx));
    
    return normalize(n);
}

vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z) {
    vec3 f = normalize(l-p),
        r = normalize(cross(vec3(0,1,0), f)),
        u = cross(f,r),
        c = p+f*z,
        i = c + uv.x*r + uv.y*u,
        d = normalize(i-p);
    return d;
}

vec3 Background(vec3 rd) {
    vec3 col = vec3(0);
    float t = uTime;

    float y = rd.y * 0.5 + 0.5;
    col += (1 - y) * COLOR * 2;

    float a = atan(rd.x, rd.z);
    float flames = sin(a * 10 + t) * sin(a * 7 - t) * sin(a * 6) * S(0.8, 0.5, y);
    col += flames;
    col = max(col, 0);

    col += S(0.5, 0.0, y);
    return col;
}

void main() {
    vec2 uv = (vTexCoord * uTexCoordScale - 0.5) * vec2(uAspectRatio, 1.0);
    vec2 m = uMousePos * 2.0 - 1.0;
    m *= 0;
    float t = uTime;

    uv += sin(uv * vec2(15, 20) + t) * 0.01;
    
    vec3 ro, rd;
    if (false) {
        ro = vec3(1.0);
        ro.yz *= Rot(m.y*3.14+1.);
        ro.xz *= Rot(-m.x*6.2831);
        ro = RotY(m.x) * RotX(clamp(m.y, -0.49 * PI, 0.49 * PI)) * vec3(0, 0, -5 + uZoom);

        rd = GetRayDir(uv, ro, vec3(0), 1.);
    } else {
        ro = vec3(0.0);

        rd = RotY(m.x) * RotX(clamp(m.y, -0.49 * PI, 0.49 * PI)) * vec3(0, 0, 1);
        rd = GetRayDir(uv, ro, rd, 1.);
    }

    float d = RayMarch(ro, rd);
    
    vec3 col = vec3(0);
    if (d < MAX_DIST) {
        vec3 p = ro + rd * d;
        vec3 n = GetNormal(p);

        float height = p.y;
        p = Transform(p);
        
        float dif = n.y * 0.5 + 0.5;
        col += dif * dif;

        float g2 = sdGyroid(p, 10.76, 0.03, 0.3);
        col *= S(-0.1, 0.1, g2); // blackening

        float crackWidth = -0.02 + S(0, -0.5, n.y) * 0.04;
        float cracks = S(crackWidth, -0.03, g2);
        float g3 = sdGyroid(p + t * 0.1, 5.76, 0.03, 0.0);
        float g4 = sdGyroid(p - t * 0.05, 4.76, 0.03, 0.0);
        cracks *= g3 * g4 * 20.0 + 0.2 * S(0.2, 0, n.y);
        col += cracks * COLOR * 3;

        float g5 = sdGyroid(p - vec3(0, t, 0), 3.76, 0.03, 0.0);
        col += g5 * COLOR;
        
        col += S(0, -2, height) * COLOR;
    }

    // col *= 0.0;
    // d = sdGyroid(vec3(uv.x, uv.y, uTime * 0.1), 20.0, 0.01, 0.0);
    // col += d * 10.0;

    col = mix(col, Background(rd), S(0, 7, d));
    // col = Background(rd);

    col *= 1 - dot(uv, uv) * 1.25;
    
    // gamma correction
    // col = pow(col, vec3(.4545));
    
    FragColor = vec4(col, 1.0);
    // FragColor = vec4(uValues[0], uValues[1], uValues[2], 1.0);
    // FragColor = vec4(m, 0.0, 1.0);
    // FragColor = vec4(uv, 0.0, 1.0);
}