FROM mcr.microsoft.com/dotnet/sdk:9.0.302-alpine3.22 AS publish
WORKDIR /src
COPY . .

ARG APP_VERSION=0.0.0
RUN dotnet publish "ClickHouse.Migrate.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION


FROM mcr.microsoft.com/dotnet/runtime:9.0.7-alpine3.22 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV DOTNET_EnableDiagnostics=0

# Create and use non-root user
RUN addgroup -g 1001 appgroup && adduser -S -u 1001 -G appgroup appuser
USER appuser

ENTRYPOINT ["dotnet", "ClickHouse.Migrate.dll"]
CMD ["up"]