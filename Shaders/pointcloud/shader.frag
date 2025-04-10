#version 450 core

in vec3 texCoord;
out vec4 FragColor;

uniform sampler3D volumeTexture;

void main()
{
    vec4 texColor = texture(volumeTexture, texCoord);
    
    FragColor = texColor;
}
