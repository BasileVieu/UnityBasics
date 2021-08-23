Shader "Fractal/Fractal Surface GPU"
{
	SubShader
	{
		CGPROGRAM
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation

		#pragma target 4.5
		
		#include "FractalJobsGPU.hlsl"

		struct Input
		{
			float3 worldPos;
		};

		float _Smoothness;

		void ConfigureSurface (Input _input, inout SurfaceOutputStandard _surface)
		{
			_surface.Albedo = GetFractalColor().rgb;
			_surface.Smoothness = GetFractalColor().a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}