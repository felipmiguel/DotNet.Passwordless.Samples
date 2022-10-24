# Create a single database and configure a firewall rule

LOCATION="eastus"
RESOURCE_GROUP=rg-dotnet-passwordless
MSSQL_HOST="mssql-passwordless"
DATABASE="checklist"
MSSQL_ADMIN_USER=azureuser
MSSQL_ADMIN_PASSWORD=$(pwgen -s 15 1)
# Specify appropriate IP address values for your environment
# to limit access to the SQL Database server
startIp=0.0.0.0
endIp=0.0.0.0

# Get current user logged in azure cli to make it postgresql AAD admin
CURRENT_USER=$(az account show --query user.name -o tsv)
CURRENT_USER_OBJECTID=$(az ad user show --id $CURRENT_USER --query id -o tsv)

az sql server create \
	--name $MSSQL_HOST \
	--resource-group $RESOURCE_GROUP \
	--location $LOCATION \
	--admin-user MSSQL_ADMIN_USER \
	--admin-password $MSSQL_ADMIN_PASSWORD
echo "Configuring firewall..."
az sql server firewall-rule create --resource-group $RESOURCE_GROUP --server $MSSQL_HOST -n AllowYourIp --start-ip-address $startIp --end-ip-address $endIp
echo "Creating $database on $server..."
az sql db create \
	--resource-group $RESOURCE_GROUP \
	--server $MSSQL_HOST \
	--name $DATABASE \
	--edition GeneralPurpose \
	--family Gen5 \
	--capacity 2 \
	--compute-model Serverless

az sql server ad-admin create \
	--resource-group $RESOURCE_GROUP \
	--server $MSSQL_HOST \
    --object-id $CURRENT_USER_OBJECTID \
    --display-name $CURRENT_USER
	