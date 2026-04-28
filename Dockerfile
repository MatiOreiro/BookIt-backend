FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BookIt.API/BookIt.API.csproj", "BookIt.API/"]
RUN dotnet restore "BookIt.API/BookIt.API.csproj"

COPY . .
RUN dotnet publish "BookIt.API/BookIt.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookIt.API.dll"]