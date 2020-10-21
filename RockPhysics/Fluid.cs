using System;

namespace RockPhysics
{
    /// <summary>
    /// Need to fix the density bit I think, not appropriate to mix like this?
    /// </summary>
    class Fluid
    {
        // Fluid variables
        public double rhowater { get; set; }
        public double kwater { get; set; }

        public double rhooil { get; set; }
        public double koil { get; set; }

        public double rhogas { get; set; }
        public double kgas { get; set; }

        /// <summary>
        /// Generic constructor
        /// </summary>
        public Fluid()
        {
            // Initialize with random parameters
            kwater = 2.7;
            rhowater = 1.05;

            koil = 1.1;
            rhooil = 0.7;

            kgas = 0.04;
            rhogas = 0.1;
        }

        /// <summary>
        /// Return 'KLiquid' for Brie average
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <returns></returns>
        public double kliquid(double sw, double so)
        {
            return Math.Pow(sw / kwater + so / koil, -1);
        }

        /// <summary>
        /// Patchy saturation (linear) Voight / geometric average
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <param name="sg"></param>
        /// <returns></returns>
        public double patchy(double sw, double so, double sg)
        {
            return sw * kwater + so * koil + sg * kgas;
        }

        /// <summary>
        /// Homogenous saturation, Reuss / harmonic average
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <param name="sg"></param>
        /// <returns></returns>
        public double homo(double sw, double so, double sg)
        {
            return Math.Pow(sw / kwater + so / koil + sg / kgas, -1);
        }

        /// <summary>
        /// Brie / patchy + homo mix
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <param name="sg"></param>
        /// <returns></returns>
        public double brie(double sw, double so, double sg)
        {
            return (kliquid(sw, so) - kgas) * Math.Pow(1 - sg, 3) + kgas;
        }

        /// <summary>
        /// Compute geometric/voight average for fluid density
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <param name="sg"></param>
        /// <returns></returns>
        public double density_voight(double sw, double so, double sg)
        {
            return sw * rhowater + so * rhooil + sg * rhogas;
        }

        /// <summary>
        /// Compute harmonic average/Reuss for fluid density
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="so"></param>
        /// <param name="sg"></param>
        /// <returns></returns>
        public double density_reuss(double sw, double so, double sg)
        {
            return Math.Pow(sw / rhowater + so / rhooil + sg / rhogas, -1);
        }
    }
}
