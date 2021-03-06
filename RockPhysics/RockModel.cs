﻿using System;

namespace RockPhysics
{
    class RockModel
    {
        // Elastic property outputs
        public double[] vp { get; set; }
        public double[] vs { get; set; }
        public double[] vpvs { get; set; }
        public double[] pr { get; set; }
        public double[] rhob { get; set; }
        public double[] phi { get; set; }
        public double[] ai { get; set; }
        public double[] si { get; set; }

        // Rock frame outputs for any model (generic)
        public double[] kdry { get; set; }
        public double[] gdry { get; set; }
        public double[] ksat { get; set; }
        public double[] ksolid { get; set; }
        public double[] gsolid { get; set; }

        // Outputs specific to bounds
        public double[] kdry_upper { get; set; }
        public double[] kdry_lower { get; set; }
        public double[] gdry_upper { get; set; }
        public double[] gdry_lower { get; set; }

        // Inputs
        public double phic { get; set; }    // Critical porosity
        public double sw { get; set; }      // Saturations
        public double so { get; set; }
        public double sg { get; set; }
        public double p { get; set; }       // Pressure GPa
        public double gd { get; set; }      // Grain density

        public int ns { get; set; }         // Number of samples in the output data

        // Objects
        public Fluid fluid { get; set; }

        private Functions f;

        /// <summary>
        /// Generic constructor
        /// </summary>
        public RockModel()
        {
            fluid = new Fluid();
            f = new Functions();

            gd = 2.65;
            sw = 1;
            so = 0;
            sg = 0;
        }

        /// <summary>
        /// Coordination number
        /// </summary>
        /// <param name="phi"></param>
        /// <returns></returns>
        public double cn(double phi)
        {
            return 20 - 34 * phi + 14 * Math.Pow(phi, 2);
        }

        /// <summary>
        /// Friable sand model
        /// </summary>
        /// <param name="kmin"></param>
        /// <param name="gmin"></param>
        /// <param name="p"></param>
        /// <param name="v"></param>
        /// <param name="phi_c"></param>
        public void FriableSand(double kmin, double gmin, double p, double phi_c)
        {
            try
            {
                phi = f.LinSpace(0, phi_c, ns);
                rhob = new double[ns];
                kdry = new double[ns];
                gdry = new double[ns];
                kdry_upper = new double[ns];
                kdry_lower = new double[ns];
                gdry_upper = new double[ns];
                gdry_lower = new double[ns];
                ksat = new double[ns];

                double z1 = 0;
                double z2 = 0;

                double v = 0.5 * ((kmin / gmin - (2.0 / 3.0)) / (kmin / gmin + (1.0 / 3.0)));
                double n_c = cn(phi_c);
                double khm = Math.Pow(p * (Math.Pow(n_c, 2) * Math.Pow(1 - phi_c, 2) * Math.Pow(gmin, 2)) / (18 * Math.Pow(Math.PI, 2) * Math.Pow(1 - v, 2)), 0.33333);
                double a = (5 - 4 * v) / (5 * (2 - v));
                double b = (3 * Math.Pow(n_c, 2) * Math.Pow(1 - phi_c, 2) * Math.Pow(gmin, 2)) * p;
                double c = 2 * Math.Pow(Math.PI, 2) * Math.Pow(1 - v, 2);
                double ghm = 0.5 * a * Math.Pow((b / c), 0.33333);                 // 0.5 is slip factor Per Avseth

                for (int i = 0; i < ns; i++)
                {
                    // Lower bound
                    a = ((phi[i] / phi_c) / (khm + 4 * ghm / 3)) + ((1 - phi[i] / phi_c) / (kmin + 4 * ghm / 3));
                    b = 4 * ghm / 3;
                    kdry_lower[i] = Math.Pow(a, -1) - b;
                    z1 = (ghm / 6) * ((9 * khm + 8 * ghm) / (khm + 2 * ghm));

                    a = (phi[i] / phi_c) / (ghm + z1);
                    b = (1 - phi[i] / phi_c) / (gmin + z1);
                    gdry_lower[i] = Math.Pow(a + b, -1) - z1;

                    // Upper bound
                    a = ((phi[i] / phi_c) / (khm + 4 * gmin / 3)) + ((1 - phi[i] / phi_c) / (kmin + 4 * gmin / 3));
                    b = 4 * gmin / 3;
                    kdry_upper[i] = Math.Pow(a, -1) - b;
                    z2 = (gmin / 6) * ((9 * kmin + 8 * gmin) / (kmin + 2 * gmin));
                    a = (phi[i] / phi_c) / (ghm + z2);
                    b = (1 - phi[i] / phi_c) / (gmin + z2);
                    gdry_upper[i] = Math.Pow(a + b, -1) - z2;

                    // Assign variables
                    ksat[i] = K_sat(kdry_upper[i], phi[i], kmin, fluid.homo(1, 0, 0));
                    kdry[i] = kdry_upper[i];
                    gdry[i] = gdry_upper[i];
                    rhob[i] = MakeDensity(phi[i]);
                }
            }
            catch (Exception ex)
            {
            }

        }

