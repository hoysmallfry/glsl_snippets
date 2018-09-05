/*------------------------------------------------------------------------------------------------------------
All content © 2011 DigiPen (USA) Corporation, all rights reserved.
Developed by Team Edge. (http://www.teamedgegames.com)

Author: Joel Barba
Purpose: Fragment shader for a deferred point light.
Takes the g-buffers and light data and generates a fill for the light accumulation buffer.
------------------------------------------------------------------------------------------------------------*/

#version 120
#extension GL_ARB_draw_buffers : require
#extension GL_EXT_gpu_shader4 : require

// varying
varying vec2 vTexCoord;

// uniform
uniform sampler2D uNormalMap;
uniform sampler2D uMaterialMap;
uniform sampler2D uPositionMap;
uniform sampler2D uSurfaceMap;

// global
const float gIntensity = 128.0;
const float gFalloff = 512.0;

// function definitions
vec4 color();
float cosine_angle(vec3 pVector1, vec3 pVector2);
float set_attenuation(float pDistance);

void main()
{
	vec4 material = texture2D(uMaterialMap, vTexCoord);

	if (material.a == 0)
		discard;

	vec4 normalSample = texture2D(uNormalMap, vTexCoord);
	vec3 normal = normalize(normalSample.xyz * 2.0 - 1.0);
	vec3 position = texture2D(uPositionMap, vTexCoord).xyz;

	vec3 lightDirection = gl_LightSource[0].position.xyz - position;
	float lightDistance = length(lightDirection);
	lightDirection = normalize(lightDirection);

	float cosAlpha = cosine_angle(normal, lightDirection);

	vec4 result;

	if (cosAlpha > 0)
	{
		float attenuation = set_attenuation(lightDistance);

		// Add ambience to color.
		vec4 ambientColor = gl_LightSource[0].ambient * material;
		result += attenuation * ambientColor;

		// Add diffuse to color.
		vec4 diffuseColor = gl_LightSource[0].diffuse * material;
		result += attenuation * (cosAlpha * diffuseColor);

		// Find the reflection vector.
		vec3 reflection = reflect(-lightDirection, normal);
		vec3 V = normalize(-position);
		float cosBeta = cosine_angle(reflection, V);

		vec2 specular = texture2D(uSurfaceMap, vTexCoord).yz;

		vec4 specularColor = gl_LightSource[0].specular * specular[0] * pow(cosBeta, specular[1]);

		result += attenuation * specularColor;
	}

	gl_FragColor = result;
}

vec4 color()
{
	// set color to the triangle vertex color.
	vec4 color = gl_Color;

	// multiply by the surface diffuse.
	color.rgb += gl_FrontMaterial.emission.rgb;

	color *= gl_FrontMaterial.ambient;
	color *= gl_FrontMaterial.diffuse;

	return color;
}

float cosine_angle(vec3 pVector1, vec3 pVector2)
{
	float cosineAngle = dot(pVector1, pVector2);
	return max(cosineAngle, 0);
}

float set_attenuation(float pDistance)
{
	float attenuation = gl_LightSource[0].constantAttenuation
		+ gl_LightSource[0].linearAttenuation * pDistance
		+ gl_LightSource[0].quadraticAttenuation * pDistance * pDistance;

	attenuation = 1.0 / attenuation;

	return min(attenuation, 1);
}
