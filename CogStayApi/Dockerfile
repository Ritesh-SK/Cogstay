FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CogStayApi/CogStayApi.csproj", "CogStayApi/"]
COPY ["CogStay.Domain/CogStay.Domain.csproj", "CogStay.Domain/"]
COPY ["CogStay.Application/CogStay.Application.csproj", "CogStay.Application/"]
COPY ["CogStay.Infrastructure/CogStay.Infrastructure.csproj", "CogStay.Infrastructure/"]
RUN dotnet restore "CogStayApi/CogStayApi.csproj"
COPY . .
WORKDIR "/src/CogStayApi"
RUN dotnet build "CogStayApi.csproj" -c Release -o /app/build
RUN dotnet publish "CogStayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CogStayApi.dll"]