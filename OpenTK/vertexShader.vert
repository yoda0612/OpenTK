#version 450 core

layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;
layout(location = 2) in vec3 aNormal;

//out vec4 vs_color;

layout (location = 21) uniform  mat4 modelView;
layout(location = 20) uniform  mat4 projection;
layout(location = 22) uniform  mat4 model;

out vec3 Normal;
out vec3 FragPos;

void main(void)
{
 gl_Position = model * projection * modelView * position;
 //vs_color = color;
 FragPos = vec3(position * model);
 Normal = aNormal * mat3(transpose(inverse(model)));
 //Normal = aNormal * mat3(transpose(inverse(model)));

}