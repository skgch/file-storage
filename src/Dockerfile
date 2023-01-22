FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish ./FileStorage.WebApi -c Release -o /app --use-current-runtime --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir /storage
ENTRYPOINT ["dotnet", "FileStorage.WebApi.dll"]