#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

uniform vec2 circleCenter;
uniform float circleRadius;
uniform vec2 iResolution;

uniform float rotation;
uniform float iTime;
uniform int seed; // will go instead of time in the snoise 3d

// Output fragment color
out vec4 finalColor;

//
// Description : Array and textureless GLSL 2D/3D/4D simplex 
//               noise functions.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
// 

vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }

vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }

float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }

vec4 permute(vec4 x) { return mod289(((x*34.0)+10.0)*x); }

float permute(float x) { return mod289(((x*34.0)+10.0)*x); }

vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

float taylorInvSqrt(float r) { return 1.79284291400159 - 0.85373472095314 * r; }

vec4 grad4(float j, vec4 ip)
  {
  const vec4 ones = vec4(1.0, 1.0, 1.0, -1.0);
  vec4 p,s;

  p.xyz = floor( fract (vec3(j) * ip.xyz) * 7.0) * ip.z - 1.0;
  p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
  s = vec4(lessThan(p, vec4(0.0)));
  p.xyz = p.xyz + (s.xyz*2.0 - 1.0) * s.www; 

  return p;
  }
						
// (sqrt(5) - 1)/4 = F4, used once below
#define F4 0.309016994374947451

float snoise4d(vec4 v)
  {
  const vec4  C = vec4( 0.138196601125011,  // (5 - sqrt(5))/20  G4
                        0.276393202250021,  // 2 * G4
                        0.414589803375032,  // 3 * G4
                       -0.447213595499958); // -1 + 4 * G4

// First corner
  vec4 i  = floor(v + dot(v, vec4(F4)) );
  vec4 x0 = v -   i + dot(i, C.xxxx);

// Other corners

// Rank sorting originally contributed by Bill Licea-Kane, AMD (formerly ATI)
  vec4 i0;
  vec3 isX = step( x0.yzw, x0.xxx );
  vec3 isYZ = step( x0.zww, x0.yyz );
//  i0.x = dot( isX, vec3( 1.0 ) );
  i0.x = isX.x + isX.y + isX.z;
  i0.yzw = 1.0 - isX;
//  i0.y += dot( isYZ.xy, vec2( 1.0 ) );
  i0.y += isYZ.x + isYZ.y;
  i0.zw += 1.0 - isYZ.xy;
  i0.z += isYZ.z;
  i0.w += 1.0 - isYZ.z;

  // i0 now contains the unique values 0,1,2,3 in each channel
  vec4 i3 = clamp( i0, 0.0, 1.0 );
  vec4 i2 = clamp( i0-1.0, 0.0, 1.0 );
  vec4 i1 = clamp( i0-2.0, 0.0, 1.0 );

  //  x0 = x0 - 0.0 + 0.0 * C.xxxx
  //  x1 = x0 - i1  + 1.0 * C.xxxx
  //  x2 = x0 - i2  + 2.0 * C.xxxx
  //  x3 = x0 - i3  + 3.0 * C.xxxx
  //  x4 = x0 - 1.0 + 4.0 * C.xxxx
  vec4 x1 = x0 - i1 + C.xxxx;
  vec4 x2 = x0 - i2 + C.yyyy;
  vec4 x3 = x0 - i3 + C.zzzz;
  vec4 x4 = x0 + C.wwww;

// Permutations
  i = mod289(i); 
  float j0 = permute( permute( permute( permute(i.w) + i.z) + i.y) + i.x);
  vec4 j1 = permute( permute( permute( permute (
             i.w + vec4(i1.w, i2.w, i3.w, 1.0 ))
           + i.z + vec4(i1.z, i2.z, i3.z, 1.0 ))
           + i.y + vec4(i1.y, i2.y, i3.y, 1.0 ))
           + i.x + vec4(i1.x, i2.x, i3.x, 1.0 ));

// Gradients: 7x7x6 points over a cube, mapped onto a 4-cross polytope
// 7*7*6 = 294, which is close to the ring size 17*17 = 289.
  vec4 ip = vec4(1.0/294.0, 1.0/49.0, 1.0/7.0, 0.0) ;

  vec4 p0 = grad4(j0,   ip);
  vec4 p1 = grad4(j1.x, ip);
  vec4 p2 = grad4(j1.y, ip);
  vec4 p3 = grad4(j1.z, ip);
  vec4 p4 = grad4(j1.w, ip);

// Normalise gradients
  vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
  p0 *= norm.x;
  p1 *= norm.y;
  p2 *= norm.z;
  p3 *= norm.w;
  p4 *= taylorInvSqrt(dot(p4,p4));

// Mix contributions from the five corners
  vec3 m0 = max(0.6 - vec3(dot(x0,x0), dot(x1,x1), dot(x2,x2)), 0.0);
  vec2 m1 = max(0.6 - vec2(dot(x3,x3), dot(x4,x4)            ), 0.0);
  m0 = m0 * m0;
  m1 = m1 * m1;
  return 49.0 * ( dot(m0*m0, vec3( dot( p0, x0 ), dot( p1, x1 ), dot( p2, x2 )))
               + dot(m1*m1, vec2( dot( p3, x3 ), dot( p4, x4 ) ) ) ) ;

  }

