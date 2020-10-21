using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockPhysics
{
    class RockModel
    {
        public double[] vp          {get; set;}
        public double[] vs          {get; set;}
        public double[] vpvs        {get; set;}
        public double[] pr          {get; set;}
        public double[] kdry        {get; set;}
        public double[] kdry_upper  {get; set;}
        public double[] kdry_lower  {get; set;}
        public double[] ksat        {get; set;}
        public double[] g           {get; set;}
        public double[] g_upper     {get; set;}
        public double[] g_lower     {get; set;}
        public double[] rhob        {get; set;}
        public double[] phi         {get; set;}
        public double[] ai          {get; set;}
        public double[] si          {get; set;}

        public double phic          {get; set;} 
        public double sw            {get; set;}
        public double so { get; set; }
        public double sg { get; set; }
        public double p             {get; set;}
        public Fluid fluid          {get; set;}
        public double gd            { get; set; }

        public RockModel()
        {
            fluid = new Fluid();
            gd = 2.65;
            sw = 1;
            so = 0;
            sg = 0;
        }

        // Copy constructor
        public RockModel(RockModel m)
        {
            fluid = new Fluid();
            // Need to finish this
        }

        public int ns { get; set; }

        public double cn(double phi)
        {
            return 20 - 34 * phi + 14 * Math.Pow(phi, 2);
        }

        public void FriableSand(double kmin, double gmin, double p, double v, double phi_c)
        {
            try
            {
                rhob = new double[ns];

                phi = LinSpace(0, phi_c, ns);
                kdry_upper = new double[ns];
                kdry_lower = new double[ns];
                g_upper = new double[ns];
                g_lower = new double[ns];
                ksat = new double[ns];
                    
                double z1 = 0;
                double z2 = 0;

                double n_c = cn(phi_c);
                double khm = Math.Pow(p * (Math.Pow(n_c, 2) * Math.Pow(1 - phi_c, 2) * Math.Pow(gmin, 2)) / (18 * Math.Pow(Math.PI, 2) * Math.Pow(1 - v, 2)), 0.33333);
                double a = (5 - 4 * v) / (5 * (2 - v));
                double b = (3 * Math.Pow(n_c, 2) * Math.Pow(1 - phi_c, 2) * Math.Pow(gmin, 2)) * p;
                double c = 2 * Math.Pow(Math.PI, 2) * Math.Pow(1 - v, 2);
                // 0.5 is slip factor Per Avseth
                double ghm = 0.5 * a * Math.Pow((b / c), 0.33333);

                for (int i = 0; i < ns; i++)
                {
                    // Lower bound
                    a = ((phi[i] / phi_c) / (khm + 4 * ghm / 3)) + ((1 - phi[i] / phi_c) / (kmin + 4 * ghm / 3));
                    b = 4 * ghm / 3;
                    kdry_lower[i] = Math.Pow(a, -1) - b;
                    z1 = (ghm / 6) * ((9 * khm + 8 * ghm) / (khm + 2 * ghm));

                    a = (phi[i] / phi_c) / (ghm + z1);
                    b = (1 - phi[i] / phi_c) / (gmin + z1);
                    g_lower[i] = Math.Pow(a + b, -1) - z1;

                    // Upper bound
                    a = ((phi[i] / phi_c) / (khm + 4 * gmin / 3)) + ((1 - phi[i] / phi_c) / (kmin + 4 * gmin / 3));
                    b = 4 * gmin / 3;
                    kdry_upper[i] = Math.Pow(a, -1) - b;
                    z2 = (gmin / 6) * ((9 * kmin + 8 * gmin) / (kmin + 2 * gmin));
                    a = (phi[i] / phi_c) / (ghm + z2);
                    b = (1 - phi[i] / phi_c) / (gmin + z2);
                    g_upper[i] = Math.Pow(a + b, -1) - z2;

                    ksat[i] = K_sat(kdry_upper[i], phi[i], kmin, fluid.homo(1,0,0));
                    g = g_upper;
                    rhob[i] = MakeDensity(phi[i]);
                }
            }
            catch (Exception ex)
            {
            }

        }

        public void ContactCement(bool cemsch, double Gc, double G, double nu, double nuC, double phi_c)
        {
            ksat = new double[ns];
            phi = new double[ns];
            rhob = new double[ns];
            g = new double[ns];
            kdry = new double[ns];

            double C = cn(phi_c);
            double K = G * 2.0 * (1 + nu) / (3.0 * (1.0 - 2.0 * nu));
            double Kc = Gc * 2.0 * (1 + nuC) / (3.0 * (1.0 - 2.0 * nuC));

            for (int i = 0; i < ns; i++)
            {
                phi[i] = phi_c - (i - 1) * (phi_c) / (ns-1);
                //double fc = phi_c - phi[i];
                double fgs = (1 - phi_c) / (1 - phi[i]);                                 // Fraction of cement in the solid
                double fcs = (phi_c - phi[i]) / (1 - phi[i]);                             // Bulk modulus of the solid
                double Ks = (fgs * K + fcs * Kc + 1 / (fgs / K + fcs / Kc)) / 2.0;        // Shear modulus of the solid 
                double Gs = (fgs * G + fcs * Gc + 1 / (fgs / G + fcs / Gc)) / 2.0;        // M-modulus of the solid 
                //double Ms = Ks + 4 * Gs / 3;

                // Two cement schemes
                // First one (0.25 power) assumes all cement depsoited at grain contacts
                // Second one, cement is deposited evenly on the grain surfaces
                double a = cemsch ? 2.0 * Math.Pow(((phi_c - phi[i]) / (3.0 * C * (1 - phi_c))), 0.25) : Math.Pow((2.0/3.0) * ((phi_c - phi[i]) / (1 - phi_c)), 0.5);

                //Capital Lambdas
                double alam = 2.0*Gc* (1 - nu) * (1 - nuC) / (Math.PI*G*(1.0- 2.0 * nuC));
                double alamtau = Gc / (Math.PI*G);

                // Effective bulk modulus
                double r1 = Kc + 4.0 * Gc / 3.0;
                double r2 = C * (1.0 - phi_c);
                double r3 = -0.024153 * Math.Pow(alam, -1.3646) * Math.Pow(a, 2) + 0.20405 * Math.Pow(alam, -0.89008) * a + 0.00024649 * Math.Pow(alam, -1.9864);
                double Kframe = (r1 * r2 * r3)/6.0;

                // Effective shear modulus
                r1 = Gc;
                r2 = 3 * C * (1 - phi_c) / 20.0;
                double a1t = -0.01 * (2.2606 * Math.Pow(nu, 2) + 2.0696 * nu + 2.2952);
                double a2t = 0.079011 * Math.Pow(nu, 2) + 0.17539 * nu - 1.3418;
                double b1t = 0.05728 * Math.Pow(nu, 2) + 0.09367 * nu + 0.20162;
                double b2t = 0.027425 * Math.Pow(nu, 2) + 0.052859 * nu - 0.87653;
                double c1t = 0.0001 * (9.6544 * Math.Pow(nu, 2) + 4.9445 * nu + 3.1008);
                double c2t = 0.018667 * Math.Pow(nu, 2) + 0.4011 * nu - 1.8186;
                r3 = a1t * Math.Pow(alamtau, a2t) * Math.Pow(a, 2) + b1t * Math.Pow(alamtau, b2t) * a + c1t * Math.Pow(alamtau, c2t);
                double Gframe = (3.0 * Kframe/5.0) + r1 * r2 * r3;
                double Mframe = Kframe + 4 * Gframe / 3;

                // Gassmann
                ksat[i] = K_sat(Kframe, phi[i], Ks, fluid.homo(1.0,0,0));
                //ksat[i] = Ks * (phi[i] * Kframe - (1 + phi[i]) * fluid.kfl * Kframe / Ks + fluid.kfl) / ((1 - phi[i]) * fluid.kfl + phi[i] * Ks - fluid.kfl * Kframe / Ks);
                double Msat = ksat[i] + 4 * Gframe / 3;

                rhob[i] = MakeDensity(phi[i]);
                kdry[i] = Kframe;
                g[i] = Gframe;
                //ComputeRP(Ksat, Gframe, MakeDensity(phi[i], rhofl, 2.65), ref RPout);

                //double nuSat = 0.5 * (Msat / Gframe - 2) / (Msat / Gframe - 1);
                //double VpVs = Math.Pow((Msat / Gframe), 0.5);
                //Msat2 = Ms * (phi_0 * Mframe - (1 + phi_0) * Kf * Mframe / Ms + Kf) / ((1 - phi_0) * Kf + phi_0 * Ms - Kf * Mframe / Ms);
            }
        }

        public double[] LinSpace(double start, double end, int step)
        {
            double[] arr = new double[step];
            for (int i = 0; i < step; i++)
            {
                arr[i] = i * ((end - start) / (step-1));
            }
            return arr;
        }

        //Gassmann
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

        // Create bulk density from porosity value, fluid density and grain density
        public double MakeDensity(double phi)
        {
            return gd * (1 - phi) + fluid.density_reuss(sw,so,sg) * phi;
        }

        // Compute all the crap, moduli, vp, vs, etc.
        public void ComputeRP()
        {
            vp =    new double[ns];
            vs =    new double[ns];
            vpvs =  new double[ns];
            pr =    new double[ns];
            ai =    new double[ns];
            si =    new double[ns];

            for(int i = 0; i < ksat.Length; i++)
            {

               vp[i]    = Math.Pow((ksat[i] + (4 / 3) * g[i]) / rhob[i], 0.5);                 
               vs[i]    = Math.Pow(g[i] / rhob[i], 0.5);                                     
               vpvs[i]  = vp[i]/ vs[i];                                                
               pr[i]    = 0.5 * (Math.Pow(vpvs[i], 2) - 2) / (Math.Pow(vpvs[i], 2) - 1); 
               ai[i]    = rhob[i]*vp[i];                                                    
               si[i]    = rhob[i]*vs[i];                                                    
            }
        }

    }
}
