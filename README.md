#  Building
## Step 1) Database
- Unzip the the folder `Project_AmeniScale.zip`. Navigate to the folder `Project_AmeniScale\Sprint 1\SQL\`. Inside the folder should be 4 files:AmeniScale_DDl.sql, AmeniScale_DML.sql, CRUD.sq1, and CRUD_Test.sql
- From within SQL Server Management Studio 2X, open the following scripts and run them in this order:
    - AmeniScale_DDl.sql
    - AmeniScale_DML.sql
    - CRUD.sq1
 
  *Note:* `CRUD_Test.sql` is just a file for testing the SP inside CRUD.sql only. 

## Step 2) Visual Studio
- Navigate to `Project_AmeniScale\Sprint 1\AmeniScale\AmenityScale`. Inside you should see a `.sln` folder. Open the solution. 
- Only 2 Projects of the 4 in this solution are relavent to this Capstone: `AmenityScale` and `GeoTools_Objects`.
- Expand the Node `AmenityScale` and locate the file `App.config`. Change the Connection string to match your local database where `DB_AmeniScale` is located.
- From the toolbar click, Build -> Clean Solution. Again from Build -> Rebuild Solution.
- Set `AmenityScale` as your default project if it  isn't already.
- Now run the application.
 **Note: IGNORE**: `DataAccess` and `Crud_Tester_5000`


  # AmenityScaleWPF
  Inside the WPF you have access to the basic CRUD For the Table Location and Amenties. You should be able to execute all stored procedures for these tables. 
