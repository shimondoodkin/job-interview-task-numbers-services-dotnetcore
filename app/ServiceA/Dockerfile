# Use the official Microsoft .NET SDK image for building the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# perbuild 
COPY ["SharedProject/SharedProject.csproj", "SharedProject/"]
COPY ["ServiceA/ServiceA.csproj", "ServiceA/"]

RUN dotnet restore "SharedProject/SharedProject.csproj"
RUN dotnet restore "ServiceA/ServiceA.csproj"


# Copy the source code and build the application
COPY . .

# remove some files to resolve ambiguity in publishing
RUN rm -f "SharedProject/appsettings.json"

RUN dotnet build "ServiceA/ServiceA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceA/ServiceA.csproj" -c Release -o /app/publish

# Use the official Microsoft .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceA.dll"]
