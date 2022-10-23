RESOURCE_GROUP=rg-dotnet-passwordless
POSTGRESQL_HOST=postgres-passwordless
DATABASE_NAME=checklist
POSTGRESQL_FQDN=${POSTGRESQL_HOST}.postgres.database.azure.com
LOCATION=eastus

MYSQL_HOST=mysql-passwordless
MYSQL_FQDN=${MYSQL_HOST}.mysql.database.azure.com

APPSERVICE_PSQL=dotnet-passwordless-pgsql
APPSERVICE_MYSQL=dotnet-passwordless-mysql
APPSERVICE_PLAN="asp-dotnetpasswordless"

# Create a resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
# Create app service plan
az appservice plan create --name $APPSERVICE_PLAN --resource-group $RESOURCE_GROUP --location $LOCATION --sku B1 --is-linux

az webapp create \
    --name $APPSERVICE_PSQL \
    --resource-group $RESOURCE_GROUP \
    --plan $APPSERVICE_PLAN \
    --runtime "DOTNETCORE:6.0"

az webapp create \
    --name $APPSERVICE_MYSQL \
    --resource-group $RESOURCE_GROUP \
    --plan $APPSERVICE_PLAN \
    --runtime "DOTNETCORE:6.0"

cd Passwordless.WebAPI.PgSql

dotnet build

# Get current user
USER=$(az account show --query user.name -o tsv)

# Get connections string
CONNSTRING="Server=${POSTGRESQL_HOST}.postgres.database.azure.com;Database=${DATABASE_NAME};Port=5432;User Id=${USER}@${POSTGRESQL_HOST};Ssl Mode=Require;Trust Server Certificate=true"
ASPNETCORE_ENVIRONMENT=Deployment
echo "{\"ConnectionStrings\":{\"AZURE_POSTGRESQL_CONNECTIONSTRING\":\"${CONNSTRING}\"}}" >appsettings.Deployment.json
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update

az webapp connection create postgres \
    --resource-group $RESOURCE_GROUP \
    --name $APPSERVICE_PSQL \
    --tg $RESOURCE_GROUP \
    --server $POSTGRESQL_HOST \
    --database $DATABASE_NAME \
    --client-type dotnet \
    --system-identity
cd ..

# MYSQL
cd Passwordless.WebAPI.MySql

dotnet build

# Get connections string
# "Server=mysql-passwordless.mysql.database.azure.com;Database=checklist;User ID=fmiguel@microsoft.com;Port=3306;SSL Mode=Required;Allow Public Key Retrieval=True;Connection Timeout=30"
CONNSTRING="Server=${MYSQL_HOST}.mysql.database.azure.com;Database=${DATABASE_NAME};User ID=${USER};Port=3306;SSL Mode=Required;Allow Public Key Retrieval=True;Connection Timeout=30"
ASPNETCORE_ENVIRONMENT=Deployment
echo "{\"ConnectionStrings\":{\"AZURE_MYSQL_CONNECTIONSTRING\":\"${CONNSTRING}\"}}" >appsettings.Deployment.json
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update

az webapp connection create mysql-flexible \
    --resource-group $RESOURCE_GROUP \
    --name $APPSERVICE_MYSQL \
    --tg $RESOURCE_GROUP \
    --server $MYSQL_HOST \
    --database $DATABASE_NAME \
    --client-type dotnet \
    --system-identity

dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
az webapp deploy -g $RESOURCE_GROUP --name $APPSERVICE_MYSQL --src-path ./app.zip --clean true
