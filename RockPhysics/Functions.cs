namespace RockPhysics
{
    class Functions
    {
        /// <summary>
        /// Replicate Python's LinSpace function to generate populated arrays
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public double[] LinSpace(double start, double end, int step)
        {
            double[] arr = new double[step];
            for (int i = 0; i < step; i++)
            {
                arr[i] = i * ((end - start) / (step - 1));
            }
            return arr;
        }
    }
}
