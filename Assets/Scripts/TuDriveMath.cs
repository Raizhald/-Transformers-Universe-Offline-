using System;
public static class TuDriveMath
{
    // Arcade steering stays responsive at road speed, with identical limits in reverse.
    public static float SteeringLimit(float metresPerSecond,float wheelbase)
    {
        if(wheelbase<=0)throw new ArgumentOutOfRangeException("wheelbase");
        double t=Math.Min(1,Math.Abs(metresPerSecond)/25);return (float)(34-24*t);
    }
}
