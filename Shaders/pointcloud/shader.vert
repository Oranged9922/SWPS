#version 450 core

layout(location = 0) in vec3 aPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 minBound;
uniform vec3 maxBound;

out vec3 texCoord;

void main()
{
    vec3 normalizedPos = (aPosition - minBound) / (maxBound - minBound);
    texCoord = normalizedPos;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    
    gl_PointSize = 5;
}
