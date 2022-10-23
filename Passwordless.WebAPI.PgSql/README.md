# Postgresql passwordless dotnet authentication sample

This sample shows how to use Azure AD authentication to connect to a Postgresql database using the [Passwordless authentication delegate](https://github.com/felipmiguel/AzureDb.Passwordless).

The dotnet sample consists of a Rest API using Entity Framework Core to connect to a Postgresql database:

* The database is hosted in Azure Database for Postgres and Azure AD authentication is enabled.
* The application is deployed on Azure App Services and uses Managed Identities to authenticate to the database.
* The connection between Azure App Service and Azure Database for Postgres is created using Service Connections. Service Connection performs all steps to configure the application:
  * Assigns a Managed Identity to the App Service.
  * Creates a Postgres user with the Managed Identity and grants all required  permissions.
  * Creates a configuration setting in the App Service with the connection string.

# Azure Setup

All steps can be performed using the Azure Portal or Azure CLI. In this example will be shown how to perform the setup using Azure CLI.

## Pre-requisites

* Azure Subscription
* Azure CLI, version 2.41 or higher
* Dotnet SDK 6.0
* pwgen (optional). This is an utility to generate random passwords.
* psql client (optional). If you want to connect to the database.
* bash. The scripts are written in bash.

> Note: All commands were tested on WLS2 with Ubuntu 22.04 LTS. Some tools can be different in other environments.

## Postgresql

For simplicity we can define some variables that will be used during all setup.

```bash
RESOURCE_GROUP=[YOUR RESOURCE GROUP]
POSTGRESQL_HOST=[YOUR POSTGRESQL HOST]
DATABASE_NAME=checklist
POSTGRESQL_FQDN=${POSTGRESQL_HOST}.postgres.database.azure.com
LOCATION=[YOUR PREFERRED LOCATION]
POSTGRESQL_ADMIN_USER=azureuser
# Generating a random password for Posgresql admin user as it is mandatory
# postgresql admin won't be used as Azure AD authentication is leveraged also for administering the database
POSTGRESQL_ADMIN_PASSWORD=$(pwgen -s 15 1)
```

Create the resource group

```bash
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION
```

Create the postgresql server

```bash
# create postgresql server
az postgres server create \
    --name $POSTGRESQL_HOST \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $POSTGRESQL_ADMIN_USER \
    --admin-password "$POSTGRESQL_ADMIN_PASSWORD" \
    --public 0.0.0.0 \
    --sku-name GP_Gen5_2 \
    --version 11 \
    --storage-size 5120
```

Create the database

```bash
# create postgres database
az postgres db create \
    -g $RESOURCE_GROUP \
    -s $POSTGRESQL_HOST \
    -n $DATABASE_NAME
```

Assign the current Azure CLI user as the Azure AD administrator of Postgresql server. To get it:

```bash
# Get current user logged in azure cli to make it postgresql AAD admin
CURRENT_USER=$(az account show --query user.name -o tsv)
CURRENT_USER_OBJECTID=$(az ad user show --id $CURRENT_USER --query id -o tsv)
```

Now create the AAD admin

```bash
# create postgresql server AAD admin user
az postgres server ad-admin create \
    --server-name $POSTGRESQL_HOST \
    --resource-group $RESOURCE_GROUP \
    --object-id $CURRENT_USER_OBJECTID \
    --display-name $CURRENT_USER
```

Optionally, if you need to connect to the database from your local machine, you can create a firewall rule to allow your IP address. If you want to test the [application locally](#test-the-application-locally) you will need it.

```bash
# Create a temporary firewall rule to allow connections from current machine to the postgresql server
MY_IP=$(curl http://whatismyip.akamai.com)
az postgres server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server-name $POSTGRESQL_HOST \
    --name AllowCurrentMachineToConnect \
    --start-ip-address ${MY_IP} \
    --end-ip-address ${MY_IP}
```

You can find the deployment script for Postgresql in [deploypsql.sh](../deploy-psql.sh).

## Azure App Service

To deploy the application on Azure App Services define a couple of additional variables:

```bash
APPSERVICE_NAME=dotnet-passwordless
APPSERVICE_PLAN="asp-${APPSERVICE_NAME}"
```

First create the Application Service Plan. It can be hosted on Windows or Linux. In this sample it will be used Linux.

```bash
# Create app service plan
az appservice plan create \
    --name $APPSERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku B1 \
    --is-linux
```

Create the App Service

```bash
az webapp create \
    --name $APPSERVICE_NAME \
    --resource-group $RESOURCE_GROUP \
    --plan $APPSERVICE_PLAN \
    --runtime "DOTNETCORE:6.0"
```

Now create the service connection

```bash
az webapp connection create postgres \
    --resource-group $RESOURCE_GROUP \
    --name $APPSERVICE_NAME \
    --tg $RESOURCE_GROUP \
    --server $POSTGRESQL_HOST \
    --database $DATABASE_NAME \
    --client-type dotnet \
    --system-identity
```

This command will perform the following actions:

* It will assign a System Assigned Managed Identity to the App Service.
* It will create a Postgres user with the Managed Identity and grants all required permissions.
* It will create a configuration setting in the App Service with the connection string.

## Create the database schema

The application uses Entity Framework Core to connect to the database. The database schema is defined in the [ChecklistContext](./EF/ChecklistContext.cs). To create the schema it is used dotnet entity framework tools as described [here](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli#create-the-database).

To connect to the database it is used the Azure AD administrator user created in the previous step. To automate the process it is created a new ASP.Net Core environment and a configuration file for it.

```bash
# Get current user
USER=$(az account show --query user.name -o tsv)
# Get connections string
CONNSTRING="Server=${POSTGRESQL_HOST}.postgres.database.azure.com;Database=${DATABASE_NAME};Port=5432;User Id=${USER}@${POSTGRESQL_HOST};Ssl Mode=Require;Trust Server Certificate=true"
ASPNETCORE_ENVIRONMENT=Deployment
echo "{\"ConnectionStrings\":{\"AZURE_POSTGRESQL_CONNECTIONSTRING\":\"${CONNSTRING}\"}}" >appsettings.Deployment.json
```

As a result a new appsettings.Deployment.json file is created. Note that the connection string does not contain any password as it used the Azure AD authentication.

Now perform the database schema creation:

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Deploy the application

The application can be now deployed as a zip file. To do so, first create a zip file with the application files:

```bash
dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
```

Now the app.zip file can be deployed to the App Service:

```bash
az webapp deploy -g $RESOURCE_GROUP --name $APPSERVICE_NAME --src-path ./app.zip
```

### Potential issues

Note that it is possible that the assigned managed identity is not granted to access the new schema created. If that happens, you can grant the access manually. You can connect to the database using the Azure AD administrator user. To do it you can retrieve an access token that can be used as password:

```bash
export PGPASSWORD=$(az account get-access-token --resource-type oss-rdbms --output tsv --query accessToken)
```

> PGPASSWORD is a special environment variable used by the psql command to set the password. It is used in that way as the password is too long to be passed as a parameter.

Now connect to the database using the Azure AD administrator user:

```bash
psql "host=$POSTGRESQL_FQDN port=5432 user=${USER}@${POSTGRESQL_HOST} dbname=${DATABASE_NAME} sslmode=require"
```

Now grant the privileges to the user used by the App Service. The user name is generated randomly by the service connector, you can get it looking at the connection string generated by the service connector. For example if the connection string is `Server=postgres-passwordless.postgres.database.azure.com;Database=checklist;Port=5432;Ssl Mode=Require;User Id=aad_postgres_8cerg@postgres-passwordless;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=100;` Take User Id and remove the @hostname. In this case the user name is `aad_postgres_8cerg`. Now grant the privileges

```sql
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO "aad_postgres_8cerg";
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO "aad_postgres_8cerg";
```

> The purpose of this sample is not to explain how to use Postgresql. Probably it is not a good practice to grant all permissions to a user.

# Using Postgresql Passwordless Authentication library

The passwordless authentication library provides a delegate that can be used with Npgsql library to authenticate using AzureAD. During application configuration in [Program.cs](Program.cs) the delegate is registered:

```csharp
builder.Services.AddDbContextFactory<ChecklistContext>(options =>
{
    AzureIdentityPostgresqlPasswordProvider passwordProvider = new AzureIdentityPostgresqlPasswordProvider();
    string connectionString = builder.Configuration.GetConnectionStringFallback();
    options.UseNpgsql(connectionString, npgopts =>
    {
        npgopts.ProvidePasswordCallback(passwordProvider.ProvidePasswordCallback);
    });
});
```

The rest of the application is just a EntityFramework Core application.

# Test the application locally

In the same way it was created an ASP.Net Core environment for the deployment, it is created one for the local development. To do so, create or update appsettings.Development.json file with the following content:

```json
{
  "ConnectionStrings": {
    "AZURE_POSTGRESQL_CONNECTIONSTRING": "Server=${POSTGRESQL_HOST}.postgres.database.azure.com;Database=${DATABASE_NAME};Port=5432;User Id=${USER}@${POSTGRESQL_HOST};Ssl Mode=Require;Trust Server Certificate=true"
  }
}
```

Now set `ASPNETCORE_ENVIRONMENT` to `Development` and run the application:

```bash
ASPNETCORE_ENVIRONMENT=Development
dotnet run
```
