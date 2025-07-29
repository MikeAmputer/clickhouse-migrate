FROM mcr.microsoft.com/dotnet/sdk:9.0.302-alpine3.22 AS publish
WORKDIR /src
COPY . .

ARG APP_VERSION=0.0.0
RUN dotnet publish "ClickHouse.Migrate.csproj" -f net9.0 -c Release -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION


FROM mcr.microsoft.com/dotnet/runtime:9.0.7-alpine3.22 AS final

RUN apk add --no-cache ca-certificates su-exec bash

WORKDIR /app
COPY --from=publish /app/publish .

ENV DOTNET_EnableDiagnostics=0

RUN addgroup -g 1001 appgroup && adduser -S -u 1001 -G appgroup appuser

COPY --chown=appuser:appgroup entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
CMD ["up"]