FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src

COPY . .

RUN dotnet restore ./IntegrationTests/IntegrationTests.csproj
RUN dotnet build ./IntegrationTests/IntegrationTests.csproj -c Release --no-restore

CMD ["dotnet","test","./IntegrationTests/IntegrationTests.csproj","-c","Release","--no-build","-l","trx;LogFileName=test_results.trx","-l","console;verbosity=detailed"]
