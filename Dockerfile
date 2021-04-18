FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY . .
RUN dotnet build
RUN dotnet test
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "DR.Services.Projects.dll"]