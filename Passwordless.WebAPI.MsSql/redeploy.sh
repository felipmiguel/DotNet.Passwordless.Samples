rm -rf publish
dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
az webapp deploy -g rg-dotnet-passwordless --name dotnet-passwordless-mssql --src-path ./app.zip --clean true
curl https://dotnet-passwordless-mssql.azurewebsites.net/checklist


