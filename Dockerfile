ARG PROJECT_FILE

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJECT_FILE
WORKDIR /src
COPY . .
RUN dotnet restore "${PROJECT_FILE}"
RUN dotnet publish "${PROJECT_FILE}" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG APP_DLL
WORKDIR /app
COPY --from=build /app/publish .
ENV APP_DLL="${APP_DLL}"
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet \"$APP_DLL\""]
