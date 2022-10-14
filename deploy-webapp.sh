RESOURCE_GROUP=rg-postgres-passwordless
POSTGRESQL_HOST=postgres-passwordless
DATABASE_NAME=checklist
DATABASE_FQDN=${POSTGRESQL_HOST}.postgres.database.azure.com
LOCATION=eastus
POSTGRESQL_ADMIN_USER=azureuser

APPSERVICE_PLAN=asp-dotnet-passwordless
APPSERVICE_NAME=dotnet-passwordless

# Create app service plan
az appservice plan create --name $APPSERVICE_PLAN --resource-group $RESOURCE_GROUP --location $LOCATION --sku B1 --is-linux

az webapp create --name $APPSERVICE_NAME --resource-group $RESOURCE_GROUP --plan $APPSERVICE_PLAN --runtime "DOTNETCORE:6.0"

cd Passwordless.WebAPI.PgSql

dotnet build

# Get current user
USER=$(az account show --query user.name -o tsv)

# Get connections string
CONNSTRING="Server=${POSTGRESQL_HOST}.postgres.database.azure.com.postgres.database.azure.com;Database=${DATABASE_NAME};Port=5432;User Id=${USER}@${POSTGRESQL_HOST};Ssl Mode=Require;Trust Server Certificate=true"
ASPNETCORE_ENVIRONMENT=Deployment
echo "{\"ConnectionStrings\":{\"AZURE_POSTGRESQL_CONNECTIONSTRING\":\"${CONNSTRING}\"}}" > appsettings.Deployment.json
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update

az webapp connection create postgres \
    --resource-group $RESOURCE_GROUP \
    --name $APPSERVICE_NAME \
    --tg $RESOURCE_GROUP \
    --server $POSTGRESQL_HOST \
    --database $DATABASE_NAME \
    --client-type dotnet \
    --system-identity

dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
az webapp deploy -g $RESOURCE_GROUP --name $APPSERVICE_NAME --src-path ./app.zip --clean true