#ifndef INTERRIOR_UV_FUNCTION_INCLUDED
#define INTERRIOR_UV_FUNCTION_INCLUDED



float2 ConvertOriginalRawUVToInteriorUV(float2 originalRawUV, float3 viewDirTangentSpace, float roomMaxDepth01Define)
{

	float depthScale = rcp(roomMaxDepth01Define) - 1.0;

	float3 viewRayStartPosBoxSpace = float3(originalRawUV * 2 - 1, -1); 
	float3 viewRayDirBoxSpace = viewDirTangentSpace * float3(1, 1, -depthScale);

	float3 viewRayDirBoxSpaceRcp = rcp(viewRayDirBoxSpace);

	
	float3 hitRayLengthForSeperatedAxis = abs(viewRayDirBoxSpaceRcp) - viewRayStartPosBoxSpace * viewRayDirBoxSpaceRcp;
	float shortestHitRayLength = min(min(hitRayLengthForSeperatedAxis.x, hitRayLengthForSeperatedAxis.y), hitRayLengthForSeperatedAxis.z);
	float3 hitPosBoxSpace = viewRayStartPosBoxSpace + shortestHitRayLength * viewRayDirBoxSpace;



	float interp = hitPosBoxSpace.z * 0.5 + 0.5;

	float realZ = saturate(interp) / depthScale + 1;
	interp = 1.0 - (1.0 / realZ);
	interp *= depthScale + 1.0;

	float2 interiorUV = hitPosBoxSpace.xy * lerp(1.0, 1 - roomMaxDepth01Define, interp);

	interiorUV = interiorUV * 0.5 + 0.5;
	return interiorUV;
}
#endif