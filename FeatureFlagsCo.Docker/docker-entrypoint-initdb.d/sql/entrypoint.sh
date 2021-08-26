#!/bin/bash
database=FeatureFlagsCo
wait_time=5s
password=YourSTRONG@Passw0rd

echo $database
# wait for SQL Server to come up
echo importing data will start in $wait_time...
sleep $wait_time
echo importing data...
sleep $wait_time
echo importing data...
sleep $wait_time
echo importing data...
sleep $wait_time
echo importing data...
sleep $wait_time
echo importing data...

# run the init script to create the DB and the tables in /table
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i ./init.sql

/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/Accounts.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AccountUserMappings.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetRoles.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetRoleClaims.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetUsers.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetUserRoles.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetUserClaims.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetUserLogins.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/AspNetUserTokens.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/Environments.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/Projects.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/ProjectUserMappings.sql
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i table/UserInvitations.sql

# for entry in "table"/*.sql
# do
#   # echo executing $entry
#   echo $entry
#   /opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i $entry
# done