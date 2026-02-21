# AmeniScale – Turnkey Build Instructions

This guide outlines the minimal steps required to set up and run the AmeniScale Capstone project.

---

## Step 1 – Database Setup (SQL Server)

1. Unzip `Project_AmeniScale.zip`.

2. Navigate to: `Project_AmeniScale\Sprint 2\SQL\`.  Open **SQL Server Management Studio**. 

5. Locate and Run the scripts in this exact order:
  1. `AmeniScale_DDL.sql`
  2. `AmeniScale_DML.sql`
  3. `CRUD.sql`

After completion, the database `DB_AmeniScale` should be fully configured.

---

## Step 2 – Visual Studio Setup

1. Navigate to:`Project_AmeniScale\Sprint 2\AmeniScale\`
2. Open the `.sln` solution file. Find the project `AmenityScaleWeb` in Visual Studio Solution Explorer.
4. Expand the project and open `Web.Config`. Update the connection string to point to your local SQL Server instance containing:
5. Open the file `Controllers\BaseController.cs`. Edit the string `"Your_Key_Here"` with your Google maps API key.
6. Set `AmenityScaleWeb` as the **Startup Project** (if not already).

**Always do:**:
- `Build -> Clean Solution`
- `Build -> Rebuild Solution`


## Team Memmbers:
If your on Windows, you don't need to constantly paste in the API key. You can set it as an environment variable. Here's how:
1. Start menu 
2. type in: View advance system system 
3. Advance tab
4. At the Bottom you'll see Environment Variable button, click it
5. In the System Variables, click the New button. Give it a name of "GOOGLE_MAPS_API_KEY". For the value, use your Key from google.
Doing this mean you shouldn't have to Copy paste in your API Key every time. 