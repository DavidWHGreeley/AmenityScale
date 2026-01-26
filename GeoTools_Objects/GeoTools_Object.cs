using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Geo;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

/// <summary>
/// Version         Date            Coder           Remarks
/// 1.0             2020-06-14      Clay            Initial
/// 1.5             2002-06-16      Clay            Good grief Altitude Fix!
/// 2.0             2020-06-20      Clay            Implement Lines and MultiLines. Implement 
///                                                 actual KML Polygons in nice Conversions. Make
///                                                 it easier to make shapes. ABSTRACTION!
///  2.2            2020-07-01      Clay                ToPlacemark Added
/// 2.72            2020-12-08      Alan            Addressed BearingTo Function Bug   
/// 2.80            2021-02-03      Clay            Break the DB out. This will break ALL other previous versions of GeoEngine.
/// 2.81            2021-02-10      Clay            Implemented IDisposable
/// 2.91            2021-11-24      Clay            Implemented Bartlett's Geo Coordinate
/// 3.00            2021-12-26      Clay            Implemented Debug Logging
/// 3.1.0           2022-06-14      Clay            Initial
/// 3.1.5           2022-06-16      Clay            Good grief Altitude Fix!
/// 3.2.0           2022-06-20      Clay            Implement Lines and MultiLines. Implement 
///                                                 actual KML Polygons in nice Conversions. Make
///                                                 it easier to make shapes. ABSTRACTION!
/// 3.2.1           2022-11-07      Victoria        MakeVectorLineList bug crushed.
/// 3.2.2           2022-11-18      Haotian         Crushed String Replace issue with polygons with holes
/// 3.2.3           2023-01-01      Clay            Expanded the Constrcutors for Clamped to Sea Floor Vertical Datums.
/// 3.2.4           2023-01-10      ShakeSpeare!    Added a reverse function for the Line Class. 
/// 3.2.5           2023-01-12      ShakeSpeare!    Implementing Vertical Datum.
/// 3.2.6           2023-01-14      Clay            Binary Operator Overload.
/// 4.0             2023-10-20      ShakeSpeare!    Last version before the big rewrite. Does this even compile????
/// 4.1             2023-11-10      Clay            Fixed the Tesselate Attribute bug.
/// 4.2             2023-12-24      ShakeSpeare!    Added Nullable Double on Coordinate Altitude. And any other changes required to push the chult project
///                                                 forward.
/// 4.3             2024-12-10      Clay            Overloaded Sample Styler
/// 4.4             2024-12-18      ShakeSpeare!    Overload Polygon Constructor that takes a KML placemark. 
/// 4.5             2024-12-22      Clay            Fixed toPlacemark function that was 
///                                                 adding points to polygons at the end in the unfounded fear that someone is building bad polygons.
/// 4.6             2025-01-12      Shakespeare!    The Ilderton Scrape needed a new constructor for polygons.  
/// 4.7             2025-03-27      Clay            The Noob Lab 4 Update.
/// 5.4             2025-10-19      Clay            The build for Fall 2025 class. This semester Waterdeep is completed....Epic.
/// 
/// </summary>
/// 
namespace Geotool_Objects
{

    public enum VerticalDatum
    {
        ClampToSeafloor = 1,
        RelativeToSeafloor = 2,
        ClampToGround = 3,
        RelativeToGround = 4,
        Absolute = 5
    }

    /// <summary>
    /// Builds a coordinate object
    /// </summary>
    public class Coordinate : IComparable
    {
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public Double Altitude { get; set; }


        //Private we need later
        private int ID;

        /// <summary>
        /// Coordinate Object. Altitude is mandatory.
        /// So what if we get nulls? Check out the ?? operator. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
        /// Goodbye null reference exceptions.
        /// </summary>
        public Coordinate(double latitude, double longitude, double? altitude)
        {
            Latitude = latitude;
            Longitude = longitude;            
            Altitude = altitude ?? 0.0D; 
            
            
        }

        public Coordinate(SharpKml.Base.Vector V)
        {
            Latitude = V.Latitude;
            Longitude = V.Longitude;

            //A fundamental rule for the whole to work is that a Coordinate MUST have a value. 
            if (V.Altitude.HasValue == true)
            {
                Altitude = V.Altitude.Value;
            }
            else
            {
                Altitude = 0;
            }

        }

        //public Coordinate(Double latitude, Double longitude)
        //{
        //    Latitude = latitude;
        //    Longitude = longitude;
        //    Altitude = 0;
        //}

        public Coordinate(Double latitude, Double longitude, Double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator ==(Coordinate LHS, Coordinate RHS)
        {
            if (LHS.Latitude == RHS.Latitude &&
               LHS.Longitude == RHS.Longitude)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Coordinate LHS, Coordinate RHS)
        {
            if (LHS.Latitude == RHS.Latitude &&
               LHS.Longitude == RHS.Longitude)
            {
                return false;
            }
            return true;
        }

        public SharpKml.Dom.Placemark ToPlaceMark()
        {
            SharpKml.Dom.Placemark PlaceyThePlacemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.Point PointyThePoint = new SharpKml.Dom.Point();
            PointyThePoint.Coordinate = new SharpKml.Base.Vector(Latitude, Longitude, Altitude);
            PointyThePoint.Id = ID.ToString();

            PlaceyThePlacemark.Geometry = PointyThePoint.Clone();
            return PlaceyThePlacemark.Clone();

        }

        public SharpKml.Dom.Placemark ToPlaceMark(bool extrude, VerticalDatum Mode)
        {

            SharpKml.Dom.Placemark PlaceyThePlacemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.Point PointyThePoint = new SharpKml.Dom.Point();
            PointyThePoint.Coordinate = new SharpKml.Base.Vector(Latitude, Longitude, Altitude);
            switch (Mode)
            {
                case VerticalDatum.Absolute: { PointyThePoint.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
                case VerticalDatum.RelativeToSeafloor: { PointyThePoint.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor; break; }
                case VerticalDatum.RelativeToGround: { PointyThePoint.AltitudeMode = SharpKml.Dom.AltitudeMode.RelativeToGround; break; }
                case VerticalDatum.ClampToSeafloor: { PointyThePoint.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor; break; }
                case VerticalDatum.ClampToGround: { PointyThePoint.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround; break; }
                default: { PointyThePoint.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
            }



            PointyThePoint.Id = ID.ToString();

            PlaceyThePlacemark.Geometry = PointyThePoint.Clone();
            return PlaceyThePlacemark.Clone();

        }

        public String ToSQLGEOGRAPHY()
        {
            String sql = "POINT (";

            sql = sql + $"{this.Longitude} {this.Latitude} {this.Altitude}";


            //glue on the end
            sql = sql + ")";
            return sql;

        }

        /// <summary>
        /// First kick at using IComparable
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            Coordinate other = (Coordinate)obj;
            if (this.Longitude > other.Longitude) { return 1; }
            else if (this.Longitude < other.Longitude) { return -1; }
            else { return 0; }
        }


    }

    /// <summary>
    /// Builds a basic Vector object
    /// </summary>
    public class Vector
    {
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }

        public Vector()
        {
            Latitude = 0;
            Longitude = 0;
        }

        public Vector(Double latitude, Double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;

        }

        public Vector(Double latitude, Double longitude, Double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>
    /// Problem in Lines and probably in other Shapes about what altitude mode to use. WE NEED TO FIX ASAP.
    /// </summary>
    public class Line
    {
        public List<Coordinate> CoordinateList { get; set; }
        private int ID;

        public Line()
        {
            CoordinateList = new List<Coordinate>();
        }

        public SharpKml.Dom.Placemark ToPlacemark(bool extrude, VerticalDatum mode)
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.LineString line = new SharpKml.Dom.LineString();
            line.Coordinates = new CoordinateCollection();
            List<Coordinate> GeoLine = new List<Coordinate>();
            line.Extrude = extrude;
            line.Tessellate = true;
            switch (mode)
            {
                case VerticalDatum.Absolute: { line.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
                case VerticalDatum.RelativeToSeafloor: { line.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor; break; }
                case VerticalDatum.RelativeToGround: { line.AltitudeMode = SharpKml.Dom.AltitudeMode.RelativeToGround; break; }
                case VerticalDatum.ClampToSeafloor: { line.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor; break; }
                case VerticalDatum.ClampToGround: { line.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround; break; }
                default: { line.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
            }

            line.Id = ID.ToString();

            {
                foreach (Coordinate C in CoordinateList)
                {

                    line.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
                }
            }

            placemark.Geometry = line.Clone();

            return placemark.Clone();
        }



        //This is a classic wrapper for another call. 
        public void Reverse()
        {
            CoordinateList.Reverse();
        }

        public SharpKml.Dom.Placemark ToPlacemark()
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.LineString line = new SharpKml.Dom.LineString();
            line.Coordinates = new CoordinateCollection();
            List<Coordinate> GeoLine = new List<Coordinate>();
            line.Id = ID.ToString();

            foreach (Coordinate C in CoordinateList)
            {
                line.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
            }
            placemark.Geometry = line.Clone();

            return placemark.Clone();
        }

        public String ToSQLGEOGRAPHY()
        {
            String sql = "LINESTRING (";
            foreach (Coordinate C in CoordinateList)
            {
                sql = sql + $"{C.Longitude} {C.Latitude} {C.Altitude},";
            }
            //Now we remove the last comma.
            sql = sql.Remove(sql.Length - 1, 1);
            //and glue on the end
            sql = sql + ")";
            return sql;

        }
    }

    /// <summary>
    /// Builds a basic Multi Line. No to Placemark. Add this to Burn Down.
    /// </summary>
    public class MultiLine
    {
        public List<List<Coordinate>> LineList { get; set; }
    }

    /// <summary>
    /// Builds a MultiPolygon. No Holes.
    /// </summary>
    public class MultiPolygon
    {
        //A multipolygon just is a list of polygons...
        public List<Polygon> PolyGonList;
        //Private we need later
        private int ID;

        public MultiPolygon()
        {
            PolyGonList = new List<Polygon>();
        }

        //I overloaded the placemark function to allow me to glue the elevation data on.
        public SharpKml.Dom.Placemark ToPlacemark(bool extrude, VerticalDatum Mode)
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();

            SharpKml.Dom.MultipleGeometry multipleGeometry = new SharpKml.Dom.MultipleGeometry();
           

            foreach (Polygon polygon in PolyGonList)
            {
                //I just pass the issue to the Polygon to sort out mode and Extrude issues. 
                multipleGeometry.AddGeometry(polygon.ToKMLPolygon(extrude, Mode).Clone());
            }
            placemark.Geometry = multipleGeometry;

            return placemark.Clone();
        }
        //I overloaded the placemark function to allow me to glue the elevation data on.


        public SharpKml.Dom.Placemark ToPlacemark()
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();

            SharpKml.Dom.MultipleGeometry multipleGeometry = new SharpKml.Dom.MultipleGeometry();


            foreach (Polygon polygon in PolyGonList)
            {
                multipleGeometry.AddGeometry(polygon.ToKMLPolygon().Clone());
            }
            placemark.Geometry = multipleGeometry;

            return placemark.Clone();
        }






    }

