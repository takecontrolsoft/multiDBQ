<img src="https://raw.githubusercontent.com/takecontrolsoft/multiDBQ/refs/heads/main/multiDBQ.ico" alt="MultiDBQ by Take Control - software & infrastructure" width="5%">

# MultiDBQ
Executing a same query on multiple databases with same structure.

## Screen

<img src="https://raw.githubusercontent.com/takecontrolsoft/multiDBQ/refs/heads/main/Screen.png" alt="MultiDBQ by Take Control - software & infrastructure" width="60%">

## Usage

### Connect to SQL server
* Enter your SQL server name or IP address. 
* Enter sql login and password. Make sure the sql login has rights to execute queries on MASTER database.
* Click button `Load`
* All available databases will be listed in the table below.

### Execute query from a file
* Select a query from a file using `Select` button.
* Check and edit the query if needed.
* Run the query upon all the listed databases on the right side with button `Execute`. The result will appear in the table only for databases that match the schema and have records matching the query.
* You can save the query from the redactor to the same file with button `Save`.

### Write a new query
* Write the query in the redactor.
* Run the query upon all the listed databases on the right side with button `Execute`. The result will appear in the table only for databases that match the schema and have records matching the query.
* You can save the query using the button `Save` to a new file chosen from the save file dialog.

## Compatibility
Windows Platforms, architectures: x86 and x64, arm64
MS SQL server


