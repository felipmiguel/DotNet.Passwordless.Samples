rm -rf publish
dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
az webapp deploy -g rg-dotnet-passwordless --name dotnet-passwordless-mysql --src-path ./app.zip --clean true
curl https://dotnet-passwordless-mysql.azurewebsites.net/checklist


