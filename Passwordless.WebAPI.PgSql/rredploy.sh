rm -rf publish
dotnet publish -c Release -o publish
cd publish
zip -r app.zip .
az webapp deploy -g rg-postgres-passwordless --name dotnet-passwordless --src-path ./app.zip --clean true
curl https://dotnet-passwordless.azurewebsites.net/checklist