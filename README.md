# EnovaLicenceRemover
The application allows you to delete information about the license in the indicated enova database.

![image](https://user-images.githubusercontent.com/19372942/154313433-b892a3c4-492a-447a-a1fe-9e80e19e4ef9.png)

## How to use:
1. In the Instance field, enter the address of the server on which the database is located. You can load a list of local instances by clicking the Load button
2. Complete the database access parameters (Is Windows authorization, PersistSecurityInfo required, or SQL user login details)
3. After entering the access data, click Refresh to load the list of databases
4. Select the database from which you want to remove the license. After selecting the database, the current license will be completed in the License field
5. Click the Remove button to remove the license information

Additional info:

- The enova program must be restarted for the change to be visible
- In the additional *.config* file, you can enter the default access data that will be loaded when the program is started
