# Game Store API

## Starting SQL Server
```powershell
$saPassword="[SA PASSWORD HERE]"
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$saPassword" -p 1433:1433 -v sqlvolume:/var/opt/mssql -d --rm --name mssql mcr.microsoft.com/mssql/server:2022-latest
```

## Setting the connection string to secret manager
```powershell
dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:GameStoreContext" "Server=localhost; Database=GameStore; User Id=sa; Password=$saPassword; TrustServerCertificate=True"

dotnet user-secrets list
```

# Creating JWT token by commands
```powershell
dotnet user-jwts create --role "Admin" --scope "games:read"
dotnet user-jwts print "Id"
```

# Setting the Azure Storage Connection string to secret manager
```powershell
$storage_connstring="[STORAGE CONN STRING HERE]"
dotnet user-secrets set "ConnectionStrings:AzureStorage" $storage_connstring
```