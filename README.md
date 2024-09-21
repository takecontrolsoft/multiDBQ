<img src="https://raw.githubusercontent.com/takecontrolsoft/multiDBQ/refs/heads/main/multiDBQ.ico" alt="MultiDBQ by Take Control - software & infrastructure" width="5%">

# MultiDBQ
Executing a same query on multiple databases with same structure.

## Benefits
* Comparing data available in different databases with the same schema. You can compare your Dev and Production environment databases.
* Sending the same data to all the databases with the same schema. For example if you need to change some setting to a domain name, or file path, ip address or something else in all the databases, you can do it with one simple UPDATE query on all the databases that matches the schema.
* SELECT, INSERT or UPDATE data to all units when working with multi databases softwares, which separate each unit to a new generated database.
* Viewing the database name next to specific data, client name, or address, in order to find the correct database name you need. This is useful in case you want to take this database Offline or to Restore it.

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
* Windows Platforms, architectures: x86 and x64, arm64
* MS SQL server