        /// <summary>
        /// Contact cement model
        /// </summary>
        /// <param name="cemsch"></param>
        /// <param name="Gc"></param>
        /// <param name="G"></param>
        /// <param name="nu"></param>
        /// <param name="nuC"></param>
        /// <param name="phi_c"></param>
        public void ContactCement(bool cemsch, double G, double Gc, double K, double Kc, double phi_c)
        {
            ksat = new double[ns];
            phi = new double[ns];
            rhob = new double[ns];
            gdry = new double[ns];
            kdry = new double[ns];
            ksolid = new double[ns];
            gsolid = new double[ns];

            double C = cn(phi_c);
            double nuC = 0.5 * ((Kc / Gc - (2.0 / 3.0)) / (Kc / Gc + (1.0 / 3.0)));

            for (int i = 0; i < ns; i++)
            {
                phi[i] = phi_c - (i * phi_c / (ns-1));                                  // Porosity
                double fc = phi_c - phi[i];                                             // Porosity fraction of cement (critical - phi) 
                double fgs = (1 - phi_c) / (1 - phi[i]);                                // Percentage of rock matrix to total rock solid
                double fcs = (fc) / (1 - phi[i]);                                       // Percentage of rock cement to total rock solid
                ksolid[i] = (fgs * K + fcs * Kc + 1.0 / (fgs / K + fcs / Kc)) / 2.0;    // Bulk mod of solid 
                gsolid[i] = (fgs * G + fcs * Gc + 1.0 / (fgs / G + fcs / Gc)) / 2.0;    // Shear mod of solid 
                //double Ms = Ks + 4.0 * Gs / 3.0;

                // This is of the SOLID, not the mineral (including cement)
                double nu = 0.5 * ((ksolid[i] / gsolid[i] - (2.0 / 3.0)) / (ksolid[i] / gsolid[i] + (1.0 / 3.0)));

                // Two cement schemes
                // First one (0.25 power) assumes all cement depsoited at grain contacts
                // Second one, cement is deposited evenly on the grain surfaces
                double a = cemsch ? 2.0 * Math.Pow(((phi_c - phi[i]) / (3.0 * C * (1 - phi_c))), 0.25) : Math.Pow((2.0 / 3.0) * ((phi_c - phi[i]) / (1 - phi_c)), 0.5);

                //Capital Lambdas
                double alam = 2.0 * Gc * (1.0 - nu) * (1.0 - nuC) / (Math.PI * gsolid[i] * (1.0 - 2.0 * nuC));
                double alamtau = Gc / (Math.PI * gsolid[i]);

                // Effective bulk modulus
                double r1 = Kc + 4.0 * Gc / 3.0;
                double r2 = C * (1.0 - phi_c);
                double r3 = -0.024153 * Math.Pow(alam, -1.3646) * Math.Pow(a, 2) + 0.20405 * Math.Pow(alam, -0.89008) * a + 0.00024649 * Math.Pow(alam, -1.9864);
                kdry[i] = (r1 * r2 * r3) / 6.0;

                // Effective shear modulus
                r1 = Gc/20;
                r2 = 3 * C * (1.0 - phi_c);
                double a1t = Math.Pow(-10,-2) * (2.2606 * Math.Pow(nu, 2) + 2.0696 * nu + 2.2952);
                double a2t = 0.07901 * Math.Pow(nu, 2) + 0.17539 * nu - 1.3418;
                double b1t = 0.05728 * Math.Pow(nu, 2) + 0.09367 * nu + 0.20162;
                double b2t = 0.02742 * Math.Pow(nu, 2) + 0.05286 * nu - 0.87653;
                double c1t = Math.Pow(10,-4) * (9.6544 * Math.Pow(nu, 2) + 4.9445 * nu + 3.1008);
                double c2t = 0.01867 * Math.Pow(nu, 2) + 0.4011 * nu - 1.8186;
                r3 = a1t * Math.Pow(alamtau, a2t) * Math.Pow(a, 2) + b1t * Math.Pow(alamtau, b2t) * a + c1t * Math.Pow(alamtau, c2t);
                gdry[i] = (3.0 * kdry[i] / 5.0) + (r1 * r2 * r3);
                //double Mframe = Kframe + 4 * Gframe / 3;

                // Gassmann
                //ksat[i] = ksolid[i] * (phi[i] * kdry[i] - (1 + phi[i]) * fluid.homo(1.0,0,0) * kdry[i] / ksolid[i] + fluid.homo(1.0,0,0) / ((1 - phi[i]) * fluid.homo(1.0, 0, 0) + phi[i] * ksolid[i] - fluid.homo(1.0, 0, 0) * kdry[i] / ksolid[i]));
                //double Msat = ksolid[i]at[i] + 4 * Gframe / 3;

                // Assign variables
                ksat[i] = K_sat(kdry[i], phi[i], ksolid[i], fluid.homo(1.0, 0, 0));
                rhob[i] = MakeDensity(phi[i]);

                //ComputeRP(Ksat, Gframe, MakeDensity(phi[i], rhofl, 2.65), ref RPout);

                //double nuSat = 0.5 * (Msat / Gframe - 2) / (Msat / Gframe - 1);
                //double VpVs = Math.Pow((Msat / Gframe), 0.5);
                //Msat2 = Ms * (phi_0 * Mframe - (1 + phi_0) * Kf * Mframe / Ms + Kf) / ((1 - phi_0) * Kf + phi_0 * Ms - Kf * Mframe / Ms);
            }
        }


