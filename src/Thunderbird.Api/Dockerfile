FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

COPY . .
RUN dotnet restore Thunderbird.Platform.slnx
RUN dotnet publish src/Thunderbird.Api/Thunderbird.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "Thunderbird.Api.dll"]
