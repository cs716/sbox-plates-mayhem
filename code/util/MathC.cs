using System;
using System.Security.Cryptography;
using Sandbox;

namespace Pl8Mayhem.util;

public abstract class MathC
{
	public static float Lerp(float numFrom, float numTo, float by){
		return numFrom * (1 - by) + numTo * by;
	}

	public static Vector3 Lerp(Vector3 vecFrom, Vector3 vecTo, float by){
		return new Vector3(
			Lerp(vecFrom.x, vecTo.x, by),
			Lerp(vecFrom.y, vecTo.y, by),
			Lerp(vecFrom.z, vecTo.z, by)
		);
	}

	public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax)
	{
		if(outputMin > outputMax)
		{
			(inputMin, inputMax) = (inputMax, inputMin);
		}
		var val = (value - inputMin) / (inputMax - inputMin);
		if(outputMin > outputMax)
		{
			return outputMin + ((1f-val) * (outputMax - outputMin));
		}
		else
		{
			return outputMin + (val * (outputMax - outputMin));
		}
	}
}
