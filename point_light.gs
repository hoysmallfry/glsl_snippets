/*------------------------------------------------------------------------------------------------------------
All content © 2011 DigiPen (USA) Corporation, all rights reserved.
Developed by Team Edge. (http://www.teamedgegames.com)

Author: Joel Barba
Purpose: Geometry shader for a deferred point light.
Creates a quad in screen space from one-vertex geometry.

------------------------------------------------------------------------------------------------------------*/

#version 120
#extension GL_EXT_gpu_shader4 : require
#extension GL_EXT_geometry_shader4 : require

#define ATTENUATION_CUTOFF 0.1

// these come in from the vertex shader.
varying in vec3 vVertexPos[1];
varying in vec4 vColor[1];

// these go out to the fragment shader.
varying out vec2 vTexCoord;

void create_vertex(float x, float y, float z);

// a sprite (a quad billboard) is created for every point passed in.
void main()
{

	float z = 1.0 / ATTENUATION_CUTOFF + gl_LightSource[0].constantAttenuation;

	float x = (gl_LightSource[0].linearAttenuation != 0.0) ?
		z / gl_LightSource[0].linearAttenuation : 0.0;

	float y = (gl_LightSource[0].quadraticAttenuation != 0.0) ?
		sqrt(z / gl_LightSource[0].quadraticAttenuation) : 0.0;

	float extent = max(x, y);

	// lower left. Each light casts itself through roughly a 1 ft radius sphere.
	create_vertex(-extent, -extent, extent);

	// lower right.
	create_vertex(extent, -extent, extent);

	// upper left.
	create_vertex(-extent, extent, extent);

	// upper right.
	create_vertex(extent, extent, extent);

	EndPrimitive();
}

vec2 coord_transform()
{
	return gl_Position.xy * 0.5 + 0.5;
}

void create_vertex(float x, float y, float z)
{
	gl_Position = gl_ProjectionMatrix * vec4(vVertexPos[0] + vec3(x, y, z), 1.0);
	gl_Position /= gl_Position.w;

	// Depth is clamped to the near plane, to ensure it doesn't get culled while influencing the scene.
	gl_Position.z = max(vVertexPos[0].z, -.99);

	// generate texture coordinates.
	vTexCoord = coord_transform();

	// set the front color of the geometry.
	gl_FrontColor = vColor[0];

	EmitVertex();
}
