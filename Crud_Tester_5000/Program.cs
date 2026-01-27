/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2015-26-01  Greeley                 Early morning email wanting command line crud. This is that.
/// 


using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crud_Tester_5000
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dt = new DataTool();

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== AmeniScale Console CRUD Tester ===");
                Console.WriteLine("1) Read All Amenities");
                Console.WriteLine("2) Read Amenity By ID");
                Console.WriteLine("3) Create Amenity");
                Console.WriteLine("4) Update Amenity");
                Console.WriteLine("5) Delete Amenity");
                Console.WriteLine("0) Exit");
                Console.Write("Select: ");

                var choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            ReadAll(dt);
                            break;

                        case "2":
                            ReadById(dt);
                            break;

                        case "3":
                            Create(dt);
                            break;

                        case "4":
                            Update(dt);
                            break;

                        case "5":
                            Delete(dt);
                            break;

                        case "0":
                            return;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("!!! ERROR !!!");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine(ex);
                }
            }
        }

        private static void ReadAll(DataTool dt)
        {
            var rows = dt.Amenity_ReadAll();

            Console.WriteLine();
            Console.WriteLine($"Found {rows.Count} amenities:");
            foreach (var a in rows)
            {
                PrintAmenity(a);
            }
        }

        private static void ReadById(DataTool dt)
        {
            int id = AskInt("AmenityID: ");

            var a = dt.Amenity_ReadById(id);
            Console.WriteLine();

            if (a == null)
            {
                Console.WriteLine("Not found.");
                return;
            }

            PrintAmenity(a);
        }

        private static void Create(DataTool dt)
        {
            Console.WriteLine();
            string name = AskString("Name: ");
            int categoryId = AskInt("CategoryID: ");
            string street = AskString("Street: ");
            string city = AskString("City: ");
            int subdivisionId = AskInt("SubdivisionID: ");

            Console.Write("Use (1) Lat/Lng or (2) WKT? ");
            var mode = Console.ReadLine()?.Trim();

            int newId;

            if (mode == "2")
            {
                string wkt = AskString("LocationWKT (ex: POINT(-81.2497 42.9834)): ");
                newId = dt.Amenity_Create(name, categoryId, street, city, subdivisionId, null, null, wkt);
            }
            else
            {
                decimal lat = AskDecimal("Latitude: ");
                decimal lng = AskDecimal("Longitude: ");
                newId = dt.Amenity_Create(name, categoryId, street, city, subdivisionId, lat, lng, null);
            }

            Console.WriteLine();
            Console.WriteLine($"Created AmenityID = {newId}");

            var created = dt.Amenity_ReadById(newId);
            if (created != null) PrintAmenity(created);
        }

        private static void Update(DataTool dt)
        {
            Console.WriteLine();
            int id = AskInt("AmenityID to update: ");

            var existing = dt.Amenity_ReadById(id);
            if (existing == null)
            {
                Console.WriteLine("Not found.");
                return;
            }

            Console.WriteLine("Leave blank to keep existing value.");
            Console.WriteLine();

            string name = AskNullableString($"Name ({existing.Name}): ");
            string street = AskNullableString($"Street ({existing.Street}): ");
            string city = AskNullableString($"City ({existing.City}): ");

            int? categoryId = AskNullableInt($"CategoryID ({existing.CategoryID}): ");
            int? subdivisionId = AskNullableInt($"SubdivisionID ({existing.SubdivisionID}): ");

            Console.Write("Update location? (y/N): ");
            var loc = Console.ReadLine()?.Trim().ToLowerInvariant();

            decimal? lat = null;
            decimal? lng = null;
            string wkt = null;

            if (loc == "y" || loc == "yes")
            {
                Console.Write("Use (1) Lat/Lng or (2) WKT? ");
                var mode = Console.ReadLine()?.Trim();

                if (mode == "2")
                {
                    wkt = AskString("LocationWKT (ex: POINT(-81.2497 42.9834)): ");
                }
                else
                {
                    lat = AskDecimal("Latitude: ");
                    lng = AskDecimal("Longitude: ");
                }
            }

            var updated = dt.Amenity_Update(
                id,
                name: name,
                categoryId: categoryId,
                street: street,
                city: city,
                subdivisionId: subdivisionId,
                latitude: lat,
                longitude: lng,
                locationWkt: wkt
            );

            Console.WriteLine();
            if (updated == null) Console.WriteLine("Update done, but re-read failed (unexpected).");
            else
            {
                Console.WriteLine("Updated:");
                PrintAmenity(updated);
            }
        }

        private static void Delete(DataTool dt)
        {
            Console.WriteLine();
            int id = AskInt("AmenityID to delete: ");

            Console.Write("Are you sure? (y/N): ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            bool ok = dt.Amenity_Delete(id);
            Console.WriteLine(ok ? "Deleted." : "Not found (or delete failed).");
        }

        private static void PrintAmenity(DataTool.AmenityRow a)
        {
            Console.WriteLine($"[{a.AmenityID}] {a.Name}");
            Console.WriteLine($"  Category: {a.CategoryID} - {a.CategoryName}");
            Console.WriteLine($"  Address : {a.Street}, {a.City} (SubdivisionID {a.SubdivisionID})");
            Console.WriteLine($"  Lat/Lng : {(a.Latitude?.ToString() ?? "NULL")}, {(a.Longitude?.ToString() ?? "NULL")}");
            Console.WriteLine($"  Geom    : {a.GeometryType}");
            Console.WriteLine($"  WKT     : {a.LocationWKT}");
            Console.WriteLine();
        }

        private static string AskString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("Required.");
            }
        }

        private static string AskNullableString(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Trim();
        }

        private static int AskInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v)) return v;
                Console.WriteLine("Enter a valid integer.");
            }
        }

        private static int? AskNullableInt(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, out int v)) return v;
            Console.WriteLine("Invalid. Leaving unchanged.");
            return null;
        }

        private static decimal AskDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal v)) return v;
                Console.WriteLine("Enter a valid decimal.");
            }
        }
    }
}