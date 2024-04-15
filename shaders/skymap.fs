#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform vec2 iResolution;
uniform float iTime;

out vec4 finalColor;

// 3D Gradient noise from: https://www.shadertoy.com/view/Xsl3Dl
vec3 hash( vec3 p ) // replace this by something better
{
	p = vec3( dot(p,vec3(127.1,311.7, 74.7)),
			  dot(p,vec3(269.5,183.3,246.1)),
			  dot(p,vec3(113.5,271.9,124.6)));

	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}

float Hash21(vec2 p ){
    p = fract(p*vec2(123.234, 234.34));
    p += dot(p, p+213.42);
    
    return fract(p.x*p.y);
}
float noise( in vec3 p )
{
    vec3 i = floor( p );
    vec3 f = fract( p );
	
	vec3 u = f*f*(3.0-2.0*f);

    return mix( mix( mix( dot( hash( i + vec3(0.0,0.0,0.0) ), f - vec3(0.0,0.0,0.0) ), 
                          dot( hash( i + vec3(1.0,0.0,0.0) ), f - vec3(1.0,0.0,0.0) ), u.x),
                     mix( dot( hash( i + vec3(0.0,1.0,0.0) ), f - vec3(0.0,1.0,0.0) ), 
                          dot( hash( i + vec3(1.0,1.0,0.0) ), f - vec3(1.0,1.0,0.0) ), u.x), u.y),
                mix( mix( dot( hash( i + vec3(0.0,0.0,1.0) ), f - vec3(0.0,0.0,1.0) ), 
                          dot( hash( i + vec3(1.0,0.0,1.0) ), f - vec3(1.0,0.0,1.0) ), u.x),
                     mix( dot( hash( i + vec3(0.0,1.0,1.0) ), f - vec3(0.0,1.0,1.0) ), 
                          dot( hash( i + vec3(1.0,1.0,1.0) ), f - vec3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

void main()
{
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = gl_FragCoord.xy/iResolution.xy;
    
    // incoorperate coord if want to pixelate
    //float dx = 2.* (1.0 / iResolution.x);
    //float dy = 2.* (1.0 / iResolution.y);
    //vec2 coord = vec2(dx * floor(uv.x / dx), dy * floor(uv.y / dy));

    // Stars computation:
    vec3 stars_direction = vec3(uv * 2.f - 1.0f, 1.0f); // if you change the 2.f next to uv the size of the stars changes
	float stars_threshold = 8.0f; // modifies the number of stars that are visible
	float stars_exposure = 200.0f; // modifies the overall strength of the stars
	float stars = pow(clamp(noise(stars_direction * 200.0f), 0.0f, 1.0f), stars_threshold) * stars_exposure;
	
    float r = Hash21(vec2(stars));
    
    //stars *= mix(sin(r*6.283185), 2.4, noise(stars_direction * 100.0f + vec3(iTime/2.))); // time based flickering
    //stars *= mix(sin(r*6.283185)*2., 2.4, noise(stars_direction * 100.0f + vec3(iTime/2.))); //i think this is too much flickering
    //stars *= mix(1.-0.5*sin(r*6.283185), 2.4, noise(stars_direction * 100.0f + vec3(iTime/2.))); 

    //stars *= mix(.4, 2.4, noise(stars_direction * 100.0f + vec3(iTime/8.)));  //good THIS IS WAY BETTER IF YOU WANT TO ZOOM BECAUSE IT EFFECTS THE STARS MORE
  
    //stars *= mix(.4, 2.4, noise(stars_direction * 100.0f + vec3(iTime/2.)))/4.; // time based flickering
    //stars *= mix(0.4, 1.4, noise(stars_direction * 200.0f + vec3(iTime/4.)));
    //stars *= 1. - sin(2.*pow(sin(r*6.283185*iTime/4.), 2.));
    
    stars *= 1. - sin(2.*pow(sin(r*6.283185*iTime/4.), 2.))/2.; // IF ZOOMED STARS 0.5f YOU CCAN BARELY SEE IT
    
    // Output to screen
    finalColor = vec4(vec3(stars),1.0);

}

// https://www.shadertoy.com/view/NtsBzB
