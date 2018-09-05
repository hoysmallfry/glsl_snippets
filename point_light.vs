/*------------------------------------------------------------------------------------------------------------
All content © 2011 DigiPen (USA) Corporation, all rights reserved.
Developed by Team Edge. (http://www.teamedgegames.com)

Author: Joel Barba
Purpose: Vertex shader for a deferred point light.
Transforms the vertex data to the proper spaces for the geometry shader.
------------------------------------------------------------------------------------------------------------*/

#version 120
#extension GL_EXT_gpu_shader4 : require

// varying
varying vec3 vVertexPos;
varying vec4 vColor;

// function definitions
void color();
void vertex_camera_coordinates();
void vertex_screen_coordinates();

void main()
{
	color();
	vertex_camera_coordinates();
	vertex_screen_coordinates();
}

void color()
{
	//gets the front color
	vColor = gl_Color;
}
void vertex_camera_coordinates()
{
	//gets the position of the vertex in camera coordinates.
	vVertexPos = vec3(gl_ModelViewMatrix * gl_Vertex);
}
void vertex_screen_coordinates()
{
	//gets the position of the vertex on the screen.
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}