    /// <summary>
    /// A basic GeoTools Polygon. Handles holes.
    /// </summary>
    public class Polygon
    {
        //This is the ouside of your shape.
        public List<Coordinate> LinearList;

        //This is your list of holes.
        public List<List<Coordinate>> InnerBoundaryList;


        //Private we need later
        private int ID;

        /// <summary>
        /// Date            Version     Name            Remarks
        /// 2024-01-12      ALPHA       Shakespeare!    A quick Hack to extract that wack 
        ///                                             esri crack. Yo Middlesex Hack
        ///                                             We have to add another library to deserialize the JSON.
        ///                                             WARNING ONLY WORKS POLYGONS WITHOUT HOLES.
        /// </summary>
        /// <param name="EsriJSON"></param>
        public Polygon(String EsriJSON)
        {
            ID = 0;
            LinearList = new List<Coordinate>();
            InnerBoundaryList = new List<List<Coordinate>>();


            //Well let's rip this JSON apart. We have yet another company
            //deciding that WKT is not a supported STANDARD. Obfuscation...Esri is
            //the Apple of GIS. WKT is a standard.
            //
            //Hard coded like a chad! I just need it to work. I will mop
            //up in the next cycle. I am the Hattori Hanzo of GIS. 

            String Tessa = EsriJSON.Split(new String[] { "[[[", "]]]" }, StringSplitOptions.None)[1];

            foreach (String S in Tessa.Split(new String[] { "],[" }, StringSplitOptions.None))
            {
                String[] TempCoord = S.Split(',');
                LinearList.Add(new Coordinate(Double.Parse(TempCoord[1]), Double.Parse(TempCoord[0]), 0));
            }

        }
        public Polygon()
        {
            ID = 0;
            LinearList = new List<Coordinate>();
            InnerBoundaryList = new List<List<Coordinate>>();
        }

        public Polygon(SharpKml.Dom.Placemark Placey)
        {
            ID = 0;
            LinearList = new List<Coordinate>();
            InnerBoundaryList = new List<List<Coordinate>>();
            if (Placey != null)
            {
                if (Placey.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                {


                    SharpKml.Dom.Polygon Po = (SharpKml.Dom.Polygon)Placey.Geometry;
                    foreach (SharpKml.Base.Vector C in Po.OuterBoundary.LinearRing.Coordinates)
                    {
                        this.LinearList.Add(new Coordinate(C.Latitude, C.Longitude, C.Altitude));
                    }
                    foreach (SharpKml.Dom.InnerBoundary IB in Po.InnerBoundary)
                    {
                        List<Coordinate> list = new List<Coordinate>();
                        foreach (SharpKml.Base.Vector C in IB.LinearRing.Coordinates)
                        {
                            list.Add(new Coordinate(C.Latitude, C.Longitude, C.Altitude));
                        }
                        this.InnerBoundaryList.Add(list);
                    }
                }
            }
            
       
           
        }

        public Polygon(List<Coordinate> Shape)
        {
            LinearList = new List<Coordinate>();
            LinearList.AddRange(Shape);
            InnerBoundaryList = new List<List<Coordinate>>();
        }

        public SharpKml.Dom.Placemark ToPlaceMark(bool extrude, VerticalDatum mode)
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.Polygon polygon = new SharpKml.Dom.Polygon();
            polygon.OuterBoundary = new SharpKml.Dom.OuterBoundary();
            polygon.OuterBoundary.LinearRing = new SharpKml.Dom.LinearRing();
            polygon.OuterBoundary.LinearRing.Tessellate = true;
            polygon.OuterBoundary.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();
            polygon.Id = ID.ToString();
            polygon.Extrude = extrude;
            switch (mode)
            {
                case VerticalDatum.Absolute: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
                case VerticalDatum.RelativeToSeafloor: { polygon.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor; break; }
                case VerticalDatum.RelativeToGround: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.RelativeToGround; break; }
                case VerticalDatum.ClampToSeafloor: { polygon.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor; break; }
                case VerticalDatum.ClampToGround: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround; break; }
                default: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
            }
            foreach (Coordinate C in LinearList)
            {
                polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
            }
            //We need to put the last coordinate on the end to close 
            //the polygon.
            //polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(LinearList[0].Latitude, LinearList[0].Longitude, LinearList[0].Altitude));

            //now we add all the possible holes
            foreach (List<Coordinate> C in InnerBoundaryList)
            {
                SharpKml.Dom.InnerBoundary temp = new SharpKml.Dom.InnerBoundary();
                temp.LinearRing = new SharpKml.Dom.LinearRing();
                temp.LinearRing.Tessellate = true;
                temp.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();

                foreach (Coordinate S in C)
                {
                    temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(S.Latitude, S.Longitude, S.Altitude));
                }
               // temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C[0].Latitude, C[0].Longitude, C[0].Altitude));
                polygon.AddInnerBoundary(temp.Clone());
            }

