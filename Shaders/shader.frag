#version 440 core

in float fragDistance;
out vec4 outputColor;

uniform vec4 ourColor;

void main()
{
    float brightness = 1.0 + fragDistance;
    outputColor = ourColor * brightness;
}