// Function to perform matrix multiplication
vec3 multiplyMatrix(vec3 v, mat3 m) {
    return vec3(
        dot(v, m[0]),
        dot(v, m[1]),
        dot(v, m[2])
    );
}

// Noise function with rotation
float snoise_rotated4d(vec3 v, mat3 rotationMatrix) {
    // Rotate the input vector using the transformation matrix
    vec3 rotatedV = multiplyMatrix(v, rotationMatrix);

    // Calculate simplex noise with the rotated coordinates
    return snoise4d(vec4(rotatedV, 0.3*iTime));
}


float color4d(vec3 xyz) { return snoise4d(vec4(xyz, 0.3*iTime)); }
float color_rotated4d(vec3 xyz, mat3 rotationMatrix) { return snoise_rotated4d(xyz, rotationMatrix); }

void main()
{
    // vec4 texel = texture(texture0, fragTexCoord);   // Get texel color
    // vec2 texelScale = vec2(0.0);

    // vec4 color = mix(vec4(0.0), outlineColor, outline);
    // finalColor = mix(color, texel, texel.a);

    // convert coordinates
    vec2 pixelCoord = gl_FragCoord.xy / iResolution;
    vec2 circleCenterCoord = circleCenter.xy / iResolution;
    float circleWidth = circleRadius / iResolution.x; // If I get some weird with this I can make it a vec2

    float dist = distance(pixelCoord, circleCenterCoord);
    
    if (circleWidth >= dist)
    {
        float x = (pixelCoord.x - circleCenterCoord.x) / circleWidth;
        float y = (pixelCoord.y - circleCenterCoord.y) / circleWidth;
        float z = sqrt( 1 - pow(x,2) - pow(y,2));

        finalColor = vec4(x,y,z,1.0);
        
        // vec2 p = (gl_FragCoord.xy/iResolution.y) * 2.0 - 1.0;

        // float z_squared = 1.0 - length(p.xy);
        float z_squared = z * z;
        // vec3 xyz = vec3(p, -sqrt(z_squared));
        vec3 xyz = vec3(x, y, -z);

        mat3 rotationMatrixX;
        rotationMatrixX[0] = vec3(1.0, 0.0, 0.0);
        rotationMatrixX[1] = vec3(0.0, cos(rotation/8), -sin(rotation/8));
        rotationMatrixX[2] = vec3(0.0, sin(rotation/8), cos(rotation/8));
        mat3 rotationMatrixY;
        rotationMatrixY[0] = vec3(cos(rotation), 0.0, sin(rotation));
        rotationMatrixY[1] = vec3(0.0, 1.0, 0.0);
        rotationMatrixY[2] = vec3(-sin(rotation), 0.0, cos(rotation));
        mat3 rotationMatrixZ;
        rotationMatrixZ[0] = vec3(cos(0.41), -sin(0.41), 0.0);
        rotationMatrixZ[1] = vec3(sin(0.41), cos(0.41), 0.0);
        rotationMatrixZ[2] = vec3(0.0, 0.0, 1.0);

        mat3 rotationMatrix = rotationMatrixZ * rotationMatrixY;

        vec3 step = vec3(1.3, 1.7, 2.1);
        // float n = color(xyz);
        // n += 0.5 * color(xyz * 2.0 - step);
        // n += 0.25 * color(xyz * 4.0 - 2.0 * step);
        // n += 0.125 * color(xyz * 8.0 - 3.0 * step);
        // n += 0.0625 * color(xyz * 16.0 - 4.0 * step);
        // n += 0.03125 * color(xyz * 32.0 - 5.0 * step);

        float n = color_rotated4d(xyz, rotationMatrix);
        n += 0.5 * color_rotated4d(xyz * 2.0 , rotationMatrix);
        n += 0.25 * color_rotated4d(xyz * 4.0 , rotationMatrix);
        n += 0.125 * color_rotated4d(xyz * 8.0 , rotationMatrix);
        n += 0.0625 * color_rotated4d(xyz * 16.0 , rotationMatrix);
        n += 0.03125 * color_rotated4d(xyz * 32.0 , rotationMatrix);

        finalColor.xyz = mix(0.0, 0.5 + 0.5 * n, smoothstep(0.0, 0.003, z_squared)) * vec3(1, 1, 1);

        if (finalColor.x < 0.5) {finalColor.xyz = vec3(0.0);}
    }
}

//https://stegu.github.io/webgl-noise/webdemo/
