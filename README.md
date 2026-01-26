# Amenity Score App
I think I got all the referncces and issues that may come up when pulling the project and possibly building.

Since the WPF side isn't done, to test the you can use the command-line application crud-tester-5000. Esnure that when you open the solution file (.sln) you change the build from Amenity Scale to Crud tester (Drop down located next to the Start button in Visual Studio 2022).

If you need to build for release for some reason, Change "Debug" to "release" in the drop down (Next to Any Cpu)

##  Building
### Inside Visual Studio
One that that will have to be changed like all of Clays projects is your Database connection. Go into the `DataTool.cs` find the string inside `private static String GetConnectionString()` and change the Server to match your SQL conenction. That's all you have to change.
You should now be able to just either Debug OR  buld it. If you build the release, remember the exe is in `Crud_Tester_5000\bin\debug OR release folders`

### Inside SQL Server Management Studio 21
There are Three files you need to run that you can find on this Repo: https://github.com/DavidWHGreeley/AmeniScaleSQL

In SSMS run the DDL, DML and CRUD.sql


