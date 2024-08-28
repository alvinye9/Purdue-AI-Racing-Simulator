using System;

public class NoiseGenerator
{
    public Random random;
    public double mean;
    public double variance;

    public NoiseGenerator(double mean, double variance, int seed)
    {
        this.mean = mean;
        this.variance = variance;
        this.random = new Random(seed);
    }

    public double NextGaussian()
    {
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();
        //Box-Muller Transform
        double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2); 
        return mean + z0 * Math.Sqrt(variance);
    }

    // Method to generate uniform noise
    public double NextUniform(double min, double max)
    {
        return min + (random.NextDouble() * (max - min));
    }
    
}


