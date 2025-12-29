#!/bin/bash
set -e

echo "Aguardando MySQL estar pronto..."
until dotnet ef database update --project /src/SportsEquipment.Infrastructure --startup-project /src/SportsEquipment.Api --no-build 2>/dev/null; do
  echo "MySQL não está pronto - aguardando..."
  sleep 2
done

echo "Migrations aplicadas com sucesso!"
echo "Iniciando aplicação..."

exec dotnet SportsEquipment.Api.dll