            placemark.Geometry = polygon;
            return placemark.Clone();
        }


        /// <summary>
        /// This function will make building placemark polygons easier.
        /// </summary>
        /// <returns></returns>
        public SharpKml.Dom.Placemark ToPlaceMark()
        {
            SharpKml.Dom.Placemark placemark = new SharpKml.Dom.Placemark();
            SharpKml.Dom.Polygon polygon = new SharpKml.Dom.Polygon();
            polygon.OuterBoundary = new SharpKml.Dom.OuterBoundary();
            polygon.OuterBoundary.LinearRing = new SharpKml.Dom.LinearRing();
            polygon.OuterBoundary.LinearRing.Tessellate = true;
            polygon.OuterBoundary.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();
            polygon.Id = ID.ToString();
            foreach (Coordinate C in LinearList)
            {
                polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
            }
            //We need to put the last coordinate on the end to close 
            //the polygon.
            //if(LinearList.Count > 0)
            //{ 
            //polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(LinearList[0].Latitude, LinearList[0].Longitude, LinearList[0].Altitude));
            //}
            //now we add all the possible holes
            foreach (List<Coordinate> C in InnerBoundaryList)
            {
                SharpKml.Dom.InnerBoundary temp = new SharpKml.Dom.InnerBoundary();
                temp.LinearRing = new SharpKml.Dom.LinearRing();
                temp.LinearRing.Tessellate = true;
                temp.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();

                foreach (Coordinate S in C)
                {
                    temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(S.Latitude, S.Longitude, S.Altitude));
                }
             //   temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C[0].Latitude, C[0].Longitude, C[0].Altitude));
                polygon.AddInnerBoundary(temp.Clone());
            }

            placemark.Geometry = polygon;
            return placemark.Clone();


        }

        /// <summary>
        /// This is the overload that takes in the Vertical Daturm
        /// </summary>
        /// <param name="extrude"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public SharpKml.Dom.Polygon ToKMLPolygon(bool extrude, VerticalDatum mode)
        {

            SharpKml.Dom.Polygon polygon = new SharpKml.Dom.Polygon();
            polygon.OuterBoundary = new SharpKml.Dom.OuterBoundary();
            polygon.OuterBoundary.LinearRing = new SharpKml.Dom.LinearRing();
            polygon.OuterBoundary.LinearRing.Tessellate = true;
            polygon.OuterBoundary.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();
            polygon.Id = ID.ToString();
            polygon.Extrude = extrude;

            switch (mode)
            {
                case VerticalDatum.Absolute: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
                case VerticalDatum.RelativeToSeafloor: { polygon.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor; break; }
                case VerticalDatum.RelativeToGround: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.RelativeToGround; break; }
                case VerticalDatum.ClampToSeafloor: { polygon.GXAltitudeMode = SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor; break; }
                case VerticalDatum.ClampToGround: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround; break; }
                default: { polygon.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute; break; }
            }
            if (LinearList.Count > 0)
            {
                foreach (Coordinate C in LinearList)
                {
                    polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
                }
                //We need to put the last coordinate on the end to close 
                //the polygon.

              //  polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(LinearList[0].Latitude, LinearList[0].Longitude, LinearList[0].Altitude));
            }
            //now we add all the possible holes
            foreach (List<Coordinate> C in InnerBoundaryList)
            {
                SharpKml.Dom.InnerBoundary temp = new SharpKml.Dom.InnerBoundary();
                temp.LinearRing = new SharpKml.Dom.LinearRing();
                temp.LinearRing.Tessellate = true;
                temp.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();

                if(C.Count > 0)
                {
                foreach (Coordinate S in C)
                {
                    temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(S.Latitude, S.Longitude, S.Altitude));
                }
               
              //  temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C[0].Latitude, C[0].Longitude, C[0].Altitude));
                }

                polygon.AddInnerBoundary(temp.Clone());
            }


            return polygon.Clone();


        }

        public String ToSQLGEOGRAPHY()
        {
            String sql = "POLYGON ((";

            foreach (Coordinate C in LinearList)
            {
                sql = sql + $"{C.Longitude} {C.Latitude} {C.Altitude},";
            }
            //Now we remove the last comma.
            sql = sql.Remove(sql.Length - 1, 1);
            //and glue on the end
            sql = sql + ")";

            //Now we have the outside of the polygon. Now we have to add all the inner polygons. "The Holes"

            //Lets bail if we do not have any inner boundaries
            if (InnerBoundaryList.Count == 0)
            {
                //I glue the last bracket on. lol. So the Polygon ends with ))
                sql = sql + ")";
                return sql;
            }
            //But if we have holes we have to add them to the Polygon.
            else
            {
                foreach (List<Coordinate> CList in InnerBoundaryList)
                {
                    String Innersql = ",(";

                    foreach (Coordinate C in CList)
                    {
                        Innersql = Innersql + $"{C.Longitude} {C.Latitude} {C.Altitude},";
                    }

                    //Now we remove the last comma.
                    Innersql = Innersql.Remove(Innersql.Length - 1, 1);
                    //and glue on the end
                    Innersql = Innersql + ")";
                    sql = sql + Innersql;

                }

                //I glue the last bracket on. lol. So the Polygon ends with ))
                sql = sql + ")";
                return sql;
            }

        }

        /// <summary>
        /// This function will make building placemark polygons easier.
        /// </summary>
        /// <returns></returns>
        public SharpKml.Dom.Polygon ToKMLPolygon()
        {

            SharpKml.Dom.Polygon polygon = new SharpKml.Dom.Polygon();
            polygon.OuterBoundary = new SharpKml.Dom.OuterBoundary();
            polygon.OuterBoundary.LinearRing = new SharpKml.Dom.LinearRing();
            polygon.OuterBoundary.LinearRing.Tessellate = true;
            polygon.OuterBoundary.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();
            polygon.Id = ID.ToString();
            foreach (Coordinate C in LinearList)
            {
                polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C.Latitude, C.Longitude, C.Altitude));
            }
            //We need to put the last coordinate on the end to close 
            //the polygon.
         //   polygon.OuterBoundary.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(LinearList[0].Latitude, LinearList[0].Longitude, LinearList[0].Altitude));

            //now we add all the possible holes
            foreach (List<Coordinate> C in InnerBoundaryList)
            {
                SharpKml.Dom.InnerBoundary temp = new SharpKml.Dom.InnerBoundary();
                temp.LinearRing = new SharpKml.Dom.LinearRing();
                temp.LinearRing.Tessellate = true;
                temp.LinearRing.Coordinates = new SharpKml.Dom.CoordinateCollection();

                foreach (Coordinate S in C)
                {
                    temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(S.Latitude, S.Longitude, S.Altitude));
                }
            //    temp.LinearRing.Coordinates.Add(new SharpKml.Base.Vector(C[0].Latitude, C[0].Longitude, C[0].Altitude));
                polygon.AddInnerBoundary(temp.Clone());
            }


            return polygon.Clone();


        }

    }

    /// <summary>
    /// Vers   Coder        Date        Comments
    /// 1.0     Clay        2022-06-25  Set Owner property public...reasons lol
    ///                                 Opened up the Owner property.
    /// </summary>
    public class GeoTools
    {

        /// <summary>
        /// Marked private to keep it invisible to everything outside the GeoTools.
        /// Yes you can encapsulate a class inside a class.
        /// </summary>
        private struct SimpleTriangle
        {
            public int a, b, c;
            public Coordinate circumcentre;
            public double radius;

            public SimpleTriangle(int a, int b, int c, Coordinate circumcentre, double radius)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.radius = radius;
                this.circumcentre = circumcentre;
            }
        }

        public String Owner;
        private Random RandoTron;
        /// <summary>
        /// A constructor with no paramaters is a default constructor.
        /// We make it private to prevent you from making it.
        /// </summary>

        private GeoTools()
        {

        }

        public GeoTools(String owner)
        {
            Owner = owner;
            RandoTron = new Random();
            Geo.Coordinate Temp = new Geo.Coordinate(0, 0);
        }

        /// <summary>
        /// Shameless stolen from Stack Overflow. 
        /// https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public bool IsPointInPolygon4(List<Coordinate> Coords, Coordinate testPoint)
        {
            Coordinate[] polygon = Coords.ToArray();
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Latitude < testPoint.Latitude && polygon[j].Latitude >= testPoint.Latitude ||
                    polygon[j].Latitude < testPoint.Latitude && polygon[i].Latitude >= testPoint.Latitude)  
                {
                    if (polygon[i].Longitude + (testPoint.Latitude - polygon[i].Latitude) /
                       (polygon[j].Latitude - polygon[i].Latitude) *
                       (polygon[j].Longitude - polygon[i].Longitude) < testPoint.Longitude)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        private void CalculateCircumcircle(Coordinate p1, Coordinate p2, Coordinate p3, out Coordinate circumCentre, out double radius)
        {
            // Calculate the length of each side of the triangle
            double a = Distance(p2, p3); // side a is opposite point 1
            double b = Distance(p1, p3); // side b is opposite point 2
            double c = Distance(p1, p2); // side c is opposite point 3
                                         // Calculate the radius of the circumcircle
            double area = Math.Abs((double)(p1.Longitude * (p2.Latitude - p3.Latitude) + p2.Longitude * (p3.Latitude - p1.Latitude) + p3.Longitude
            * (p1.Latitude - p2.Latitude)) / 2);
            radius = a * b * c / (4 * area);
            // Define area coordinates to calculate the circumcentre
            double pp1 = Math.Pow(a, 2) * (Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2));
            double pp2 = Math.Pow(b, 2) * (Math.Pow(c, 2) + Math.Pow(a, 2) - Math.Pow(b, 2));
            double pp3 = Math.Pow(c, 2) * (Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2));
            // Normalise
            double t1 = pp1 / (pp1 + pp2 + pp3);
            double t2 = pp2 / (pp1 + pp2 + pp3);
            double t3 = pp3 / (pp1 + pp2 + pp3);
            // Convert to Cartesian
            double x = t1 * p1.Longitude + t2 * p2.Longitude + t3 * p3.Longitude;
            double y = t1 * p1.Latitude + t2 * p2.Latitude + t3 * p3.Latitude;
            circumCentre = new Coordinate(y, x, 0);

        }

        public List<Coordinate> BuildClimbingCircle(Coordinate Origin)
        {

            //Making a geodesic circle 1 km in radius centered over an origin.
            List<Coordinate> GeodesicCircle = MakeGeodesicCircleList(1, Origin);

            //I am going to add some "Styling to each of my points. An Icon and a Colour
            //Note how I can use the power of parameterization. I add a Elevation Component.
            for (int i = 0; i < GeodesicCircle.Count; ++i)
            {
                GeodesicCircle[i].Altitude = 1 + (i * 100);


            }
            return GeodesicCircle;

        }

        public List<Coordinate> RainbowTwistyHexagon(Coordinate Origin, double Theta, double UV, int Iterations)
        {
            List<Coordinate> List = new List<Coordinate>();



            for (int j = 0; j < Iterations; j++)
            {
                List<Coordinate> Hex = MakeGeodesicHexagon(Origin, UV * j);
                Hex = GeodesicRotation(Hex, Origin, Theta * j);
                List.AddRange(Hex);
            }

            return List;
        }

        public List<Coordinate> MakeGeodesicHexagon(Coordinate Origin, double UV)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (double i = 0; i < 360; i += 60)
            {
                List.Add(DestinationCoordinate(Origin, UV, i));
            }

            return List;
        }

        public List<Coordinate> MakeGeodesicOctogon(Coordinate Origin, double UV)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (double i = 0; i < 360; i += 45)
            {
                List.Add(DestinationCoordinate(Origin, UV, i));
            }

            return List;
        }

        public List<Coordinate> MakeGeodesicStarBurst(Coordinate Origin, double UV)
        {
            List<Coordinate> List = new List<Coordinate>();
            bool s = true;

            for (double i = 0; i < 360; i += 22.5)
            {
                if(s)
                {
                List.Add(DestinationCoordinate(Origin, UV, i));
                    s = !s;
                }
                else
                {
                    List.Add(DestinationCoordinate(Origin, UV/2, i));
                    s = !s;
                }
                
            }

            return List;
        }

        public List<Coordinate> MakeGeodesicRandomSplotch(Coordinate Origin, double UV)
        {
            List<Coordinate> List = new List<Coordinate>();
           
           

            for (double i = 0; i < 360; i += this.RandoTron.NextDouble()*20)
            {
              
                    List.Add(DestinationCoordinate(Origin, this.RandoTron.NextDouble()*UV+(UV/2), i));
            

            }

            return List;
        }



        /// <summary>
        /// You give it a list of valid polygon placemarks.
        /// It gives you a MultiGeometry Placemark. Zap! All that hard work is abstracted. 
        /// </summary>
        /// <param name="LP"></param>
        /// <returns></returns>
        public Placemark CombineIntoMultiGeometry(List<Placemark> LP)
        {
            Placemark returnPlacemark = new Placemark();

            SharpKml.Dom.MultipleGeometry multipleGeometry = new SharpKml.Dom.MultipleGeometry();
            foreach (Placemark P in LP)
            {
                multipleGeometry.AddGeometry(P.Geometry.Clone());
            }
            returnPlacemark.Name = LP[0].Name;
            returnPlacemark.Geometry = multipleGeometry;
            returnPlacemark.Description = new Description();
            returnPlacemark.AddStyle(LP[0].Styles.FirstOrDefault().Clone());
            //returnPlacemark.Description = LP[0].Description.Clone();
            return returnPlacemark;

        }

        /// <summary>
        /// Makes an arc upon a geodesic sphere.
        /// Date            Coder   Notes
        /// </summary>
        /// <param name="Radius">In Kms</param>
        /// <param name="Origin">The Origin of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakeGeodesicArcList(Double Radius, Coordinate Origin, Double Begin, Double End)
        {
            List<Coordinate> List = new List<Coordinate>();
            for (double angle = Begin; angle <= End; ++angle)
            {

                Coordinate C = DestinationCoordinate(Origin, Radius, angle);
                List.Add(C);

            }

            return List;
        }

        /// <summary>
        /// Builds a Planar circle and raises and lowers the elevation of the pins in a Sine Wave
        /// </summary>
        /// <param name="Origin"></param>
        /// <returns></returns>
        public List<Coordinate> BuildSinusoidalCircle(Coordinate Origin)
        {
            //Making a planar circle 1 km in radius centered over an origin.
            List<Coordinate> PlanarCircle = MakePlanarCircleList(1, Origin);

            //I am going to add some "Styling to each of my points. An Icon and a Colour
            //Note how I can use the power of parameterization. I add a Elevation Component.
            //The return of Randotron! 
            Random RandoTron = new Random();
            for (int i = 0; i < PlanarCircle.Count; ++i)
            {
                PlanarCircle[i].Altitude = 500 + (100 * Math.Sin(i));
            }

            return PlanarCircle;

        }

        public List<Coordinate> BuildGeodesicSquarePyramid(Coordinate Origin)
        {
            List<Coordinate> BaseList = new List<Coordinate>();

            //Two variables used to control height and size and colour.
            for (int i = 8, k = 0; i > 0; --i, ++k)
            {
                //Making a geodesic square i*0.2 km on the side length centered over an origin.
                List<Coordinate> GeodesicSquare = MakeGeodesicSquareList(i * 0.2, Origin);
                for (int j = 0; j < GeodesicSquare.Count; ++j)
                {
                    GeodesicSquare[j].Altitude = (k * 100) + Origin.Altitude;
                }
                BaseList.AddRange(GeodesicSquare);

            }

            return BaseList;

        }

        public List<Coordinate> BuildGrid(Coordinate Origin)
        {
            List<Coordinate> BaseList = new List<Coordinate>();

            Coordinate Destination = DestinationCoordinate(Origin, 1, 90);

            List<Coordinate> BaseLine = MakePlanarLineList(Origin, Destination, 10);

            for (int i = 0; i < BaseLine.Count; ++i)
            {
                Coordinate Temp = DestinationCoordinate(BaseLine[i], 1, 180);
                List<Coordinate> Row = MakePlanarLineList(BaseLine[i], Temp, 10);
                BaseList.AddRange(Row);
            }
            for (int i = 0; i < BaseList.Count; ++i)
            {

                BaseList[i].Altitude = Origin.Altitude;

            }

            return BaseList;
        }

        public List<Coordinate> SineWaveLatitude(Coordinate Origin)
        {
            Coordinate Destination = DestinationCoordinate(Origin, 100, 90);
            List<Coordinate> Line = MakePlanarLineList(Origin, Destination, 100);

            for (int i = 0; i < Line.Count; ++i)
            {

                Line[i].Altitude = Origin.Altitude;

            }
            return Line;
        }

        public List<Coordinate> SquareWaveLatitude(Coordinate Origin)
        {
            Coordinate Destination = DestinationCoordinate(Origin, 100, 90);
            List<Coordinate> Line = MakePlanarLineList(Origin, Destination, 100);

            for (int i = 0; i < Line.Count; ++i)
            {

                Line[i].Altitude = Origin.Altitude;

            }
            return Line;
        }

        /// <summary>
        /// Makes a rhumb line shape
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="Bearing"></param>
        /// <param name="Distance"></param>
        /// <param name="Iterations"></param>
        /// <returns></returns>
        public List<Coordinate> MakeRhumbList(Coordinate Origin, Double Bearing, Double Distance, int Iterations)
        {
            List<Coordinate> List = new List<Coordinate>();
            Coordinate Destination = new Coordinate(0, 0, 0);
            List.Add(Origin);

            for (int i = 0; i < Iterations && Destination.Latitude >= -90 && Destination.Latitude <= 90; ++i)
            {
                //Figure out what the new step will look like
                Destination = DestinationCoordinate(Origin, Distance, Bearing);
                //Add it to the list.
                List.Add(Destination);
                //Set it as the new Origin for the next loop.
                Origin = Destination;
            }
            return List;
        }

        /// <summary>
        /// Note the Latitude Nonsense. You want to make a line that goes over the pole...Fine....
        /// But it is not a bearing!
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="V"></param>
        /// <param name="Iterations"></param>
        /// <returns></returns>
        public List<Coordinate> MakeVectorLineList(Coordinate Origin, Vector V, int Iterations)
        {
            List<Coordinate> List = new List<Coordinate>();
            List.Add(Origin);

            for (int i = 0; i < Iterations; ++i)
            {
                //Latitude Formula
                #region NORTHSOUTH

                double CircularAngle = (Origin.Latitude + V.Latitude) % 360;


                if ((CircularAngle >= 0 && CircularAngle <= 90) || (CircularAngle <= 0 && CircularAngle >= -90))
                {
                    Origin.Latitude = CircularAngle;
                }
                else if (CircularAngle >= 90 && CircularAngle < 180)
                {
                    Origin.Latitude = 90 - Math.Abs(CircularAngle % 90);
                }
                else if (CircularAngle <= -90 && CircularAngle > -180)
                {
                    Origin.Latitude = -90 + Math.Abs(CircularAngle % 90);
                }
                else if (CircularAngle >= 180 && CircularAngle < 270)
                {
                    Origin.Latitude = 0 - Math.Abs(CircularAngle % 90);
                }
                else if (CircularAngle <= -180 && CircularAngle > -270)
                {
                    Origin.Latitude = 0 + Math.Abs(CircularAngle % 90);
                }
                else if (CircularAngle >= 270)
                {
                    Origin.Latitude = -90 + Math.Abs(CircularAngle % 90);
                }
                else if (CircularAngle <= -270)
                {
                    Origin.Latitude = 90 - Math.Abs(CircularAngle % 90);
                }
                #endregion

                //Longitude Formula
                #region WESTEAST
                CircularAngle = (Origin.Longitude + (V.Longitude % 360));
                if (CircularAngle > 180)
                {
                    Origin.Longitude = -180 + (CircularAngle % 180);
                }
                else if (CircularAngle < -180)
                {
                    Origin.Longitude = 180 + (CircularAngle % 180);
                }

                else
                {
                    Origin.Longitude = CircularAngle;
                }


                #endregion

                List.Add(Origin);
            }
            return List;
        }

        /// <summary>
        /// THE MARK OF THE GUILD o-o-o Look Inside.
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="UV"></param>
        /// <returns></returns>
        public List<Coordinate> MarkOfTheGuild(Coordinate Origin, double UV)
        {
            List<Coordinate> List = new List<Coordinate>();

            Coordinate LeftFocus = DestinationCoordinate(Origin, UV, 270);
            Coordinate RightFocus = DestinationCoordinate(Origin, UV, 90);

            List<Coordinate> CenterCircle = MakeGeodesicCircleList(UV / 3, Origin);

            List<Coordinate> LeftCircle = MakeGeodesicCircleList(UV / 3, LeftFocus);

            List<Coordinate> RightCircle = MakeGeodesicCircleList(UV / 3, RightFocus);

            List<Coordinate> GuildLine = MakePlanarLineList(LeftFocus, RightFocus, 100);

            List.AddRange(GuildLine);
            List.AddRange(LeftCircle);
            List.AddRange(RightCircle);
            List.AddRange(CenterCircle);
            return List;
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        private double deg2rad(double deg)
        {
            const double degToRadFactor = Math.PI / 180;
            return deg * degToRadFactor;

            //  return (deg * Math.PI / 180.0);
        }

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        private double rad2deg(double rad)
        {
            const double radToDegFactor = 180 / Math.PI;
            return rad * radToDegFactor;

            //     return (rad / Math.PI * 180.0);
        }

        /// <summary>
        /// Calculates the bearing between two location objects
        /// Vers    Date        Coder       Comments
        /// 1.0     2015-01-01  Clay        Initial
        /// 1.5     2019-10-31  Clay        New Code.
        /// 2.0     2022-05-30  Clay        Coupled to new Framework.
        ///                                 Totally new code.
        /// 
        /// Bearing could be magnetic, grid, stellar (North Pole) Whatever your "North Points".
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <param name="lat1"></param>
        /// <returns></returns>
        public double BearingBetween(Coordinate position1, Coordinate position2)
        {
            double lat1 = deg2rad(position1.Latitude);
            double lat2 = deg2rad(position2.Latitude);
            double long1 = deg2rad(position2.Longitude);
            double long2 = deg2rad(position1.Longitude);
            double dLon = long1 - long2;

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x);

            return (rad2deg(brng) + 360) % 360;
        }

        /// <summary>
        /// Converts a radians angle into a valid compass bearing in degrees. Any extra is removed using modulus.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        private double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (rad2deg(radians) + 360) % 360;
        }

        /// <summary>
        /// Provide a Origin Coordinate, a distance in Kilometers and a Decimal Grid Bearing 
        /// 
        /// </summary>
        /// <param name="Origin">The Origin point.</param>
        /// <param name="dist">Distance in Meters</param>
        /// <param name="brng">Grid Bearing in Decimal Degrees.</param>
        /// <returns></returns>
        public Coordinate DestinationCoordinate(Coordinate Origin, double dist, double brng)
        {

            const double radiusEarthKilometres = 6378137.0;
            double distRatio = Convert.ToDouble(dist) / radiusEarthKilometres;
            double distRatioSine = Math.Sin(distRatio);
            double distRatioCosine = Math.Cos(distRatio);

            double startLatRad = deg2rad(Origin.Latitude);
            double startLonRad = deg2rad(Origin.Longitude);

            double startLatCos = Math.Cos(startLatRad);
            double startLatSin = Math.Sin(startLatRad);

            double endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(deg2rad(brng))));

            double endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(deg2rad(brng)) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));


            Double Latitude = rad2deg(endLatRads);
            Double Longitude = rad2deg(endLonRads);


            //If you want to keep all the wrapping then you would comment this out.
            //If you want to keep all the wrapping then you would comment this out.
            Longitude = ((Longitude) % 360 + 540) % 360 - 180;

            if (Latitude > 90)
            {
                Latitude = 90;
            }
            if (Latitude < -90)
            {
                Latitude = -90;
            }

            return new Coordinate(Latitude, Longitude, Origin.Altitude);

        }

        /// <summary>
        /// Creates a line of points between two Coordinates
        /// </summary>
        /// <param name="Origin">The Origin of the Line</param>
        /// <param name="Destination">The Destination of the Line</param>
        /// <param name="Iterations">The number of points desired between the the Origin and Destination</param>
        /// <returns></returns>
        public List<Coordinate> MakePlanarLineList(Coordinate Origin, Coordinate Destination, int Iterations)
        {
            List<Coordinate> List = new List<Coordinate>();

            //Build the vector we need to make a line
            Vector V = new Vector(Destination.Latitude - Origin.Latitude, Destination.Longitude - Origin.Longitude);

            //Take the Vector and divide it into a smaller vector to use in a loop.
            Vector SmallVector = new Vector(V.Latitude / Iterations, V.Longitude / Iterations);

            for (int i = 0; i <= Iterations; ++i)
            {
                Coordinate C = new Coordinate(0, 0, 0);

                C.Latitude = Origin.Latitude + SmallVector.Latitude * i;
                C.Longitude = Origin.Longitude + SmallVector.Longitude * i;
                C.Altitude = Origin.Altitude;
                List.Add(C);
            }

            return List;
        }

        /// <summary>
        /// Calculates the distance in km between two Coordinates as a great circle
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public double Distance(Coordinate loc1, Coordinate loc2)
        {


            double EarthRadiusInKilometers = 6378137.0;


            //Dont worry. Dont worry.Never will I test this!
            //This function is highly/tightly "coupled".
            double dLat = deg2rad(loc2.Latitude) - deg2rad(loc1.Latitude);
            double dLon = deg2rad(loc2.Longitude) - deg2rad(loc1.Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(deg2rad(loc1.Latitude)) * Math.Cos(deg2rad(loc2.Latitude)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * (Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
            double distance = c * EarthRadiusInKilometers;

            return distance;
        }

        public double GetLength(List<Coordinate> list)
        {
            double temp = 0;
            for(int i = 0; i< list.Count;++i)
            {
                if(i != list.Count-1)
                {
                temp = temp + Distance(list[i], list[i + 1]);
                }
                
            }
            return temp;
        }

        public double GetMaxDistance(Coordinate Origin, List<Coordinate> coords)
        {
            double distance = 0;
            foreach(Coordinate Coordinate in coords)
            {
                if(Distance(Origin, Coordinate) > distance)
                {
                    distance = Distance(Origin, Coordinate);
                }
            }
            return distance;
        }

        public double GetMinDistance(Coordinate Origin, List<Coordinate> coords)
        {
            double distance = Distance(Origin, coords.First());
            foreach (Coordinate Coordinate in coords)
            {
                if (Distance(Origin, Coordinate) < distance)
                {
                    distance = Distance(Origin, Coordinate);
                }
            }
            return distance;
        }

        public Coordinate GetAverage(List<Coordinate> list)
        {
            double Lat = 0;
            double Long = 0;
            double Altitude = 0;

            foreach(Coordinate coord in list)
            {
                Lat = Lat + coord.Latitude;
                Long = Long + coord.Longitude;
                Altitude = Altitude + coord.Altitude;
            }
            Lat = Lat / (double)list.Count;
            Long = Long / (double)list.Count;
            Altitude = Altitude / (double)list.Count;
            return new Coordinate(Lat,Long,Altitude);
        }

        /// <summary>
        /// This is the best translation of all.......this is the PUZZLE. Ask me for help. :)
        /// 
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Origin"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public List<Coordinate> ReferenceTranslation(List<Coordinate> TheShape, Coordinate Origin, Coordinate Target)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Makes a square based in a geodesic geometry.
        /// 2022-06-20      Clay    Right Hand Rule Compliant.
        /// </summary>
        /// <param name="SideLength"></param>
        /// <param name="Origin"></param>
        /// <returns></returns>
        public List<Coordinate> MakeGeodesicSquareList(Double SideLength, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();
            //Solve the Hypoteneuse
            Double C = Math.Sqrt(Math.Pow(SideLength / 2, 2) + Math.Pow(SideLength / 2, 2));
            List.Add(DestinationCoordinate(Origin, SideLength / 2, 0));
            List.Add(DestinationCoordinate(Origin, C, 45));
            List.Add(DestinationCoordinate(Origin, SideLength / 2, 90));
            List.Add(DestinationCoordinate(Origin, C, 135));
            List.Add(DestinationCoordinate(Origin, SideLength / 2, 180));
            List.Add(DestinationCoordinate(Origin, C, 225));
            List.Add(DestinationCoordinate(Origin, SideLength / 2, 270));
            List.Add(DestinationCoordinate(Origin, C, 315));
            List.Add(DestinationCoordinate(Origin, SideLength / 2, 0));
            List.Reverse();
            return List;
        }

        /// <summary>
        /// Creates a circle
        /// 2022-06-20      Clay    Right Hand Rule Compliant.
        /// </summary>
        /// <param name="Radius">The radius of the circle in degrees</param>
        /// <param name="Origin">The Coordinate that is the basis of the construction of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakePlanarCircleList(Double Radius, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();
            for (double Angle = 360; Angle >= 0; --Angle)
            {

                Coordinate C = new Coordinate(0, 0, 0);
                C.Longitude = Origin.Longitude + (Radius * Math.Cos(deg2rad(Angle)));
                C.Latitude = Origin.Latitude + (Radius * Math.Sin(deg2rad(Angle)));
                C.Altitude = Origin.Altitude;
                List.Add(C);

            }

            return List;
        }

        /// <summary>
        /// Creates a equalateral triangle
        /// 2022-06-20      Clay    Right Hand Rule Compliant.
        /// </summary>
        /// <param name="UV">The Unit Vector determines the scale of the triangle</param>
        /// <param name="Origin">The Coordinate that is the basis of the construction of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakePlanarEquilateralTriangleList(Double SideLength, Coordinate Origin)
        {  List<Coordinate> List = new List<Coordinate>();
            List.Add(Origin);
            List.Add(DestinationCoordinate(Origin, SideLength, 210));
            List.Add(DestinationCoordinate(Origin, SideLength, 150));
             return List;
        }

       

        /// <summary>
        /// Makes an equilateral triangle based in geodesic geometry
        /// 2022-06-20      Clay    Right Hand Rule Compliant.
        /// </summary>
        /// <param name="SideLength"></param>
        /// <param name="Origin"></param>
        /// <returns></returns>
        public List<Coordinate> MakeGeodesicEquilateralTriangleList(Double UV, Coordinate Origin)
        {
          List<Coordinate> List = new List<Coordinate>();

            GeoTools OG = new GeoTools("");
            List.Add(OG.DestinationCoordinate(Origin, UV, 0));
            List.Add(OG.DestinationCoordinate(Origin, UV, 120));
            List.Add(OG.DestinationCoordinate(Origin, UV, 220));
            return List;

           
        }

        /// <summary>
        /// Adding a Vector to a Coordinate to move the Coordinate. This is a Planar Transformation.
        /// 
        /// </summary>
        /// <param name="TheShape">The list of coordinates you want to move</param>
        /// <param name="TheTranslation">The vector that describes the move.</param>
        /// <returns></returns>
        public List<Coordinate> Translation(List<Coordinate> TheShape, Vector TheTranslation)
        {
            List<Coordinate> List = new List<Coordinate>();

            foreach (Coordinate C in TheShape)
            {
                List.Add(new Coordinate(C.Latitude + TheTranslation.Latitude, C.Longitude + TheTranslation.Longitude, C.Altitude));
            }

            return List;
        }

        /// <summary>
        /// Reflects a list passed into it.
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Origin"></param>
        /// <param name="Axis">Pass Lat and Long values to describe the symmetery. 2 will be a full reflection. 0</param>
        /// <returns></returns>
        public List<Coordinate> Reflection(List<Coordinate> TheShape, Coordinate Origin, Vector Axis)
        {
            List<Coordinate> List = new List<Coordinate>();

            foreach (Coordinate Coordinate in TheShape)
            {
                Vector V = new Vector((Origin.Latitude - Coordinate.Latitude), (Origin.Longitude - Coordinate.Longitude));
                List.Add(new Coordinate(Origin.Latitude + (V.Latitude * (Axis.Latitude)), Origin.Longitude + (V.Longitude * (Axis.Longitude)), Origin.Altitude));
            }
            return List;

        }

        /// <summary>
        /// Planar dilation of a shape centered on the origin. 
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Origin"></param>
        /// <param name="ScaleFactor"></param>
        /// <returns></returns>
        public List<Coordinate> Dilation(List<Coordinate> TheShape, Coordinate Origin, Double ScaleFactor)
        {
            List<Coordinate> List = new List<Coordinate>();

            foreach (Coordinate C in TheShape)
            {
                //Figure out the vector for each coordinate in the shape
                Vector TheTranslation = new Vector(C.Latitude - Origin.Latitude, C.Longitude - Origin.Longitude);
                TheTranslation.Latitude = TheTranslation.Latitude * ScaleFactor;
                TheTranslation.Longitude = TheTranslation.Longitude * ScaleFactor;
                List.Add(new Coordinate(C.Latitude + TheTranslation.Latitude, C.Longitude + TheTranslation.Longitude, C.Altitude));
            }

            return List;
        }

        /// <summary>
        /// Geodesic dilation of a shape centered omn the origin.
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Origin"></param>
        /// <param name="ScaleFactor"></param>
        /// <returns></returns>
        public List<Coordinate> GeodesicDilation(List<Coordinate> TheShape, Coordinate Origin, Double ScaleFactor)
        {
            List<Coordinate> List = new List<Coordinate>();

            foreach (Coordinate C in TheShape)
            {
                //I need to know the angle between the the Origin and the current coordinate
                double bearing = BearingBetween(C, Origin);
                //Now I need to know how far away it is....
                double distance = Distance(C, Origin);

                //Then I add my adjustment....
                distance = distance * ScaleFactor;
                //Then this nice nested call...
                List.Add(DestinationCoordinate(Origin, distance, bearing));
                //That coordinate is adjusted and we loop again to do the next one.    
            }

            return List;
        }

        /// <summary>
        /// Performs a Geodesic Rotation.
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Origin">The center of the Rotation</param>
        /// <param name="Angle">The angle in degrees.</param>
        /// <returns></returns>
        public List<Coordinate> GeodesicRotation(List<Coordinate> TheShape, Coordinate Origin, Double Angle)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (int i = 0; i < TheShape.Count; i++)
            {
                //I need to know the angle between the the current coordinate and the Origin.
                Origin.Altitude = TheShape[i].Altitude;
                double bearing = BearingBetween(Origin, TheShape[i]);
                //Then I add my adjustment....
                bearing = bearing + Angle % 360;
                //Now I need to know how far away it is....
                double distance = Distance(Origin, TheShape[i]);

                //Then this nice nested call...
                List.Add(DestinationCoordinate(Origin, distance, bearing));
                //That coordinate is adjusted and we loop again to do the next one.                
            }
            return List;
        }

        /// <summary>
        /// Translation. Beware the limits of distance before distortion.
        /// </summary>
        /// <param name="TheShape"></param>
        /// <param name="Distance"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        public List<Coordinate> GeodesicTranslation(List<Coordinate> TheShape, double Distance, Double Angle)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (int i = 0; i < TheShape.Count; i++)
            {

                List.Add(DestinationCoordinate(TheShape[i], Distance, Angle));

            }
            return List;
        }

        /// <summary>
        /// Makes a circle upon a sphere.
        /// 2022-06-20      Clay    Right Hand Rule Compliant.
        /// </summary>
        /// <param name="Radius">In Kms</param>
        /// <param name="Origin">The Origin of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakeGeodesicCircleList(Double Radius, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();
            for (double angle = 360; angle >= 0; --angle)
            {
                if (angle % 15 == 0)
                {
                    Coordinate C = DestinationCoordinate(Origin, Radius, angle);
                    List.Add(C);
                }
            }

            return List;
        }

        /// <summary>
        /// Creates a square that has 6 Coordinates.
        /// 2022-06-20      Clay    Right Hand Rule Compliant. Hopefully...
        /// </summary>
        /// <param name="UV">Unit Vector is the scale factor of the shape.</param>
        /// <param name="Origin">The Coordinate that is the basis of the construction of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakePlanarSquareList(Double UV, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();

            List.Add(new Coordinate(Origin.Latitude + UV, Origin.Longitude + UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + UV, Origin.Longitude + (UV / 2), Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + UV, Origin.Longitude, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + UV, Origin.Longitude - (UV / 2), Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + UV, Origin.Longitude - UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + (UV / 2), Origin.Longitude - UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude, Origin.Longitude - UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - (UV / 2), Origin.Longitude - UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - UV, Origin.Longitude - UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - UV, Origin.Longitude - (UV / 2), Origin.Altitude));

            List.Add(new Coordinate(Origin.Latitude - UV, Origin.Longitude, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - UV, Origin.Longitude + (UV / 2), Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - UV, Origin.Longitude + UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude - (UV / 2), Origin.Longitude + UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude, Origin.Longitude + UV, Origin.Altitude));
            List.Add(new Coordinate(Origin.Latitude + (UV / 2), Origin.Longitude + UV, Origin.Altitude));


            return List;
        }

        /// <summary>
        /// Makes a geodesic geometry spiral.
        /// </summary>
        /// <param name="delta">delta means an amount of change</param>
        /// <param name="Unit_Angle">The scale factor. Keep it small to start. 0.005</param>
        /// <param name="revolutions"></param>
        /// <param name="Origin"></param>
        /// <returns></returns>
        public List<Coordinate> MakeGeodesicSpiralList(double delta, double Unit_Angle, double revolutions, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();
            List.Add(Origin);
            //Theta is a greek letter that represents an Angle.
            double theta = 0;
            double radius = 0;

            //How does it work?
            //The while loop will rotate as long as the Angle (theta) is less than revolutions*360.
            //If you pass in 2 revolutions that would be a 720 
            while (theta <= (revolutions * 360))
            {
                //The delta represents the idea of the amount of change
                //So each time we loop in we increase the theta by a set amount. delta is what decides
                //the distance between Coordinates.
                //2 for the delta is 2 degrees between loops.
                theta += delta;

                //So you see that the radius keeps getting bigger. As we turn around the circle.
                //But the radius can get soooo big that we create invalid lats and longs. So I have a 
                //scalar called the Unit_Angle. It "makes the spiral smaller". 0.005 is a good value to start
                radius = theta * (Unit_Angle);

                //Then we figure out the destination to be turned into the new Origin as
                //we loop around again.
                //Note the formula below! Then compare it to the circle formula I included in 
                //the commented section below. What makes the Spiral attractive?? 
                // I tightened the ratio of the angle to 1/360th theta/360
                //Now a smaller  number like theta/(revolutions * 360)
                //theta/180* (Math.PI*2)
                Coordinate C = DestinationCoordinate(Origin, radius * (deg2rad(theta)), theta);

                List.Add(C);
            }
            return List;


        }

        /// <summary>
        /// Makes an exponential curve.
        /// </summary>
        /// <param name="GrowthRate">The growth rate of the curve</param>
        /// <param name="UV">The Unit Vector changes the scale</param>
        /// <param name="Origin">The Coordinate that is the basis of the construction of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakeExponentialList(Double GrowthRate, Double UV, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (Double x = Origin.Longitude; x < Origin.Longitude + (100 * UV); x += UV)
            {
                Double Longitude = x;
                Double Latitude = Math.Pow((GrowthRate), (x)) + Origin.Latitude;
                List.Add(new Coordinate(Latitude, Longitude, Origin.Altitude));
            }
            return List;
        }

        /// <summary>
        /// Makes a logarithmic curve
        /// </summary>
        /// <param name="GrowthRate">The growth rate of the curve</param>
        /// <param name="UV">The Unit Vector changes the scale</param>
        /// <param name="Origin">The Coordinate that is the basis of the construction of the shape</param>
        /// <returns></returns>
        public List<Coordinate> MakeLogList(Double GrowthRate, Double UV, Coordinate Origin)
        {
            List<Coordinate> List = new List<Coordinate>();

            for (Double x = Origin.Longitude; x < Origin.Longitude + (100 * UV); x += UV)
            {
                Double Longitude = x;
                Double Latitude = Math.Log(x, GrowthRate) + Origin.Latitude;
                if (Double.IsInfinity(Latitude) == false)
                {
                    List.Add(new Coordinate(Latitude, Longitude, Origin.Altitude));
                }

            }
            return List;
        }


        


        /// <summary>
        /// Sick and tired of Multi Geometries making your life awful? Try this function to make them all 
        /// regular polygons! Poof. Tada easier to do stuff with.
        /// </summary>
        /// <param name="LP"></param>
        /// <returns></returns>
        public List<Placemark> RemoveMultiGeometry(List<Placemark> LP)
        {
            List<Placemark> returnList = new List<Placemark>();

            for (int i = 0; i < LP.Count; i++)
            {
                if (LP[i].Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
                {
                    //This is always the tricky bit. We are building an xml container...don't panic.
                    SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)LP[i].Geometry.Clone();

                    foreach (SharpKml.Dom.Geometry PolyThePolyGon in temp.Geometry)
                    {
                        SharpKml.Dom.Placemark PlaceyThePlaceMark = new SharpKml.Dom.Placemark();
                        // temp.Geometry = new SharpKml.Dom.Polygon();
                        PlaceyThePlaceMark.Geometry = PolyThePolyGon.Clone();

                        if (String.IsNullOrEmpty(LP[i].Name))
                        {
                            PlaceyThePlaceMark.Name = "No Name";
                        }
                        else
                        {
                            PlaceyThePlaceMark.Name = LP[i].Name;
                        }
                        PlaceyThePlaceMark.Description = LP[i].Description;

                        PlaceyThePlaceMark.AddStyle(LP[i].Styles.FirstOrDefault().Clone());
                        if (LP[i].ExtendedData != null)
                        {
                            PlaceyThePlaceMark.ExtendedData = LP[i].ExtendedData.Clone();
                        }
                        //The cloning is because of containers. See lists. West World?                                                
                        returnList.Add(PlaceyThePlaceMark.Clone());
                    }

                }
                else
                {
                    returnList.Add(LP[i].Clone());
                }
            }
            return returnList;
        }

        /// <summary>
        /// Some people like to make things random for testing purposes...
        /// Useful to figure how to use styling to make you own styling functions.
        /// 
        /// </summary>
        /// <param name="YourModel"></param>
        /// <param name="extrude"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private List<Placemark> RandoStyle(List<Placemark> YourModel, bool extrude, SharpKml.Dom.AltitudeMode mode)
        {
            Random RandoTron = new Random();
            Double RandomHeight = RandoTron.NextDouble() * 100;
            Double NewBetterRandom = RandoTron.Next(1000, 25000);

            for (int i = 0; i < YourModel.Count; i++)
            {
                NewBetterRandom = RandoTron.Next(1000, 25000);
                SharpKml.Dom.Style style = new SharpKml.Dom.Style();
                style.Polygon = new PolygonStyle();
                style.Polygon.ColorMode = ColorMode.Normal;

                Color32 Color = new Color32(255,
                                            Convert.ToByte(RandoTron.Next(0, 255)),
                                            Convert.ToByte(RandoTron.Next(0, 255)),
                                            Convert.ToByte(RandoTron.Next(0, 255))
                                            );
                style.Polygon.Color = Color;
                style.Line = new LineStyle();
                style.Line.ColorMode = ColorMode.Normal;
                style.Line.Color = Color.Clone();

                if (YourModel[i].Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                {
                    YourModel[i].AddStyle(style.Clone());
                    SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)YourModel[i].Geometry.Clone();
                    temp.Extrude = extrude;
                    temp.AltitudeMode = mode;

                    foreach (SharpKml.Base.Vector CO in temp.OuterBoundary.LinearRing.Coordinates)
                    {
                        CO.Altitude = NewBetterRandom;
                    }


                    foreach (SharpKml.Dom.InnerBoundary IB in temp.InnerBoundary)
                    {
                        foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                        {
                            CO.Altitude = NewBetterRandom;
                        }
                    }
                    YourModel[i].Geometry = temp;
                }

                if (YourModel[i].Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
                {

                    YourModel[i].AddStyle(style.Clone());
                    SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)YourModel[i].Geometry.Clone();

                    foreach (SharpKml.Dom.Polygon PO in temp.Geometry)
                    {
                        PO.Extrude = extrude;
                        PO.AltitudeMode = mode;

                        foreach (SharpKml.Base.Vector CO in PO.OuterBoundary.LinearRing.Coordinates)
                        {
                            CO.Altitude = NewBetterRandom;
                        }

                        foreach (SharpKml.Dom.InnerBoundary IB in PO.InnerBoundary)
                        {
                            foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                            {
                                CO.Altitude = NewBetterRandom;
                            }
                        }
                    }
                    YourModel[i].Geometry = temp;

                }
            }

            return YourModel;
        }


        /// <summary>
        /// This function literally styles a placemark exactly the way you need. :) 
        /// </summary>
        /// <param name="YourModel"></param>
        /// <param name="extrude"></param>
        /// <param name="mode"></param>
        /// <param name="Fill"></param>
        /// <param name="LineColor"></param>
        /// <param name="PlacemarkName"></param>
        /// <returns></returns>
        public Placemark SampleStyler(Placemark YourModel, bool extrude, SharpKml.Dom.GX.AltitudeMode mode, Color32 Fill, Color32 LineColor, String PlacemarkName)
        {

            SharpKml.Dom.Style style = new SharpKml.Dom.Style();
            style.Polygon = new PolygonStyle();
            style.Polygon.ColorMode = ColorMode.Normal;


            style.Polygon.Color = Fill;
            style.Line = new LineStyle();
            style.Line.ColorMode = ColorMode.Normal;
            style.Line.Color = LineColor;

            if (YourModel.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
            {
                YourModel.AddStyle(style.Clone());
                SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)YourModel.Geometry.Clone();
                temp.Extrude = extrude;
                temp.GXAltitudeMode = mode;
                YourModel.Geometry = temp.Clone();
            }

            if (YourModel.Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
            {

                YourModel.AddStyle(style.Clone());
                SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)YourModel.Geometry.Clone();

                foreach (SharpKml.Dom.Polygon PO in temp.Geometry)
                {
                    PO.Extrude = extrude;
                    PO.GXAltitudeMode = mode;
                }
                YourModel.Geometry = temp;

            }
            YourModel.Name = PlacemarkName;
            return YourModel;
        }


        public Placemark SampleStyler(Placemark YourModel, bool extrude, SharpKml.Dom.AltitudeMode mode, Color32 Fill, Color32 LineColor, String PlacemarkName)
        {

            SharpKml.Dom.Style style = new SharpKml.Dom.Style();
            style.Polygon = new PolygonStyle();
            style.Polygon.ColorMode = ColorMode.Normal;


            style.Polygon.Color = Fill;
            style.Line = new LineStyle();
            style.Line.ColorMode = ColorMode.Normal;
            style.Line.Color = LineColor;

            if (YourModel.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
            {
                YourModel.AddStyle(style.Clone());
                SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)YourModel.Geometry.Clone();
                temp.Extrude = extrude;
                temp.AltitudeMode = mode;
                YourModel.Geometry = temp.Clone();
            }

            if (YourModel.Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
            {

                YourModel.AddStyle(style.Clone());
                SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)YourModel.Geometry.Clone();

                foreach (SharpKml.Dom.Polygon PO in temp.Geometry)
                {
                    PO.Extrude = extrude;
                    PO.AltitudeMode = mode;
                }
                YourModel.Geometry = temp;

            }
            YourModel.Name = PlacemarkName;
            return YourModel;
        }


        /// <summary>
        /// This rotates polygons and multis in the model about the UK. Points and lines dont move.
        /// You may need to code that yurself based on this function as the template. Worth doing.....
        /// Adds a random height to it and a random color. USEFUL FOR YOUR LABWORK
        /// </summary>
        /// <param name="YourModel"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        private List<Placemark> RotateAbout(List<Placemark> YourModel, double Angle, Coordinate TheFocus)
        {
            //Restrict the rotation.
            Angle = Angle % 360;


           // Coordinate TheFocus = new Coordinate(51.504233, -3.228245, 0);

            for (int i = 0; i < YourModel.Count; i++)
            {
                if (YourModel[i].ExtendedData != null)
                {


                    ExtendedData ED = new ExtendedData();
                    //We try to see if it has extended data. Useful for UK. But worth learning next semseter
                    //Might be null

                    SchemaData ScHED = YourModel[i].ExtendedData.SchemaData.FirstOrDefault();
                    List<SimpleData> test = ScHED.SimpleData.ToList();

                    //Next semester we learn an easier way to deal with this issue.
                    //but for now we do it the hard way.
                    //We are getting the County Names from the MetaData.
                    foreach (SimpleData testData in test)
                    {
                        if (testData.Name == "NAME2")
                        {
                            YourModel[i].Name = testData.Text;
                        }
                    }
                }


                if (YourModel[i].Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                {

                    SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)YourModel[i].Geometry.Clone();
                    List<Coordinate> tempList = new List<Coordinate>();
                    SharpKml.Dom.CoordinateCollection C = (CoordinateCollection)temp.Flatten().ToList()[3];
                    List<SharpKml.Base.Vector> D = C.ToList();

                    foreach (SharpKml.Base.Vector CO in temp.OuterBoundary.LinearRing.Coordinates)
                    {
                        tempList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                    }
                    List<Coordinate> tempList2 = this.GeodesicRotation(tempList, TheFocus, Angle);

                    LinearRing TempRing = new LinearRing();
                    TempRing.Coordinates = new CoordinateCollection();
                    foreach (Coordinate C2 in tempList2)
                    {
                        TempRing.Coordinates.Add(new SharpKml.Base.Vector(C2.Latitude, C2.Longitude, C2.Altitude));
                    }
                    temp.OuterBoundary.LinearRing = TempRing.Clone();

                    foreach (SharpKml.Dom.InnerBoundary IB in temp.InnerBoundary)
                    {
                        tempList.Clear();

                        foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                        {
                            tempList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                        }

                        tempList2 = this.GeodesicRotation(tempList, TheFocus, Angle);

                        TempRing = new LinearRing();
                        TempRing.Coordinates = new CoordinateCollection();
                        foreach (Coordinate C2 in tempList2)
                        {
                            TempRing.Coordinates.Add(new SharpKml.Base.Vector(C2.Latitude, C2.Longitude, C2.Altitude));
                        }
                        IB.LinearRing = TempRing.Clone();
                    }
                    YourModel[i].Geometry = temp.Clone();
                }

                if (YourModel[i].Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
                {


                    SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)YourModel[i].Geometry.Clone();

                    foreach (SharpKml.Dom.Polygon PO in temp.Geometry)
                    {
                        SharpKml.Dom.Polygon t = PO.Clone();
                        List<Coordinate> tempList = new List<Coordinate>();
                        SharpKml.Dom.CoordinateCollection C = (CoordinateCollection)PO.Flatten().ToList()[3];
                        List<SharpKml.Base.Vector> D = C.ToList();

                        foreach (SharpKml.Base.Vector CO in PO.OuterBoundary.LinearRing.Coordinates)
                        {
                            tempList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                        }
                        List<Coordinate> tempList2 = this.GeodesicRotation(tempList, TheFocus, Angle);

                        LinearRing TempRing = new LinearRing();
                        TempRing.Coordinates = new CoordinateCollection();
                        foreach (Coordinate C2 in tempList2)
                        {
                            TempRing.Coordinates.Add(new SharpKml.Base.Vector(C2.Latitude, C2.Longitude, C2.Altitude));
                        }
                        PO.OuterBoundary.LinearRing = TempRing.Clone();


                        foreach (SharpKml.Dom.InnerBoundary IB in PO.InnerBoundary)
                        {
                            tempList.Clear();

                            foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                            {
                                tempList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                            }

                            tempList2 = this.GeodesicRotation(tempList, TheFocus, Angle);

                            TempRing = new LinearRing();
                            TempRing.Coordinates = new CoordinateCollection();
                            foreach (Coordinate C2 in tempList2)
                            {
                                TempRing.Coordinates.Add(new SharpKml.Base.Vector(C2.Latitude, C2.Longitude, C2.Latitude));
                            }
                            IB.LinearRing = TempRing.Clone();
                        }
                        YourModel[i].Geometry = temp.Clone();
                    }
                }
            }
            return YourModel;
        }

        /// <summary>
        /// Close but approximation of the area of a coordinate collection
        /// 2023-12-25  Opened this function use outside the class.
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public double GetArea(List<Coordinate> C)
        {

            //List<SharpKml.Base.Vector> coords = C.ToList();

            // Add all coordinates to a list, converting them to meters:
            List<SharpKml.Base.Vector> points = new List<SharpKml.Base.Vector>();
            foreach (Coordinate coord in C)
            {
                SharpKml.Base.Vector p = new SharpKml.Base.Vector(
                          // 6378137   6378137.01
                          coord.Longitude * (System.Math.PI * 6356752.3142 / 180),
                          coord.Latitude * (System.Math.PI * 6356752.3142 / 180)
                        );
                points.Add(p);
            }

            // Calculate polygon area (in square meters):
            return System.Math.Abs(points.Take(points.Count - 1)
              .Select((p, i) => (points[i + 1].Latitude - p.Latitude) * (points[i + 1].Longitude + p.Longitude))
              .Sum() / 2);
        }


        /// <summaryspawn
        /// This function figures out the length of a coordinate collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public double GetLength(SharpKml.Dom.CoordinateCollection Thang)
        {
            double length = 0;
            //This makes the Coordinate Object a List of Vectors we can turn into Coordinates
            List<SharpKml.Base.Vector> VecList = Thang.ToList();
            //This is a fun way to calculate the length. For Loop
            //Notice I stop 1 early in the loop
            for (int i = 0; i < VecList.Count - 1; i++)
            {
                //So we need to have two coordinates as we loop through
                //the Start Coordinate
                Coordinate Start =
                    new Coordinate(VecList[i].Latitude,
                                   VecList[i].Longitude,
                                   Convert.ToDouble(VecList[i].Altitude));

                //And the End Coordinate. This is why stop early.
                Coordinate End =
                    new Coordinate(VecList[i + 1].Latitude,
                                   VecList[i + 1].Longitude,
                                   Convert.ToDouble(VecList[i + 1].Altitude));

                //Then our Geotools "Brain Object" does the work for us. The Control
                length += this.Distance(Start, End);
            }
            return length;
        }


        /// <summary>
        /// This makes the cheese model. 
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="UV"></param>
        /// <returns></returns>
        private List<Placemark> MakeCheese(Coordinate Origin, double UV)
        {
            //Using our good friend RandoTron! Gotta keep the cheese random.
            Random Randotron = new Random();

            //Lets make the Origin's Elevation random for each cheese.
            Origin.Altitude = Randotron.NextDouble() * 100 + Origin.Altitude;

            //You can set the height of your polygon at the orgin and the code should be
            //continue to preserve that altitude. Remember the great UK rotation disater of June 2022.
            //Setting the height might be useful for lab work.
            Coordinate SwissCheeseOrigin = this.DestinationCoordinate(Origin, UV * 3, 270);

            Origin.Altitude = Randotron.NextDouble() * 100 + Origin.Altitude;
            Coordinate AmericanCheeseOrigin = this.DestinationCoordinate(Origin, UV * 3, 90);

            Origin.Altitude = Randotron.NextDouble() * 100 + Origin.Altitude;
            Coordinate CheeseStringOrigin = this.DestinationCoordinate(Origin, UV * 3, 180);

            //Making Swiss Cheese
            Polygon SwissCheesePoly = new Polygon();
            SwissCheesePoly.LinearList = this.MakeGeodesicCircleList((Randotron.NextDouble() * UV) + UV, SwissCheeseOrigin);
            //Now the holes for the cheese. Three Random holes. Every run produces a different slice of cheese...within boundaries of the UV.
            //SwissCheesePoly.InnerBoundaryList.Add(TheControl.MakeGeodesicCircleList(0.25, SwissCheeseOrigin));
            SwissCheesePoly.InnerBoundaryList.Add(this.MakeGeodesicCircleList(Randotron.NextDouble() * UV, this.DestinationCoordinate(SwissCheeseOrigin, Randotron.NextDouble() * UV, Randotron.NextDouble() * 359.999)));
            SwissCheesePoly.InnerBoundaryList.Add(this.MakeGeodesicCircleList(Randotron.NextDouble() * UV, this.DestinationCoordinate(SwissCheeseOrigin, Randotron.NextDouble() * UV, Randotron.NextDouble() * 359.999)));
            SwissCheesePoly.InnerBoundaryList.Add(this.MakeGeodesicCircleList(Randotron.NextDouble() * UV, this.DestinationCoordinate(SwissCheeseOrigin, Randotron.NextDouble() * UV, Randotron.NextDouble() * 359.999)));


            //Making the American Cheese

            Polygon AmericanCheese = new Polygon();
            AmericanCheese.LinearList = this.MakeGeodesicSquareList((Randotron.NextDouble() * 3), AmericanCheeseOrigin);
            //American Cheese has no holes. But they come in two slices.....So we need a multipolygon
            MultiPolygon TwoSlicesOfAmericanCheese = new MultiPolygon();
            TwoSlicesOfAmericanCheese.PolyGonList.Add(AmericanCheese);
            //Now we need to add the second slice...we will do this with a rotation and a transition.
            Polygon AmericanCheeseAnotherSlice = new Polygon();
            AmericanCheeseAnotherSlice.LinearList = this.GeodesicRotation(AmericanCheese.LinearList, AmericanCheeseOrigin, Randotron.NextDouble() * 359.999);
            AmericanCheeseAnotherSlice.LinearList = this.GeodesicTranslation(AmericanCheeseAnotherSlice.LinearList, Randotron.NextDouble() * UV, Randotron.NextDouble() * 359.9999);

            TwoSlicesOfAmericanCheese.PolyGonList.Add(AmericanCheeseAnotherSlice);

            //Making the Mozzerella Cheese String
            Line MozzyStick = new Line();
            MozzyStick.CoordinateList = this.MakeGeodesicSpiralList(Randotron.NextDouble() * UV, 0.005, 2, CheeseStringOrigin);


            //We make PlaceMarks out of them. Give them names and style them
            Placemark SwissPlacemark = SwissCheesePoly.ToPlaceMark(true, VerticalDatum.RelativeToSeafloor);
            Placemark AmericanCheesePlacemark = TwoSlicesOfAmericanCheese.ToPlacemark(true, VerticalDatum.RelativeToSeafloor);
            Placemark MozzyCheese = MozzyStick.ToPlacemark(true, VerticalDatum.RelativeToSeafloor);

            SwissPlacemark.Name = "Swiss Cheese.";
            AmericanCheesePlacemark.Name = "Two slices of American processed cheese";
            MozzyCheese.Name = "A curious spiral of Mozzy Cheese.";



            //Now we need to style them. 
            SharpKml.Dom.Style CheeseStyle = new SharpKml.Dom.Style();
            CheeseStyle.Line = new LineStyle();
            CheeseStyle.Line.ColorMode = ColorMode.Normal;
            CheeseStyle.Line.Width = 5;
            CheeseStyle.Line.Color = new Color32(255, 127, 231, 255);
            CheeseStyle.Polygon = new PolygonStyle();
            CheeseStyle.Polygon.Color = new Color32(255, 127, 231, 255);
            CheeseStyle.Polygon.ColorMode = ColorMode.Normal;

            //Notice I clone them.
            SwissPlacemark.AddStyle(CheeseStyle.Clone());

            //American cheese is orange....
            CheeseStyle.Line.Color = new Color32(255, 0, 181, 255);
            CheeseStyle.Polygon.Color = new Color32(255, 0, 181, 255);

            AmericanCheesePlacemark.AddStyle(CheeseStyle.Clone());

            //Mozzy is close to beige
            CheeseStyle.Line.Color = new Color32(255, 204, 245, 245);
            CheeseStyle.Polygon.Color = new Color32(255, 250, 250, 200);

            MozzyCheese.AddStyle(CheeseStyle.Clone());

            List<Placemark> returnList = new List<Placemark>();
            returnList.Add(SwissPlacemark.Clone());
            returnList.Add(AmericanCheesePlacemark.Clone());
            returnList.Add(MozzyCheese);

            return returnList;


        }


        /// <summary>
        /// This function is Polygon Cutter! You need it cut?
        /// This function returns a Placemark that holds a 
        /// polygon of what the "Slice would be". Its also imperfect because as you can see
        /// everyones cut gets "smaller" from different perspective. 
        /// </summary>
        /// <param name="StartMeridian"></param>
        /// <param name="Endmeridian"></param>
        /// <param name="Pie"></param>
        /// <param name="PolyName"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private Placemark SliceThePie(Double StartMeridian, Double Endmeridian, Placemark Pie, String PolyName, SharpKml.Dom.Style style)
        {
            Placemark returnMark = new Placemark();

            if (Pie.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
            {
                SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)Pie.Geometry.Clone();
                temp.AltitudeMode = SharpKml.Dom.AltitudeMode.ClampToGround;
                List<Coordinate> tempList = new List<Coordinate>();
                SharpKml.Dom.CoordinateCollection C = (CoordinateCollection)temp.Flatten().ToList()[3];
                List<SharpKml.Base.Vector> D = C.ToList();
                List<SharpKml.Base.Vector> newList = new List<SharpKml.Base.Vector>();

                for (int i = 0; i < D.Count; i++)
                {
                    if (D[i].Longitude > StartMeridian && D[i].Longitude < Endmeridian)
                    {
                        newList.Add(D[i]);
                    }
                }
                SharpKml.Dom.Polygon SlicedPolygon = new SharpKml.Dom.Polygon();
                SlicedPolygon.OuterBoundary = new OuterBoundary();
                SlicedPolygon.OuterBoundary.LinearRing = new LinearRing();
                SlicedPolygon.OuterBoundary.LinearRing.Coordinates = new CoordinateCollection();
                foreach (SharpKml.Base.Vector V in newList)
                {
                    SlicedPolygon.OuterBoundary.LinearRing.Coordinates.Add(V);
                }
                returnMark.AddStyle(style);
                returnMark.Geometry = SlicedPolygon.Clone();
                returnMark.Name = PolyName;
            }
            return returnMark;
        }


    }


}
