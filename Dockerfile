ARG PROJECT_FILE

FROM node:22-alpine AS frontend
WORKDIR /client
COPY VinWeb/ClientApp/package*.json ./
RUN npm ci
COPY VinWeb/ClientApp/ ./
RUN npm run build

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
COPY --from=frontend /client/dist/vin-client/ ./wwwroot/
ENV APP_DLL="${APP_DLL}"
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet \"$APP_DLL\""]
