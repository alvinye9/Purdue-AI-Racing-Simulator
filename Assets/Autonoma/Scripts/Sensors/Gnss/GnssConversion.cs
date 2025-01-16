/* 
Copyright 2023 Autonoma, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at:

    http://www.apache.org/licenses/LICENSE-2.0

The software is provided "AS IS", WITHOUT WARRANTY OF ANY KIND, 
express or implied. In no event shall the authors or copyright 
holders be liable for any claim, damages or other liability, 
whether in action of contract, tort or otherwise, arising from, 
out of or in connection with the software or the use of the software.
*/
using UnityEngine;
using VehicleDynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
public static class LatLonHeight2Enu
{
    static double earth_semimajor = 6378137;
    static double earth_eccentricity = 0.00669438;
    static double deg2rad = (double)(1 / (180.0 / Math.PI));

    public static double[] calcEnu(double lat, double lon, double h, double lat0,double lon0,double h0)
    {
        double clatRef = Math.Cos(lat0  * deg2rad);
        double clonRef = Math.Cos(lon0  * deg2rad);
        double slatRef = Math.Sin(lat0   * deg2rad);
        double slonRef = Math.Sin(lon0  * deg2rad);
        double clat    = Math.Cos(lat  * deg2rad);
        double clon    = Math.Cos(lon * deg2rad);
        double slat    = Math.Sin(lat  * deg2rad);
        double slon    = Math.Sin(lon * deg2rad);

        double r0Ref = earth_semimajor / (Math.Sqrt((1.0 - earth_eccentricity * slatRef * slatRef)));
        double[] ecefRef = new double[3];
        ecefRef[0] = (h0 + r0Ref) * clatRef * clonRef;
        ecefRef[1] = (h0 + r0Ref) * clatRef * slonRef;
        ecefRef[2] = (h0 + r0Ref * (1.0 - earth_eccentricity)) * slatRef;  

        double r0 = earth_semimajor / (Math.Sqrt((1.0 - earth_eccentricity * slat * slat)));
        double[] dECEF = new double[3];

        dECEF[0] = (h + r0) * clat * clon - ecefRef[0];              
        dECEF[1] = (h + r0) * clat * slon - ecefRef[1];                
        dECEF[2] = (h + r0 * (1.0 - earth_eccentricity)) * slat - ecefRef[2]; 

        double[,] R = new double[3,3]
        { {-slonRef, clonRef, 0.0},
          { -slatRef * clonRef, -slatRef * slonRef, clatRef},
          {clatRef * clonRef,  clatRef * slonRef, slatRef} };
        
        double[] enu = new double[3];
        for (int row = 0; row<3; row++)
        {   
            enu[row] = 0;
            for (int col = 0; col<3; col++)
            {
                enu[row] += R[row,col]*dECEF[col];
            }
        }
        return enu;
    }
}
public static class Enu2LatLonHeight
{
    public static double h0; 
    public static double lat0rad;
    public static double lon0rad;
    private static double earth_semimajor = 6378137;
    private static double earth_eccentricity = 0.00669438;
    private static double earth_semiminor = 6356752.3;
    private static double earth_eccen2 = earth_eccentricity / (1 - earth_eccentricity);
    private static double flattening = (earth_semimajor - earth_semiminor) / earth_semimajor; 
    private static double lat,lon,height;
    private static double east,north,up;
    private static double u,v,w,x0,y0,z0;
    private static double deg2rad = (double)(1 / (180.0 / Math.PI));
    private static double[] ecef = new double[3];
    private static void geodetic2ecef()
    {
        double N = Math.Pow(earth_semimajor,2)/ ( Math.Sqrt( Math.Pow(earth_semimajor,2) * Math.Pow(Math.Cos(lat0rad),2) 
                                                          +  Math.Pow(earth_semiminor,2) * Math.Pow(Math.Sin(lat0rad),2) ) );

        x0 = (N + h0) * Math.Cos(lat0rad) * Math.Cos(lon0rad);
        y0 = (N + h0) * Math.Cos(lat0rad) * Math.Sin(lon0rad);
        z0 = (N * Math.Pow(earth_semiminor / earth_semimajor,2)+ h0) * Math.Sin(lat0rad);
    }
    private static void enu2uvw()
    {
        double t = Math.Cos(lat0rad) * up - Math.Sin(lat0rad) * north;
        w = Math.Sin(lat0rad) * up + Math.Cos(lat0rad) * north;
        u = Math.Cos(lon0rad) * t - Math.Sin(lon0rad) * east;
        v = Math.Sin(lon0rad) * t + Math.Cos(lon0rad) * east;
    }
    private static void enu2ecef()
    {
        geodetic2ecef();
        enu2uvw();
        ecef[0] = x0 + u;
        ecef[1] = y0 + v;
        ecef[2] = z0 + w;
    }
    private static void ecef2geodetic()
    {
        double r = Math.Sqrt( ecef[0]*ecef[0] + ecef[1]*ecef[1] + ecef[2]*ecef[2] );
        double E = Math.Sqrt( Math.Pow(earth_semimajor,2) - Math.Pow(earth_semiminor,2) );
        double u = Math.Sqrt(0.5 * (r*r - E*E) + 0.5 * Math.Sqrt( Math.Pow(r*r - E*E,2)  + 4 * E * E * ecef[2]*ecef[2]));
        double Q = Math.Sqrt(ecef[0]*ecef[0] + ecef[1]*ecef[1]);
        double huE = Math.Sqrt(u*u + E*E);
        double Beta = 0;
        if (u == 0)
        {
            Beta = ecef[2]>=0 ? Math.PI/2 : -Math.PI/2;
        }
        else
        {
            Beta = Math.Atan(huE / u * ecef[2] / Q);
        }    
    
        double eps = ((earth_semiminor * u - earth_semimajor * huE + E * E) * Math.Sin(Beta)) 
            /(earth_semimajor * huE * 1 / Math.Cos(Beta) - E * E * Math.Cos(Beta) );

        Beta += eps;
        lat = HelperFunctions.rad2deg( Math.Atan(earth_semimajor / earth_semiminor * Math.Tan(Beta)) );
        lon = HelperFunctions.rad2deg( Math.Atan2(ecef[1], ecef[0]) );
        height = Math.Sqrt( Math.Pow( ecef[2] - earth_semiminor * Math.Sin(Beta),2) + Math.Pow(Q - earth_semimajor * Math.Cos(Beta),2) );
        bool inside = Math.Pow(ecef[0] * ecef[0] / earth_semimajor,2) + Math.Pow( ecef[1] * ecef[1] / earth_semimajor,2) + Math.Pow( ecef[2] * ecef[2] / earth_semiminor,2) < 1;
        height = inside ? -height : height; 
    }
    public static double[] calcLatLonHeight(Vector3 pos,double lat0, double lon0, double h0)
    {
        Enu2LatLonHeight.h0  = h0;
        lat0rad = deg2rad * lat0;
        lon0rad = deg2rad * lon0;
        east = (double)pos[0];
        north = (double)pos[1];
        up = (double)pos[2];

        enu2ecef();
        ecef2geodetic(); 
        double[] llh = new double[3];

        llh[0] = lat;
        llh[1] = lon;
        llh[2] = height;
        return llh;
    }

}

    public class LatLngUTMConverter
    {
        public class LatLng
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class UTMResult
        {
            public double Easting { get; set; }
            public double UTMEasting { get; set; }
            public double Northing { get; set; }
            public double UTMNorthing { get; set; }
            public int ZoneNumber { get; set; }
            public String ZoneLetter { get; set; }
            public String Zona
            {
                get
                {
                    return ZoneNumber + ZoneLetter;
                }
            }

            public override string ToString()
            {
                return "" + ZoneNumber + ZoneLetter + " " + Easting + "" + Northing;
            }
        }

        private double a;
        private double eccSquared;
        private bool status;
        private string datumName = "WGS 84";

    public LatLngUTMConverter(String datumNameIn)
    {
        if (!String.IsNullOrEmpty(datumNameIn))
        {
            datumName = datumNameIn;
        }

        this.setEllipsoid(datumName);
    }

    private double toRadians(double grad)
    {
        return grad * Math.PI / 180;
    }

    private String getUtmLetterDesignator(double latitude)
    {
        if ((84 >= latitude) && (latitude >= 72))
            return "X";
        else if ((72 > latitude) && (latitude >= 64))
            return "W";
        else if ((64 > latitude) && (latitude >= 56))
            return "V";
        else if ((56 > latitude) && (latitude >= 48))
            return "U";
        else if ((48 > latitude) && (latitude >= 40))
            return "T";
        else if ((40 > latitude) && (latitude >= 32))
            return "S";
        else if ((32 > latitude) && (latitude >= 24))
            return "R";
        else if ((24 > latitude) && (latitude >= 16))
            return "Q";
        else if ((16 > latitude) && (latitude >= 8))
            return "P";
        else if ((8 > latitude) && (latitude >= 0))
            return "N";
        else if ((0 > latitude) && (latitude >= -8))
            return "M";
        else if ((-8 > latitude) && (latitude >= -16))
            return "L";
        else if ((-16 > latitude) && (latitude >= -24))
            return "K";
        else if ((-24 > latitude) && (latitude >= -32))
            return "J";
        else if ((-32 > latitude) && (latitude >= -40))
            return "H";
        else if ((-40 > latitude) && (latitude >= -48))
            return "G";
        else if ((-48 > latitude) && (latitude >= -56))
            return "F";
        else if ((-56 > latitude) && (latitude >= -64))
            return "E";
        else if ((-64 > latitude) && (latitude >= -72))
            return "D";
        else if ((-72 > latitude) && (latitude >= -80))
            return "C";
        else
            return "Z";
    }

    public UTMResult convertLatLngToUtm(double latitude, double longitude)
    {
        if (status)
        {
            throw new Exception("No ecclipsoid data associated with unknown datum: " + datumName);
        }

        int ZoneNumber;

        var LongTemp = longitude;
        var LatRad = toRadians(latitude);
        var LongRad = toRadians(LongTemp);

        if (LongTemp >= 8 && LongTemp <= 13 && latitude > 54.5 && latitude < 58)
        {
            ZoneNumber = 32;
        }
        else if (latitude >= 56.0 && latitude < 64.0 && LongTemp >= 3.0 && LongTemp < 12.0)
        {
            ZoneNumber = 32;
        }
        else
        {
            ZoneNumber = (int) ((LongTemp + 180) / 6) + 1;

            if (latitude >= 72.0 && latitude < 84.0)
            {
                if (LongTemp >= 0.0 && LongTemp < 9.0)
                {
                    ZoneNumber = 31;
                }
                else if (LongTemp >= 9.0 && LongTemp < 21.0)
                {
                    ZoneNumber = 33;
                }
                else if (LongTemp >= 21.0 && LongTemp < 33.0)
                {
                    ZoneNumber = 35;
                }
                else if (LongTemp >= 33.0 && LongTemp < 42.0)
                {
                    ZoneNumber = 37;
                }
            }
        }

        var LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone
        var LongOriginRad = toRadians(LongOrigin);

        var UTMZone = getUtmLetterDesignator(latitude);

        var eccPrimeSquared = (eccSquared) / (1 - eccSquared);

        var N = a / Math.Sqrt(1 - eccSquared * Math.Sin(LatRad) * Math.Sin(LatRad));
        var T = Math.Tan(LatRad) * Math.Tan(LatRad);
        var C = eccPrimeSquared * Math.Cos(LatRad) * Math.Cos(LatRad);
        var A = Math.Cos(LatRad) * (LongRad - LongOriginRad);

        var M = a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * LatRad
                - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * LatRad)
                + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * LatRad)
                - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * LatRad));

        var UTMEasting = 0.9996 * N * (A + (1 - T + C) * A * A * A / 6
                + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
                + 500000.0;

        var UTMNorthing = 0.9996 * (M + N * Math.Tan(LatRad) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24
                + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720));

        if (latitude < 0)
            UTMNorthing += 10000000.0;

        return new UTMResult { Easting= UTMEasting, Northing= UTMNorthing, ZoneNumber= ZoneNumber, ZoneLetter= UTMZone};
    }

    public float computeMeridianConvergence(int zoneNumber, double latitude, double longitude)
    {
        // Step 1: Calculate Central Meridian (λ0) in degrees
        double lambda0 = (zoneNumber - 1) * 6 - 180 + 3; // Degrees

        // Step 2: Convert to Radians
        double latRad = latitude * Mathf.Deg2Rad;
        double lonRad = longitude * Mathf.Deg2Rad;
        double lambda0Rad = lambda0 * Mathf.Deg2Rad;

        // Step 3: Compute Meridian Convergence Angle (γ) in Radians
        double gammaRad = Mathf.Atan(Mathf.Tan((float)(lonRad - lambda0Rad)) * Mathf.Sin((float)latRad));

        // Step 4: Convert γ to Degrees
        float gammaDeg = (float)(gammaRad * Mathf.Rad2Deg);

        return gammaDeg;

    }

    private void setEllipsoid(String name)
    {
        switch (name)
        {
            case "Airy":
                a = 6377563;
                eccSquared = 0.00667054;
                break;
            case "Australian National":
                a = 6378160;
                eccSquared = 0.006694542;
                break;
            case "Bessel 1841":
                a = 6377397;
                eccSquared = 0.006674372;
                break;
            case "Bessel 1841 Nambia":
                a = 6377484;
                eccSquared = 0.006674372;
                break;
            case "Clarke 1866":
                a = 6378206;
                eccSquared = 0.006768658;
                break;
            case "Clarke 1880":
                a = 6378249;
                eccSquared = 0.006803511;
                break;
            case "Everest":
                a = 6377276;
                eccSquared = 0.006637847;
                break;
            case "Fischer 1960 Mercury":
                a = 6378166;
                eccSquared = 0.006693422;
                break;
            case "Fischer 1968":
                a = 6378150;
                eccSquared = 0.006693422;
                break;
            case "GRS 1967":
                a = 6378160;
                eccSquared = 0.006694605;
                break;
            case "GRS 1980":
                a = 6378137;
                eccSquared = 0.00669438;
                break;
            case "Helmert 1906":
                a = 6378200;
                eccSquared = 0.006693422;
                break;
            case "Hough":
                a = 6378270;
                eccSquared = 0.00672267;
                break;
            case "International":
                a = 6378388;
                eccSquared = 0.00672267;
                break;
            case "Krassovsky":
                a = 6378245;
                eccSquared = 0.006693422;
                break;
            case "Modified Airy":
                a = 6377340;
                eccSquared = 0.00667054;
                break;
            case "Modified Everest":
                a = 6377304;
                eccSquared = 0.006637847;
                break;
            case "Modified Fischer 1960":
                a = 6378155;
                eccSquared = 0.006693422;
                break;
            case "South American 1969":
                a = 6378160;
                eccSquared = 0.006694542;
                break;
            case "WGS 60":
                a = 6378165;
                eccSquared = 0.006693422;
                break;
            case "WGS 66":
                a = 6378145;
                eccSquared = 0.006694542;
                break;
            case "WGS 72":
                a = 6378135;
                eccSquared = 0.006694318;
                break;
            case "ED50":
                a = 6378388;
                eccSquared = 0.00672267;
                break; // International Ellipsoid
            case "WGS 84":
            case "EUREF89": // Max deviation from WGS 84 is 40 cm/km see http://ocq.dk/euref89 (in danish)
            case "ETRS89": // Same as EUREF89 
                a = 6378137;
                eccSquared = 0.00669438;
                break;
            default:
                status = true;
                break;
        }
    }

    public LatLng convertUtmToLatLng(double UTMEasting, double UTMNorthing, int UTMZoneNumber, String UTMZoneLetter)
    {
        var e1 = (1 - Math.Sqrt(1 - this.eccSquared)) / (1 + Math.Sqrt(1 - this.eccSquared));
        var x = UTMEasting - 500000.0; //remove 500,000 meter offset for longitude
        var y = UTMNorthing;
        var ZoneNumber = UTMZoneNumber;
        var ZoneLetter = UTMZoneLetter;
        // int NorthernHemisphere;

        if ("N" == ZoneLetter)
        {
            // NorthernHemisphere = 1;
        }
        else
        {
            // NorthernHemisphere = 0;
            y -= 10000000.0;
        }

        var LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;

        var eccPrimeSquared = (this.eccSquared) / (1 - this.eccSquared);

        double M = y / 0.9996;
        var mu = M / (this.a * (1 - this.eccSquared / 4 - 3 * this.eccSquared * this.eccSquared / 64 - 5 * this.eccSquared * this.eccSquared * this.eccSquared / 256));

        var phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
        var phi1 = this.toDegrees(phi1Rad);

        var N1 = this.a / Math.Sqrt(1 - this.eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
        var T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
        var C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
        var R1 = this.a * (1 - this.eccSquared) / Math.Pow(1 - this.eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
        var D = x / (N1 * 0.9996);

        var Lat = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
                + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
        Lat = this.toDegrees(Lat);

        var Long = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)
                * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
        Long = LongOrigin + this.toDegrees(Long);
        return new LatLng { Lat = Lat, Lng = Long };
    }

    private double toDegrees (double rad) {
        return rad / Math.PI* 180;
    }
}
