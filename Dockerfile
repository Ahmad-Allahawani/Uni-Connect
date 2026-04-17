# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Uni-Connect/Uni-Connect.csproj", "Uni-Connect/"]
RUN dotnet restore "Uni-Connect/Uni-Connect.csproj"

# Copy everything else and build the app
COPY . .
WORKDIR "/src/Uni-Connect"
RUN dotnet build "Uni-Connect.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "Uni-Connect.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Uni-Connect.dll"]
