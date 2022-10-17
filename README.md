# Azure AD Authentication samples for dotnet core

This repository contains usage sample for Azure AD Authentication for dotnet core. Many Azure services supports Azure AD as an authentication mechanism, for instance Azure Database for Postgres or MySql. The client application can use Azure.Identity library to get an access token to be used as authentication credential, so it is possible to use Managed Identities, azure cli, Visual Studio/IntelliJ to authenticate.

For Postgres and MySQL, the samples use the Passwordless libraries that can be found in this repository [AzureDb.Passwordless](https://github.com/felipmiguel/AzureDb.Passwordless). That repository publishes nuget packages to facilitate the development of applications that use Azure AD authentication.

* [Postgresql](https://github.com/felipmiguel/AzureDb.Passwordless/packages/1669347)
* [MySql](https://github.com/felipmiguel/AzureDb.Passwordless/packages/1669348). This package depends on a customized version of MySql Connector for Net, that can be found in this [repository](https://github.com/felipmiguel/mysql-connector-net) and that exposes nuget [packages](https://github.com/felipmiguel?tab=packages&repo_name=mysql-connector-net)

To setup your local environment it is necessary to add `https://nuget.pkg.github.com/felipmiguel/index.json` as a nuget source feed. You can do it by running the following command:

```bash
dotnet nuget add source --username [YOUR USERNAME] --password [YOUR PAT] --store-password-in-clear-text --name github "https://nuget.pkg.github.com/felipmiguel/index.json"
```

Your username correspond to your GitHub user name, so it is necessary having a GitHub account. The PAT (Personal Access Token) can be generated in your GitHub account settings. The PAT needs to have the following scope: _read:packages_.

You can find more information about how to create a PAT in this [link](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token).

## Available samples

* [WebApi EntityFramework Postgres](./Passwordless.WebAPI.PgSql/)
