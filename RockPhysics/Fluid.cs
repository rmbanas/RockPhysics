using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockPhysics
{
    class Fluid
    {
        public double rhowater { get; set; }
        public double kwater { get; set; }
        public double rhooil { get; set; }
        public double koil { get; set; }
        public double rhogas { get; set; }
        public double kgas { get; set; }

        public Fluid()
        {
            kwater = 2.7;
            rhowater = 1.05;

            koil = 1.1;
            rhooil = 0.7;

            kgas = 0.04;
            rhogas = 0.1;
        }

        public double kliquid(double sw, double so)
        {
            return Math.Pow(sw / kwater + so / koil, -1);
        }

        public double patchy(double sw, double so, double sg)
        {
            return sw * kwater + so * koil + sg * kgas;
        }

        public double homo(double sw, double so, double sg)
        {
            return Math.Pow(sw / kwater + so / koil + sg / kgas, -1);
        }

        public double brie(double sw, double so, double sg)
        {
            return (kliquid(sw, so) - kgas) * Math.Pow(1 - sg, 3) + kgas;
        }

        public double density_voight(double sw, double so, double sg)
        {
            return sw * rhowater + so * rhooil + sg * rhogas;
        }

        public double density_reuss(double sw, double so, double sg)
        {
            return Math.Pow(sw / rhowater + so / rhooil + sg / rhogas, -1);
        }
    }
}
