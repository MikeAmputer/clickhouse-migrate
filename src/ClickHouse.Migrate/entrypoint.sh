#!/bin/sh

set -e

if [ "$(id -u)" -eq 0 ]; then

  if [ -d "/usr/local/share/ca-certificates" ] && \
      [ -n "$(find /usr/local/share/ca-certificates -type f -print -quit 2>/dev/null)" ]; then

    echo "Updating CA certificates..."
    update-ca-certificates || echo "Warning: update-ca-certificates failed"

  fi
fi

exec su-exec appuser dotnet ClickHouse.Migrate.dll "$@"