# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for optimized Docker caching
COPY ["QuickCart/QuickCart.csproj", "QuickCart/"]
COPY ["QuickCart.DataAccess/QuickCart.DataAccess.csproj", "QuickCart.DataAccess/"]
COPY ["QuickCart.Models/QuickCart.Models.csproj", "QuickCart.Models/"]
COPY ["QuickCart.Utility/QuickCart.Utility.csproj", "QuickCart.Utility/"]

# Restore dependencies
RUN dotnet restore "QuickCart/QuickCart.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/QuickCart"
RUN dotnet build "QuickCart.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "QuickCart.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickCart.dll"]
