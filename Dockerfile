#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ./src .
RUN dotnet restore COLID.ReportingService.WebApi/WebApi.csproj 

# build app
WORKDIR /src/COLID.ReportingService.WebApi
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ENV PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE $PORT
WORKDIR /app
COPY --from=build /app ./
CMD ["dotnet", "COLID.ReportingService.WebApi.dll"]
