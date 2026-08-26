FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CogStay/CogStay.csproj", "CogStay/"]
COPY ["CogStay.Domain/CogStay.Domain.csproj", "CogStay.Domain/"]
COPY ["CogStay.Application/CogStay.Application.csproj", "CogStay.Application/"]
COPY ["CogStay.Infrastructure/CogStay.Infrastructure.csproj", "CogStay.Infrastructure/"]
RUN dotnet restore "CogStay/CogStay.csproj"
COPY . .
WORKDIR "/src/CogStay"
RUN dotnet build "CogStay.csproj" -c Release -o /app/build
RUN dotnet publish "CogStay.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CogStay.dll"]