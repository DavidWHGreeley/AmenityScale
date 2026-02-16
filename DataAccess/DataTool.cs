using Geotool_Objects;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace DataAccess
{
    /// <summary>
    /// Version         Date        Coder           Remarks
    /// 1.0             2015-01-01  Clay            Initial Version
    /// 1.1             2022-12-23  Shake Spear!    A file Reader/Writer and Database Tool
    /// 2.1             2023-01-12  Shake Spear!    Implementing Vetical Datum.
    /// 
    public class DataTool
    {
        //DO NOT CHANGE ANY OTHER CODE IN THIS SOLUTION.
        //JUST CHANGE YOUR CONNECTION STRING
        //DO NOT CHANGE ANYTHING ELSE!
        private static String GetConnectionString()
        {
            return @"Server=Server=DESKTOP-F2RBVRA\MSSQLSERVER01;Database=DB_AmeniScale;Trusted_Connection=Yes;";
        }


        /// <summary>
        /// Gets All the Shapes that are active in DB_Toril
        /// Date        Name                    Vers    Comments
        /// 2022-10-01  Shake Spear!            1.0     Initial
        /// 2022-11-18  Haotian                 1.1     Updated with bug fix from Haotian.
        /// 2022-12-24  Clay                    1.2     Fixed STDifference Stripping Elevation Data
        /// </summary>
        /// <returns></returns>
        public List<Placemark> GetAllShapes()
        {
            // throw new NotImplementedException();
            List<Placemark> Shapes = new List<Placemark>();

            System.Data.SqlClient.SqlDataReader t;


            SharpKml.Dom.Style s = new SharpKml.Dom.Style();
            String text = "";
            int iD = 0;
            try
            {
                t = PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_GetAllShapes");
                if (t.HasRows)
                {
                    while (t.Read())
                    {
                        iD = t.GetInt32(0);
                        //Amenity_ID,0 -------
                        //AmenityName,1 -----
                        //LabelScale,2 --------------------
                        //IconScale,3------
                        //AmenityDescription,4 --------------
                        //IconName,		5-----
                        //ShapeString,6
                        //FillRed,7
                        //FillGreen,8
                        //FillBlue,9
                        //FillAlpha,10
                        //LineRed,11
                        //LineGreen,12
                        //LineBlue,13
                        //LineAlpha,14
                        //IconRed,15  -----
                        //IconGreen,16----
                        //IconBlue,17----
                        //IconAlpha,18-----
                        //LineWeight,19
                        //Extrude, 20
                        //Vertical Datum, 21
                        try
                        {
                            //Shake Spear! Vertical Datum.
                            bool ExtRuuuuude = Convert.ToBoolean(t[20]);

                            //Now I have an enum that holds a whole bunch of vertical datums. Two of which have different libraries!
                            //Enums make things easier. See below where I use them.
                            VerticalDatum VDatum = (VerticalDatum)t[21];


                            String Shape = t[6].ToString();


                            //We have the data from the database. Now we start by building Points
                            //Fix any nonesense 
                            Shape = Shape.Replace("), (", "),(");
                            Shape = Shape.Replace(") , (", "),(");
                            Shape = Shape.Replace(") ,(", "),(");

                            ///Make brand new copy each time we loop.
                            s = new SharpKml.Dom.Style();

                            if (Shape.Contains("POINT"))
                            {
                                s.Icon = new IconStyle();
                                s.Icon.Scale = double.Parse(t[3].ToString());
                                s.Icon.Color = new Color32(Byte.Parse(t[18].ToString()), Byte.Parse(t[17].ToString()),
                                    Byte.Parse(t[16].ToString()), Byte.Parse(t[15].ToString()));
                                s.Label = new LabelStyle();
                                s.Label.Scale = double.Parse(t[2].ToString());
                                s.Label.Color = new Color32(255, 255, 255, 255);

                                //Clean The String
                                Shape = Shape.Replace("POINT ", "");
                                Shape = Shape.Replace("  ", " ");
                                Shape = Shape.Replace("(", "");
                                Shape = Shape.Replace(")", "");

                                Shape = Shape.Trim();
                                //Lets break the Shape String up.
                                String[] b = Shape.Split(' ');

                                Coordinate C = new Coordinate(Double.Parse(b[1]), Double.Parse(b[0]),
                                    Double.Parse(b[2]));
                                s.Icon.Icon = new IconStyle.IconLink(new Uri(t[5].ToString()));

                                Placemark P = C.ToPlaceMark(ExtRuuuuude, VDatum);

                                P.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                P.Description = D.Clone();
                                P.Id = t[0].ToString();

                                P.AddStyle(s.Clone());

                                Shapes.Add(P.Clone());
                            }

                            else if (Shape.Contains("LINESTRING"))
                            {
                                s.Label = new LabelStyle();
                                s.Label.Scale = double.Parse(t[2].ToString());
                                s.Label.Color = new Color32(255, 255, 255, 255);

                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));

                                //Clean The String
                                Shape = Shape.Replace("LINESTRING ", "");
                                Shape = Shape.Replace("(", "");
                                Shape = Shape.Replace(")", "");


                                //Lets break the Shape String up.

                                //Step 1: Split the coordinate pairs up 
                                String[] b = Shape.Split(',');
                                //I define the line. 
                                Geotool_Objects.Line Geometry = new Geotool_Objects.Line();
                                //And now the programmatic pleasure....
                                //I define the line.        
                                foreach (String Coord in b)
                                {
                                    //So now I split the coordinate up with the whitespace
                                    //Trim any leading whitespaces

                                    String[] c = Coord.Trim().Split(' ');
                                    Geometry.CoordinateList.Add(new Coordinate(Double.Parse(c[1]), Double.Parse(c[0]),
                                        Double.Parse(c[2])));
                                }

                                //Convert the whole thing to a KML placemark
                                Placemark PlaceyThePlacemark = Geometry.ToPlacemark(ExtRuuuuude, VDatum);

                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();

                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            else if (Shape.Contains("MULTIPOLYGON") &&
                                     (Shape.Contains(")),((") || (Shape.Contains(")), (("))))
                            {
                                Geotool_Objects.MultiPolygon Geometry = new Geotool_Objects.MultiPolygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));

                                //Clean The String
                                Shape = Shape.Replace(")), ((", ")),((");
                                Shape = Shape.Replace("MULTIPOLYGON (((", "");
                                Shape = Shape.Replace(")))", "");

                                //Since this is a multi we need to get all the polygons first.
                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { ")),((" };
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                //So now I have an Array of strings each element a seperate polygon string it may have holes....
                                foreach (String PolyString in b)
                                {
                                    Geotool_Objects.Polygon Pol = new Geotool_Objects.Polygon();
                                    //Splitting a string using a string array.
                                    String[] DelimeterInner = new string[] { "),(" };

                                    //Step 1. Split the multis up into polygons
                                    String[] bInner = PolyString.Split(DelimeterInner, StringSplitOptions.None);


                                    for (int i = 0; i < bInner.Length; ++i)
                                    {
                                        bInner[i] = bInner[i].Replace("(", "");
                                        bInner[i] = bInner[i].Replace(")", "");

                                        //This should be the outer because it is firt I think
                                        if (i == 0)
                                        {
                                            //Step 2. Split the Coordinates into Polygons
                                            String[] Coord = bInner[i].Split(',');

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), 0));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Step 2 Split the Polygons into Coordinates
                                            String[] Coord = bInner[i].Split(',');
                                            List<Coordinate> temp = new List<Coordinate>();

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                                        Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    temp.Add(
                                                        new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]), 0));
                                                }
                                            }

                                            Pol.InnerBoundaryList.Add(temp);
                                        }
                                    }

                                    Geometry.PolyGonList.Add(Pol);
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlacemark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            //This is a Polygon with holes!
                            else if (Shape.Contains("POLYGON") && (Shape.Contains("),(") || (Shape.Contains("), ("))))
                            {
                                Shape = Shape.Replace("), (", "),(");
                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));


                                //Clean The String
                                Shape = Shape.Replace("), (", "),(");
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { "),(" };

                                //Step 1. Split the multis up into polygons
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                for (int i = 0; i < b.Length; ++i)
                                {
                                    if (i == 0)
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), 0));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');
                                        List<Coordinate> temp = new List<Coordinate>();

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                                    Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]), 0));
                                            }
                                        }

                                        Geometry.InnerBoundaryList.Add(temp);
                                    }
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            //This is a regular with no holes
                            else if (Shape.Contains("POLYGON") && Shape.Contains("),(") != true)
                            {
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));


                                //Clean The String
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Step 2 Split the Polygons into Coordinates
                                String[] Coord = Shape.Split(',');

                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();

                                foreach (String Part in Coord)
                                {
                                    //Step 3 Split the Coordinate into its parts.
                                    String[] s3 = Part.Trim().Split(' ');
                                    if (s3.Count() == 3)
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            Double.Parse(s3[2])));
                                    }
                                    else
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            0));
                                    }
                                }


                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);

                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }
                        }
                        catch (Exception ex)
                        {
                            //Swallow and loop
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                String x = text;
            }


            return Shapes;
        }

        public List<Placemark> GetAllShapes(string I)
        {
            List<Placemark> Shapes = new List<Placemark>();

            System.Data.SqlClient.SqlDataReader t;


            SharpKml.Dom.Style s = new SharpKml.Dom.Style();
            String text = "";
            int iD = 0;
            try
            {
                //  t = PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), 
                //   System.Data.CommandType.Text, "SELECT Amenity_ID,AmenityName,LabelScale,IconScale,AmenityDescription,IconName,ShapeString,FillRed,FillGreen,FillBlue,FillAlpha,LineRed,LineGreen,LineBlue,LineAlpha,IconRed,IconGreen,IconBlue,IconAlpha,LineWeight,Extrude,VerticalDatum_FK FROM tbl_Amenity L JOIN tbl_Icon I ON I.Icon_ID = L.Icon_FK WHERE AmenityName = " + "'"+I+"'");
                //
                t = PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_GetAllShapes");
                if (t.HasRows)
                {
                    while (t.Read())
                    {
                        iD = t.GetInt32(0);

                        //Amenity_ID,0 -------
                        //AmenityName,1 -----
                        //LabelScale,2 --------------------
                        //IconScale,3------
                        //AmenityDescription,4 --------------
                        //IconName,		5-----
                        //ShapeString,6
                        //FillRed,7
                        //FillGreen,8
                        //FillBlue,9
                        //FillAlpha,10
                        //LineRed,11
                        //LineGreen,12
                        //LineBlue,13
                        //LineAlpha,14
                        //IconRed,15  -----
                        //IconGreen,16----
                        //IconBlue,17----
                        //IconAlpha,18-----
                        //LineWeight,19
                        //Extrude, 20
                        //Vertical Datum, 21
                        try
                        {
                            //Shake Spear! Vertical Datum.
                            bool ExtRuuuuude = Convert.ToBoolean(t[20]);

                            //Now I have an enum that holds a whole bunch of vertical datums. Two of which have different libraries!
                            //Enums make things easier. See below where I use them.
                            VerticalDatum VDatum = (VerticalDatum)t[21];


                            String Shape = t[6].ToString();


                            //We have the data from the database. Now we start by building Points
                            //Fix any nonesense 
                            //This coulld better implemented with an array for academic glory.
                            Shape = Shape.Replace("), (", "),(");
                            Shape = Shape.Replace(") , (", "),(");
                            Shape = Shape.Replace(") ,(", "),(");
                            Shape = Shape.Replace(")), ((", ")),((");

                            ///Make brand new copy each time we loop.
                            s = new SharpKml.Dom.Style();

                            if (Shape.Contains("POINT"))
                            {
                                s.Icon = new IconStyle();
                                s.Icon.Scale = double.Parse(t[3].ToString());
                                s.Icon.Color = new Color32(Byte.Parse(t[18].ToString()), Byte.Parse(t[17].ToString()),
                                    Byte.Parse(t[16].ToString()), Byte.Parse(t[15].ToString()));
                                s.Label = new LabelStyle();
                                s.Label.Scale = double.Parse(t[2].ToString());
                                s.Label.Color = new Color32(255, 255, 255, 255);

                                //Clean The String
                                Shape = Shape.Replace("POINT ", "");
                                Shape = Shape.Replace("  ", " ");
                                Shape = Shape.Replace("(", "");
                                Shape = Shape.Replace(")", "");

                                Shape = Shape.Trim();
                                //Lets break the Shape String up.
                                String[] b = Shape.Split(' ');

                                Coordinate C = new Coordinate(Double.Parse(b[1]), Double.Parse(b[0]),
                                    Double.Parse(b[2]));
                                s.Icon.Icon = new IconStyle.IconLink(new Uri(t[5].ToString()));

                                Placemark P = C.ToPlaceMark(ExtRuuuuude, VDatum);

                                P.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                P.Description = D.Clone();
                                P.Id = t[0].ToString();

                                P.AddStyle(s.Clone());

                                Shapes.Add(P.Clone());
                            }

                            else if (Shape.Contains("LINESTRING"))
                            {
                                s.Label = new LabelStyle();
                                s.Label.Scale = double.Parse(t[2].ToString());
                                s.Label.Color = new Color32(255, 255, 255, 255);

                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));

                                //Clean The String
                                Shape = Shape.Replace("LINESTRING ", "");
                                Shape = Shape.Replace("(", "");
                                Shape = Shape.Replace(")", "");


                                //Lets break the Shape String up.

                                //Step 1: Split the coordinate pairs up 
                                String[] b = Shape.Split(',');
                                //I define the line. 
                                Geotool_Objects.Line Geometry = new Geotool_Objects.Line();
                                //And now the programmatic pleasure....
                                //I define the line.        
                                foreach (String Coord in b)
                                {
                                    //So now I split the coordinate up with the whitespace
                                    //Trim any leading whitespaces

                                    String[] c = Coord.Trim().Split(' ');
                                    Geometry.CoordinateList.Add(new Coordinate(Double.Parse(c[1]), Double.Parse(c[0]),
                                        Double.Parse(c[2])));
                                }

                                //Convert the whole thing to a KML placemark
                                Placemark PlaceyThePlacemark = Geometry.ToPlacemark(ExtRuuuuude, VDatum);

                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();

                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            else if (Shape.Contains("MULTIPOLYGON") && Shape.Contains(")),((") == true)
                            {
                                Geotool_Objects.MultiPolygon Geometry = new Geotool_Objects.MultiPolygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));

                                //Clean The String
                                Shape = Shape.Replace("MULTIPOLYGON (((", "");
                                Shape = Shape.Replace(")))", "");

                                //Since this is a multi we need to get all the polygons first.
                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { ")),((" };
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                //So now I have an Array of strings each element a seperate polygon string it may have holes....
                                foreach (String PolyString in b)
                                {
                                    Geotool_Objects.Polygon Pol = new Geotool_Objects.Polygon();
                                    //Splitting a string using a string array.
                                    String[] DelimeterInner = new string[] { "),(" };

                                    //Step 1. Split the multis up into polygons
                                    String[] bInner = PolyString.Split(DelimeterInner, StringSplitOptions.None);

                                    for (int i = 0; i < bInner.Length; ++i)
                                    {
                                        if (i == 0)
                                        {
                                            //Step 2 Split the Coordinates into Polygons
                                            String[] Coord = bInner[i].Split(',');

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), 0));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Step 2 Split the Polygons into Coordinates
                                            String[] Coord = bInner[i].Split(',');
                                            List<Coordinate> temp = new List<Coordinate>();

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), 0));
                                                }
                                            }

                                            Pol.InnerBoundaryList.Add(temp);
                                        }
                                    }

                                    Geometry.PolyGonList.Add(Pol);
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlacemark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            //This is a Polygon with holes!
                            else if (Shape.Contains("POLYGON") && Shape.Contains("),(") == true)
                            {
                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));


                                //Clean The String
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { "),(" };

                                //Step 1. Split the multis up into polygons
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                for (int i = 0; i < b.Length; ++i)
                                {
                                    if (i == 0)
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), 0));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');
                                        List<Coordinate> temp = new List<Coordinate>();

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                                    Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]), 0));
                                            }
                                        }

                                        Geometry.InnerBoundaryList.Add(temp);
                                    }
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }

                            //This is a regular with no holes
                            else if (Shape.Contains("POLYGON") && Shape.Contains("),(") != true)
                            {
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(Byte.Parse(t[10].ToString()), Byte.Parse(t[9].ToString()),
                                    Byte.Parse(t[8].ToString()), Byte.Parse(t[7].ToString()));
                                s.Line = new LineStyle();
                                s.Line.Width = double.Parse(t[19].ToString());
                                s.Line.Color = new Color32(Byte.Parse(t[14].ToString()), Byte.Parse(t[13].ToString()),
                                    Byte.Parse(t[12].ToString()), Byte.Parse(t[11].ToString()));


                                //Clean The String
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Step 2 Split the Polygons into Coordinates
                                String[] Coord = Shape.Split(',');

                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();

                                foreach (String Part in Coord)
                                {
                                    //Step 3 Split the Coordinate into its parts.
                                    String[] s3 = Part.Trim().Split(' ');
                                    if (s3.Count() == 3)
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            Double.Parse(s3[2])));
                                    }
                                    else
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            0));
                                    }
                                }


                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);

                                PlaceyThePlacemark.Name = t[1].ToString();
                                SharpKml.Dom.Description D = new SharpKml.Dom.Description();
                                D.Text = t[4].ToString();
                                PlaceyThePlacemark.Description = D.Clone();
                                PlaceyThePlacemark.Id = t[0].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }
                        }
                        catch (Exception ex)
                        {
                            //Swallow and loop
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                String x = text;
            }


            return Shapes;
        }

        public List<Placemark> ExtractPlacemarks(KmlFile MmmmKay)
        {
            List<Placemark> tt = new List<Placemark>();


            //Under Construction.
            //var mmmmmmmmm = MmmmKay.Root.Flatten().OfType<Style>();


            ////After the file read I add these to the container.
            foreach (Placemark p in MmmmKay.Root.Flatten().OfType<Placemark>())
            {
                //    Placemark temp = p.Clone();
                //    String text = temp.StyleUrl.ToString().Replace("#", String.Empty);
                //    //The weird world of lambdas!
                //    SharpKml.Dom.Style th = mmmmmmmmm.Where(s => s.Id == text).FirstOrDefault();
                //    if (th != null)
                //    {
                //        int x = 0;
                //    }
                //    //temp.AddStyle(th);

                tt.Add(p.Clone());
            }

            //Next I pull the styles out of the file and attach them to the invidual Placemarks.
            //Google Earth Pro does not do a good job with Styles makes a mess of references.
            //You will see this as the style Url. This functions pulls these styles out and
            //matches them with their needed styles. What matters is that you see a loop inside a loop
            //where we do all the cool things. cloning, string concatination, ToString(). Ideally I want to pull all the
            // cool functionality from the kml format. 
            //for (int i = 0; i < tt.Count; ++i)
            //{
            //    foreach (Style syt in mmmmmmmmm)
            //    {
            //        if (tt[i].StyleUrl.ToString() == "#" + syt.Id)
            //        {
            //            tt[i].AddStyle(syt.Clone());
            //        }
            //    }
            //}
            return tt;
        }

        /// Gets All the Shapes that are active in DB_Toril
        /// Date        Name                    Vers    Comments
        /// 2023-03-07  Clay                   1.0     Initial
        /// </summary>
        /// <param name="TheModel"></param>
        /// <returns></returns>
        public List<String> ExtractSQLGeographyStrings(List<Placemark> TheModel)
        {
            List<String> tt = new List<String>();

            foreach (Placemark Shape in TheModel)
            {
                try
                {
                    if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.Point))
                    {
                        SharpKml.Dom.Point temp = (SharpKml.Dom.Point)Shape.Geometry.Clone();
                        SharpKml.Base.Vector v = temp.Coordinate;
                        tt.Add($"POINT ({v.Longitude} {v.Latitude} {v.Altitude})");
                    }

                    else if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                    {
                        SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)Shape.Geometry.Clone();
                        Geotool_Objects.Polygon p = new Geotool_Objects.Polygon();


                        foreach (SharpKml.Base.Vector CO in temp.OuterBoundary.LinearRing.Coordinates)
                        {
                            p.LinearList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                        }

                        foreach (SharpKml.Dom.InnerBoundary IB in temp.InnerBoundary)
                        {
                            List<Coordinate> list = new List<Coordinate>();

                            foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                            {
                                list.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                            }

                            p.InnerBoundaryList.Add(list);
                        }

                        tt.Add(p.ToSQLGEOGRAPHY());
                    }

                    else if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.LineString))
                    {
                        SharpKml.Dom.LineString temp = (SharpKml.Dom.LineString)Shape.Geometry.Clone();
                        Geotool_Objects.Line L = new Geotool_Objects.Line();

                        foreach (SharpKml.Base.Vector CO in temp.Coordinates)
                        {
                            L.CoordinateList.Add(new Coordinate(CO.Latitude, CO.Longitude,
                                Convert.ToDouble(CO.Altitude)));
                        }

                        tt.Add(L.ToSQLGEOGRAPHY());
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return tt;
        }

        /// <summary>
        /// Inserts a valid list of Polygon, Point, LineString Placemarks into DB_Toril
        /// Date        Name                    Vers    Comments
        /// 2022-12-21  Shake Spear!            1.0     The initial It does not do style yet.
        /// </summary>
        /// <param name="TheModel"></param>
        /// <returns></returns>
        public bool InsertShapes(List<Placemark> TheModel)
        {
            System.Data.SqlClient.SqlDataReader t;


            //More control with a for loop
            //for(int i = 0; i < TheModel.Count;++i)
            //{
            //    SharpKml.Dom.StyleSelector S = (Style)TheModel[i].Styles;   //.Styles.FirstOrDefault();


            //}
            int TestNumber = 0;
            foreach (Placemark Shape in TheModel)
            {
                ++TestNumber;
                if (TestNumber >= 705)
                {
                    int y = 0;
                }

                SharpKml.Dom.Style S = (Style)Shape.Styles.FirstOrDefault();


                //You have to be styled to go into Project Atuin's Database. No blanks and
                //no nulls is what we want. We want all the colour, the scales, styling.
                //This is going to be a few steps. Just look how I test to see if if it is
                //fill it with a value.
                if (S != null)
                {
                    if (S.Label == null)
                    {
                        S.Label = new SharpKml.Dom.LabelStyle();
                        S.Label.Scale = 1;
                    }

                    if (S.Icon == null)
                    {
                        S.Icon = new IconStyle();
                        S.Icon.Icon =
                            new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/shapes/donut.png"));
                        S.Icon.Scale = 1;
                        S.Icon.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                    }

                    if (S.Line == null)
                    {
                        S.Line = new SharpKml.Dom.LineStyle();
                        S.Line.Width = 1;
                        S.Line.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                    }

                    if (S.Line.Width == null)
                    {
                        S.Line.Width = 1;
                    }

                    if (S.Polygon == null)
                    {
                        S.Polygon = new SharpKml.Dom.PolygonStyle();
                        S.Polygon.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                    }
                }

                if (S == null)
                {
                    S = new Style();

                    S.Label = new SharpKml.Dom.LabelStyle();
                    S.Label.Scale = 1;

                    S.Icon = new IconStyle();
                    S.Icon.Scale = 1;
                    S.Icon.Icon =
                        new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/shapes/donut.png"));
                    S.Icon.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                    S.Line = new SharpKml.Dom.LineStyle();
                    S.Line.Width = 1;
                    S.Line.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                    S.Polygon = new SharpKml.Dom.PolygonStyle();
                    S.Polygon.Color = new SharpKml.Base.Color32(255, 255, 255, 255);
                }

                if (Shape.Description == null)
                {
                    Shape.Description = new Description();
                    Shape.Description.Text = "";
                }

                if (Shape.Name == null)
                {
                    Shape.Name = "";
                }

                //If found this snippit in some SharpKml documents. I think this would make colouring things way easier.

                //var c = System.Drawing.ColorTranslator.FromHtml("#33FF33");
                //var postStyle = new Style();
                //postStyle.Id = "post_vanlig";
                //postStyle.Icon = new IconStyle
                //{
                //    Color = new Color32(c.A, c.R, c.G, c.B),
                //    Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/paddle/wht-blank.png")),
                //    Scale = 0.5
                //};

                try
                {
                    VerticalDatum verticalDatum = new VerticalDatum();

                    //Style and colour are hard coded at the moment.
                    //POINTS!!!!
                    if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.Point))
                    {
                        SharpKml.Dom.Point temp = (SharpKml.Dom.Point)Shape.Geometry.Clone();
                        SharpKml.Base.Vector v = temp.Coordinate;


                        //This is ugly. I am open to suggestions...
                        if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.ClampToGround)
                        {
                            verticalDatum = VerticalDatum.ClampToGround;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.Absolute)
                        {
                            verticalDatum = VerticalDatum.Absolute;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.RelativeToGround)
                        {
                            verticalDatum = VerticalDatum.RelativeToGround;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor)
                        {
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor)
                        {
                            verticalDatum = VerticalDatum.RelativeToSeafloor;
                        }
                        else
                        {
                            //Just in case....
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }

                        var x = PDM.Data.SqlHelper.ExecuteScalar(GetConnectionString(), "sp_CreateShape",
                            Shape.Name,
                            S.Label.Scale,
                            S.Icon.Scale,
                            Shape.Description.Text,
                            S.Icon.Icon.Href.ToString(),
                            $"POINT ({v.Longitude} {v.Latitude} {v.Altitude})",
                            S.Polygon.Color.GetValueOrDefault().Red,
                            S.Polygon.Color.GetValueOrDefault().Green,
                            S.Polygon.Color.GetValueOrDefault().Blue,
                            S.Polygon.Color.GetValueOrDefault().Alpha,
                            S.Line.Color.GetValueOrDefault().Red,
                            S.Line.Color.GetValueOrDefault().Green,
                            S.Line.Color.GetValueOrDefault().Blue,
                            S.Line.Color.GetValueOrDefault().Alpha,
                            S.Icon.Color.GetValueOrDefault().Red,
                            S.Icon.Color.GetValueOrDefault().Green,
                            S.Icon.Color.GetValueOrDefault().Blue,
                            255, //Set to 255 for Sanity Check S.Icon.Color.GetValueOrDefault().Alpha, 
                            S.Line.Width,
                            1,
                            temp.Extrude, //This is the extrude set to true hard coded. We have to fix this.
                            (int)verticalDatum //This is  vertical datum.
                        );
                    }
                    //POLYGONS
                    else if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                    {
                        SharpKml.Dom.Polygon temp = (SharpKml.Dom.Polygon)Shape.Geometry.Clone();
                        Geotool_Objects.Polygon p = new Geotool_Objects.Polygon();

                        //This is ugly. I am open to suggestions...
                        if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.ClampToGround)
                        {
                            verticalDatum = VerticalDatum.ClampToGround;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.Absolute)
                        {
                            verticalDatum = VerticalDatum.Absolute;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.RelativeToGround)
                        {
                            verticalDatum = VerticalDatum.RelativeToGround;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor)
                        {
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor)
                        {
                            verticalDatum = VerticalDatum.RelativeToSeafloor;
                        }
                        else
                        {
                            //Just in case....
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }


                        foreach (SharpKml.Base.Vector CO in temp.OuterBoundary.LinearRing.Coordinates)
                        {
                            p.LinearList.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                        }

                        foreach (SharpKml.Dom.InnerBoundary IB in temp.InnerBoundary)
                        {
                            List<Coordinate> list = new List<Coordinate>();

                            foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                            {
                                list.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                            }

                            p.InnerBoundaryList.Add(list);
                        }

                        //Style is hardcoded. I need to put this into sprint 2.
                        var x = PDM.Data.SqlHelper.ExecuteScalar(GetConnectionString(), "sp_CreateShape",
                            Shape.Name,
                            S.Label.Scale,
                            S.Icon.Scale,
                            Shape.Description.Text,
                            S.Icon.Icon.Href.ToString(),
                            p.ToSQLGEOGRAPHY(),
                            S.Polygon.Color.GetValueOrDefault().Red,
                            S.Polygon.Color.GetValueOrDefault().Green,
                            S.Polygon.Color.GetValueOrDefault().Blue,
                            S.Polygon.Color.GetValueOrDefault().Alpha,
                            S.Line.Color.GetValueOrDefault().Red,
                            S.Line.Color.GetValueOrDefault().Green,
                            S.Line.Color.GetValueOrDefault().Blue,
                            S.Line.Color.GetValueOrDefault().Alpha,
                            S.Icon.Color.GetValueOrDefault().Red,
                            S.Icon.Color.GetValueOrDefault().Green,
                            S.Icon.Color.GetValueOrDefault().Blue,
                            S.Icon.Color.GetValueOrDefault().Alpha,
                            S.Line.Width,
                            1,
                            temp.Extrude,
                            (int)verticalDatum
                        );
                    }

                    else if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.LineString))
                    {
                        SharpKml.Dom.LineString temp = (SharpKml.Dom.LineString)Shape.Geometry.Clone();
                        Geotool_Objects.Line L = new Geotool_Objects.Line();

                        //This is ugly. I am open to suggestions...
                        if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.ClampToGround)
                        {
                            verticalDatum = VerticalDatum.ClampToGround;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.Absolute)
                        {
                            verticalDatum = VerticalDatum.Absolute;
                        }
                        else if (temp.AltitudeMode == SharpKml.Dom.AltitudeMode.RelativeToGround)
                        {
                            verticalDatum = VerticalDatum.RelativeToGround;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor)
                        {
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }
                        else if (temp.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor)
                        {
                            verticalDatum = VerticalDatum.RelativeToSeafloor;
                        }
                        else
                        {
                            //Just in case....
                            verticalDatum = VerticalDatum.ClampToSeafloor;
                        }


                        foreach (SharpKml.Base.Vector CO in temp.Coordinates)
                        {
                            L.CoordinateList.Add(new Coordinate(CO.Latitude, CO.Longitude,
                                Convert.ToDouble(CO.Altitude)));
                        }

                        var x = PDM.Data.SqlHelper.ExecuteScalar(GetConnectionString(), "sp_CreateShape",
                            Shape.Name,
                            S.Label.Scale,
                            S.Icon.Scale,
                            Shape.Description.Text,
                            S.Icon.Icon.Href.ToString(),
                            L.ToSQLGEOGRAPHY(),
                            S.Polygon.Color.GetValueOrDefault().Red,
                            S.Polygon.Color.GetValueOrDefault().Green,
                            S.Polygon.Color.GetValueOrDefault().Blue,
                            S.Polygon.Color.GetValueOrDefault().Alpha,
                            S.Line.Color.GetValueOrDefault().Red,
                            S.Line.Color.GetValueOrDefault().Green,
                            S.Line.Color.GetValueOrDefault().Blue,
                            S.Line.Color.GetValueOrDefault().Alpha,
                            S.Icon.Color.GetValueOrDefault().Red,
                            S.Icon.Color.GetValueOrDefault().Green,
                            S.Icon.Color.GetValueOrDefault().Blue,
                            S.Icon.Color.GetValueOrDefault().Alpha,
                            S.Line.Width,
                            1,
                            temp.Extrude,
                            (int)verticalDatum
                        );
                    }

                    //We have to bail on this and use the Microsoft GeoSpatial.
                    else if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.MultipleGeometry))
                    {
                        throw new NotImplementedException("MultipleGeometry is not supported on INSERT");
                        ////SharpKml.Dom.MultipleGeometry temp = (SharpKml.Dom.MultipleGeometry)Shape.Geometry.Clone();
                        ////Geotool_Objects.MultiPolygon L = new Geotool_Objects.MultiPolygon();

                        ////I fear this may fail for multilinestrings
                        ////SharpKml.Dom.Polygon TempG = (SharpKml.Dom.Polygon)temp.Geometry.FirstOrDefault();

                        ////This is ugly.I am open to suggestions...
                        ////if (TempG.AltitudeMode == SharpKml.Dom.AltitudeMode.ClampToGround)
                        ////{
                        ////    verticalDatum = VerticalDatum.ClampToGround;
                        ////}
                        //else if (TempG.AltitudeMode == SharpKml.Dom.AltitudeMode.Absolute)
                        //{
                        //    verticalDatum = VerticalDatum.Absolute;
                        //}
                        //else if (TempG.AltitudeMode == SharpKml.Dom.AltitudeMode.RelativeToGround)
                        //{
                        //    verticalDatum = VerticalDatum.RelativeToGround;
                        //}
                        //else if (TempG.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.ClampToSeafloor)
                        //{
                        //    verticalDatum = VerticalDatum.ClampToSeafloor;
                        //}
                        //else if (TempG.GXAltitudeMode == SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor)
                        //{
                        //    verticalDatum = VerticalDatum.RelativeToSeafloor;
                        //}
                        //else
                        //{
                        //    //Just in case....
                        //    verticalDatum = VerticalDatum.ClampToSeafloor;
                        //}
                        //foreach(Geometry G in temp.Geometry)
                        //{
                        //    if(Shape.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                        //    {

                        //    }
                        //    if (Shape.Geometry.GetType() == typeof(SharpKml.Dom.Polygon))
                        //    {

                        //    }

                        //        int x = 0;
                        //}

                        //foreach (SharpKml.Dom.InnerBoundary IB in temp.InnerBoundary)
                        //{
                        //    List<Coordinate> list = new List<Coordinate>();

                        //    foreach (SharpKml.Base.Vector CO in IB.LinearRing.Coordinates)
                        //    {
                        //        list.Add(new Coordinate(CO.Latitude, CO.Longitude, Convert.ToDouble(CO.Altitude)));
                        //    }
                        //    p.InnerBoundaryList.Add(list);
                        //}


                        //     var x = PDM.Data.SqlHelper.ExecuteScalar(GetConnectionString(), "sp_CreateShape",
                        //Shape.Name,
                        //S.Label.Scale,
                        // S.Icon.Scale,
                        //Shape.Description.Text,
                        // S.Icon.Icon.Href.ToString(),
                        //L.ToSQLGEOGRAPHY(),
                        // S.Polygon.Color.GetValueOrDefault().Red,
                        // S.Polygon.Color.GetValueOrDefault().Green,
                        // S.Polygon.Color.GetValueOrDefault().Blue,
                        // S.Polygon.Color.GetValueOrDefault().Alpha,
                        // S.Line.Color.GetValueOrDefault().Red,
                        // S.Line.Color.GetValueOrDefault().Green,
                        // S.Line.Color.GetValueOrDefault().Blue,
                        // S.Line.Color.GetValueOrDefault().Alpha,
                        // S.Icon.Color.GetValueOrDefault().Red,
                        // S.Icon.Color.GetValueOrDefault().Green,
                        // S.Icon.Color.GetValueOrDefault().Blue,
                        // S.Icon.Color.GetValueOrDefault().Alpha,
                        // S.Line.Width,
                        // 1,
                        // temp.Extrude,
                        // (int)verticalDatum
                        // );
                    }
                }


                catch (Exception ex)
                {
                    Console.WriteLine($"Line Number: {TestNumber} " + ex.Message);
                }
            }

            return true;
        }

        /// <summary>
        /// Does not actually insert a letter. REFACTOR THIS TO SOLVEFORLETTER
        /// Date        Name                    Vers    Comments
        /// 2024-12-22  Clay                    1.0     Finally! We have enough to form student work to form a useful call!
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="UVL"></param>
        /// <param name="Lat"></param>
        /// <param name="Long"></param>
        /// <param name="BufferScaler"></param>
        /// <returns></returns>
        public Placemark InsertLetter(Char letter, Decimal UVL, Double Lat, Double Long, Double BufferScaler)
        {
            List<Placemark> Shapes = new List<Placemark>();

            System.Data.SqlClient.SqlDataReader t;


            SharpKml.Dom.Style s = new SharpKml.Dom.Style();
            String text = "";
            int iD = 0;
            try
            {
                t = PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_GetLetter",
                    letter,
                    UVL,
                    Lat,
                    Long,
                    BufferScaler);

                if (t.HasRows)
                {
                    while (t.Read())
                    {
                        try
                        {
                            //Shake Spear! Vertical Datum.
                            bool ExtRuuuuude = false;

                            //Now I have an enum that holds a whole bunch of vertical datums. Two of which have different libraries!
                            //Enums make things easier. See below where I use them.
                            VerticalDatum VDatum = VerticalDatum.RelativeToSeafloor;


                            String Shape = t[0].ToString();


                            //We have the data from the database. Now we start by building Points
                            //Fix any nonesense 
                            Shape = Shape.Replace("), (", "),(");
                            Shape = Shape.Replace(") , (", "),(");
                            Shape = Shape.Replace(") ,(", "),(");

                            ///Make brand new copy each time we loop.
                            s = new SharpKml.Dom.Style();


                            if (Shape.Contains("MULTIPOLYGON") &&
                                (Shape.Contains(")),((") || (Shape.Contains(")), (("))))
                            {
                                Geotool_Objects.MultiPolygon Geometry = new Geotool_Objects.MultiPolygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(255, 255, 255, 255);
                                s.Line = new LineStyle();
                                s.Line.Width = 1;
                                s.Line.Color = new Color32(255, 255, 255, 255);

                                //Clean The String
                                Shape = Shape.Replace(")), ((", ")),((");
                                Shape = Shape.Replace("MULTIPOLYGON (((", "");
                                Shape = Shape.Replace(")))", "");

                                //Since this is a multi we need to get all the polygons first.
                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { ")),((" };
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                //So now I have an Array of strings each element a seperate polygon string it may have holes....
                                foreach (String PolyString in b)
                                {
                                    Geotool_Objects.Polygon Pol = new Geotool_Objects.Polygon();
                                    //Splitting a string using a string array.
                                    String[] DelimeterInner = new string[] { "),(" };

                                    //Step 1. Split the multis up into polygons
                                    String[] bInner = PolyString.Split(DelimeterInner, StringSplitOptions.None);


                                    for (int i = 0; i < bInner.Length; ++i)
                                    {
                                        bInner[i] = bInner[i].Replace("(", "");
                                        bInner[i] = bInner[i].Replace(")", "");

                                        //This should be the outer because it is firt I think
                                        if (i == 0)
                                        {
                                            //Step 2. Split the Coordinates into Polygons
                                            String[] Coord = bInner[i].Split(',');

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    Pol.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                        Double.Parse(s3[0]), 0));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Step 2 Split the Polygons into Coordinates
                                            String[] Coord = bInner[i].Split(',');
                                            List<Coordinate> temp = new List<Coordinate>();

                                            foreach (String Part in Coord)
                                            {
                                                //Step 3 Split the Coordinate into its parts.
                                                String[] s3 = Part.Trim().Split(' ');
                                                if (s3.Count() == 3)
                                                {
                                                    temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                                        Double.Parse(s3[2])));
                                                }
                                                else
                                                {
                                                    temp.Add(
                                                        new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]), 0));
                                                }
                                            }

                                            Pol.InnerBoundaryList.Add(temp);
                                        }
                                    }

                                    Geometry.PolyGonList.Add(Pol);
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlacemark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();

                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }
                            //This is a Polygon with holes!
                            else if (Shape.Contains("POLYGON") && (Shape.Contains("),(") || (Shape.Contains("), ("))))
                            {
                                Shape = Shape.Replace("), (", "),(");
                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(255, 255, 255, 255);
                                s.Line = new LineStyle();
                                s.Line.Width = 1;
                                s.Line.Color = new Color32(255, 255, 255, 255);


                                //Clean The String
                                Shape = Shape.Replace("), (", "),(");
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Splitting a string using a string array.
                                String[] Delimeter = new string[] { "),(" };

                                //Step 1. Split the multis up into polygons
                                String[] b = Shape.Split(Delimeter, StringSplitOptions.None);

                                for (int i = 0; i < b.Length; ++i)
                                {
                                    if (i == 0)
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]),
                                                    Double.Parse(s3[0]), 0));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Step 2 Split the Polygons into Coordinates
                                        String[] Coord = b[i].Split(',');
                                        List<Coordinate> temp = new List<Coordinate>();

                                        foreach (String Part in Coord)
                                        {
                                            //Step 3 Split the Coordinate into its parts.
                                            String[] s3 = Part.Trim().Split(' ');
                                            if (s3.Count() == 3)
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                                    Double.Parse(s3[2])));
                                            }
                                            else
                                            {
                                                temp.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]), 0));
                                            }
                                        }

                                        Geometry.InnerBoundaryList.Add(temp);
                                    }
                                }

                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);
                                PlaceyThePlacemark.Name = t[1].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }
                            //This is a regular with no holes
                            else if (Shape.Contains("POLYGON") && Shape.Contains("),(") != true)
                            {
                                s.Polygon = new PolygonStyle();
                                s.Polygon.Fill = true;
                                s.Polygon.Color = new Color32(255, 255, 255, 255);
                                s.Line = new LineStyle();
                                s.Line.Width = 1;
                                s.Line.Color = new Color32(255, 255, 255, 255);


                                //Clean The String
                                Shape = Shape.Replace("POLYGON ((", "");
                                Shape = Shape.Replace("))", "");

                                //Step 2 Split the Polygons into Coordinates
                                String[] Coord = Shape.Split(',');

                                Geotool_Objects.Polygon Geometry = new Geotool_Objects.Polygon();

                                foreach (String Part in Coord)
                                {
                                    //Step 3 Split the Coordinate into its parts.
                                    String[] s3 = Part.Trim().Split(' ');
                                    if (s3.Count() == 3)
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            Double.Parse(s3[2])));
                                    }
                                    else
                                    {
                                        Geometry.LinearList.Add(new Coordinate(Double.Parse(s3[1]), Double.Parse(s3[0]),
                                            0));
                                    }
                                }


                                Placemark PlaceyThePlacemark = Geometry.ToPlaceMark(ExtRuuuuude, VDatum);

                                PlaceyThePlacemark.Name = t[1].ToString();
                                PlaceyThePlacemark.AddStyle(s.Clone());
                                Shapes.Add(PlaceyThePlacemark.Clone());
                            }
                        }
                        catch (Exception ex)
                        {
                            //Swallow and loop
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                String x = text;
            }

            return Shapes.FirstOrDefault();
        }

        /// <summary>
        /// Writes a KML file based on a list of Placemarks
        /// Date        Name                    Vers    Comments
        /// 2022-09-01  Shake Spear!            1.0     Initial
        /// <param name="TheModel"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool WriteKmlFile(List<Placemark> TheModel, String FileName)
        {
            var Document = new Document();

            Document.Id = $"Project Atuin Base Build: {Environment.UserName}:{DateTime.Now}";
            Document.Name = $"{DateTime.Now}";

            Description description = new Description();
            description.Text = @"<h1>Project Chult Test File</h1>";

            //Super Easy foreach through a polygon list that adds all the polygons to the list.
            //Weird thing is placemarks holds polygons, lines, multigeometries, markers, heck
            //any GIS thingabob we need. Arent Lists supposed to be Homogeneous?
            //Well they are...but they can take List<Object> and everything is an Object. The
            //Object is the universal concept of a thing. Everything is an Object!
            foreach (Placemark placemark in TheModel)
            {
                Document.AddFeature(placemark.Clone());
            }

            var kml = new Kml();
            kml.Feature = Document;


            KmlFile kmlfile = KmlFile.Create(kml, true);

            //This using is worth knowing. after this function is finished
            //the System.IO.File is thrown out. Saves memory.
            //we moved from OpenWrite that kinda apends data to the end and Create that Overwrites the file.
            //We were getting writing errors so we changed the function to overwrite the file name. 
            //This function is super cool. This save function makes the KML based on whatever is in the
            //placemarks container.
            using (var stream = System.IO.File.Create(FileName))
            {
                kmlfile.Save(stream);
            }

            if (TheModel.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Writes a KML file based on a list of Placemarks
        /// Date        Name                    Vers    Comments
        /// 2022-09-01  Shake Spear!            1.0     Initial
        public KmlFile ReadKmlFile(String FileName)
        {
            try
            {
                String KMLString = "";
                KMLString = File.ReadAllText(FileName);

                //What is this Arcane Majic?! It takes the String the "Letters" of the string and converts it into  a memory stream and 
                //inside the using we build a binary representation of the KML file. WHY?! Because we can use the power of KML to do Geography for us.
                //What is "using" it allows us to "dispose" of memory eating objects like Memory Stream. KML files can get waaaay big.
                using (var stream = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(KMLString)))
                {
                    //Its that simple. We have a model. 
                    KmlFile TheKMLModel = KmlFile.Load(stream);

                    return TheKMLModel;
                }
            }
            catch (Exception ex)
            {
                var Document = new Document();

                Document.Id = $"Project Atuin Base Build: {Environment.UserName}:{DateTime.Now}";
                Document.Name = $"{DateTime.Now}";

                Description description = new Description();
                description.Text = @"<h1>Sky Pirate Test File</h1>";
                var kml = new Kml();
                kml.Feature = Document;


                return KmlFile.Create(kml, true);
            }
        }

        /// <summary>
        /// Writes a KML file based on a list of Placemarks
        /// Date        Name                    Vers    Comments
        /// 2023-11-28  Clay                    1.0     Initial
        public KmlFile ReadKmzFile(String FileName)
        {
            try
            {
                byte[] KMLString = File.ReadAllBytes(FileName);

                int x = 0;

                //What is this Arcane Majic?! It takes the String the "Letters" of the string and converts it into  a memory stream and 
                //inside the using we build a binary representation of the KML file. WHY?! Because we can use the power of KML to do Geography for us.
                //What is "using" it allows us to "dispose" of memory eating objects like Memory Stream. KML files can get waaaay big.
                using (var stream = new MemoryStream(KMLString))
                {
                    //Its that simple. We have a model.
                    KmzFile kmz = KmzFile.Open(stream);
                    KmlFile TheKMLModel = kmz.GetDefaultKmlFile();


                    return TheKMLModel;
                }
            }
            catch (Exception ex)
            {
                var Document = new Document();

                Document.Id = $"Project Atuin Base Build: {Environment.UserName}:{DateTime.Now}";
                Document.Name = $"{DateTime.Now}";

                Description description = new Description();
                description.Text = @"<h1>Sky Pirate Test File</h1>";
                var kml = new Kml();
                kml.Feature = Document;


                return KmlFile.Create(kml, true);
            }
        }

        /// <summary>
        /// Makes a KMZ file based on a valid path to a valid kml
        /// Date        Name                    Vers    Comments
        /// 2022-09-01  Shake Spear!            1.0     Initial 
        /// 2022-11-28  Clay                    1.1     Deprecated make Private Do not use.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public KmzFile MakeKmzFile(String FileName)
        {
            KmlFile TheKMLModel;
            string basePath = Path.GetDirectoryName(FileName);


            String KMLString = "";
            KMLString = File.ReadAllText(FileName);


            //What is this Arcane Majic?! It takes the String the "Letters" of the string and converts it into  a memory stream and 
            //inside the using we build a binary representation of the KML file. WHY?! Because we can use the power of KML to do Geography for us.
            //What is "using" it allows us to "dispose" of memory eating objects like Memory Stream. KML files can get waaaay big.
            using (var stream = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(KMLString)))
            {
                //Its that simple. We have a model. 
                TheKMLModel = KmlFile.Load(stream);
            }

            LinkResolver link = new LinkResolver(TheKMLModel);


            SharpKml.Engine.KmzFile kmzFile = KmzFile.Create(TheKMLModel);

            foreach (var l in link.GetRelativePaths())
            {
                Uri uri = new Uri(Path.Combine(basePath, l), UriKind.RelativeOrAbsolute);
                String t = uri.OriginalString;
                byte[] file = File.ReadAllBytes(t);
                kmzFile.AddFile(l, file);
            }

            //This will have to be looped eventually as well. Sprint 6 maybe.
            Uri texture = new Uri(Path.Combine(basePath, "Callisto.jpg"), UriKind.RelativeOrAbsolute);
            String textureString = texture.OriginalString;
            byte[] texturefile = File.ReadAllBytes(textureString);
            kmzFile.AddFile("Callisto.jpg", texturefile);

            texture = new Uri(Path.Combine(basePath, "barque.jpg"), UriKind.RelativeOrAbsolute);
            textureString = texture.OriginalString;
            texturefile = File.ReadAllBytes(textureString);
            kmzFile.AddFile("barque.jpg", texturefile);


            return kmzFile;
        }

        public bool WriteKmzFile(KmzFile kmzfile, String FileName)
        {
            //This using is worth knowing. after this function is finished
            //the System.IO.File is thrown out. Saves memory.
            //we moved from OpenWrite that kinda apends data to the end and Create that Overwrites the file.
            //We were getting writing errors so we changed the function to overwrite the file name. 
            //This function is super cool. This save function makes the KML based on whatever is in the
            //placemarks container.
            using (var stream = System.IO.File.Create(FileName))
            {
                kmzfile.Save(stream);
            }

            return true;
        }

        public bool AddCoordinate(double[] data)
        {
            return true;
            throw new NotImplementedException();
        }

        public class AmenityRow
        {
            public int AmenityID { get; set; }
            public string Name { get; set; } = "";
            public int CategoryID { get; set; }
            public string CategoryName { get; set; } = "";
            public string Street { get; set; } = "";
            public string City { get; set; } = "";
            public int SubdivisionID { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string GeometryType { get; set; } = "";
            public string LocationWKT { get; set; } = "";
        }

        public List<AmenityRow> Amenity_ReadAll()
        {
            var list = new List<AmenityRow>();

            System.Data.SqlClient.SqlDataReader r =
                PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Amenity_Read");

            while (r.Read())
            {
                list.Add(MapAmenityRow(r));
            }

            return list;
        }


        private AmenityRow MapAmenityRow(System.Data.SqlClient.SqlDataReader r)
        {

            var row = new AmenityRow
            {
                AmenityID = Convert.ToInt32(r["AmenityID"]),
                Name = r["Name"]?.ToString() ?? "",
                CategoryID = Convert.ToInt32(r["CategoryID"]),
                CategoryName = r["CategoryName"]?.ToString() ?? "",
                Street = r["Street"]?.ToString() ?? "",
                City = r["City"]?.ToString() ?? "",
                SubdivisionID = Convert.ToInt32(r["SubdivisionID"]),
                GeometryType = r["GeometryType"]?.ToString() ?? "",
                LocationWKT = r["LocationWKT"]?.ToString() ?? ""
            };

            // Nullable decimals
            row.Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]);
            row.Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"]);

            return row;
        }

        public int Amenity_Create(
            string name,
            int categoryId,
            string street,
            string city,
            int subdivisionId,
            decimal? latitude = null,
            decimal? longitude = null,
            string locationWkt = null
        )
        {
            object scalar = PDM.Data.SqlHelper.ExecuteScalar(
                GetConnectionString(),
                "sp_Amenity_Create",
                name,
                categoryId,
                street,
                city,
                subdivisionId,
                DbOrNull(latitude),
                DbOrNull(longitude),
                DbOrNull(locationWkt)
            );

            return Convert.ToInt32(scalar);
        }

        public AmenityRow Amenity_ReadById(int amenityId)
        {
            System.Data.SqlClient.SqlDataReader r =
                PDM.Data.SqlHelper.ExecuteReader(GetConnectionString(), "sp_Amenity_Read", amenityId);

            if (!r.Read()) return null;

            return MapAmenityRow(r);
        }

        public AmenityRow Amenity_Update(
            int amenityId,
            string name = null,
            int? categoryId = null,
            string street = null,
            string city = null,
            int? subdivisionId = null,
            decimal? latitude = null,
            decimal? longitude = null,
            string locationWkt = null
        )
        {

            PDM.Data.SqlHelper.ExecuteNonQuery(
                GetConnectionString(),
                "sp_Amenity_Update",
                amenityId,
                DbOrNull(name),
                DbOrNull(categoryId),
                DbOrNull(street),
                DbOrNull(city),
                DbOrNull(subdivisionId),
                DbOrNull(latitude),
                DbOrNull(longitude),
                DbOrNull(locationWkt)
            );

            return Amenity_ReadById(amenityId);
        }

        private static object DbOrNull(object v) => v ?? DBNull.Value;


        public bool Amenity_Delete(int amenityId)
        {
            try
            {
                PDM.Data.SqlHelper.ExecuteNonQuery(GetConnectionString(), "sp_Amenity_Delete", amenityId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}