        /// <summary>
        /// Gassmann
        /// </summary>
        /// <param name="K_dry"></param>
        /// <param name="Phi"></param>
        /// <param name="K0"></param>
        /// <param name="K_fl"></param>
        /// <returns></returns>
        public double K_sat(double K_dry, double Phi, double K0, double K_fl)
        {
            double numer = Math.Pow((1 - (K_dry / K0)), 2);
            double denom = Phi / K_fl + (1 - Phi) / K0 - K_dry / Math.Pow(K0, 2);

            //to ensure we're not dividing by zero when porosity is set to 0...which happens when porosity is set to 0
            if (denom > 0)
            {
                return K_dry + (numer / denom);
            }
            else
            {
                return K_dry;
            }
        }

        /// <summary>
        /// Create bulk density from porosity with fluid density and grain density
        /// </summary>
        public double MakeDensity(double phi)
        {
            return gd * (1 - phi) + fluid.density_reuss(sw, so, sg) * phi;
        }

        /// <summary>
        /// Compute rock physics elastic properties
        /// </summary>
        public void ComputeRP()
        {
            vp = new double[ns];
            vs = new double[ns];
            vpvs = new double[ns];
            pr = new double[ns];
            ai = new double[ns];
            si = new double[ns];

            for (int i = 0; i < ksat.Length; i++)
            {
                vp[i] = Math.Pow((ksat[i] + (4 / 3) * gdry[i]) / rhob[i], 0.5);
                vs[i] = Math.Pow(gdry[i] / rhob[i], 0.5);
                vpvs[i] = vp[i] / vs[i];
                pr[i] = 0.5 * (Math.Pow(vpvs[i], 2) - 2) / (Math.Pow(vpvs[i], 2) - 1);
                ai[i] = rhob[i] * vp[i];
                si[i] = rhob[i] * vs[i];
            }
        }

    }
}
