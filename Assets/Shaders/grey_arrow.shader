shader_type canvas_item;

uniform bool blockAction = true;

void fragment(){
	COLOR = texture(TEXTURE, UV);	
	if(blockAction){
		float intensity =  sqrt( COLOR.r*COLOR.r + COLOR.g*COLOR.g + COLOR.b*COLOR.b )/3.;
		COLOR = vec4(intensity, intensity, intensity, pow(COLOR.a,1));
	}
}