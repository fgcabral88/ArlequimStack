Write-Host "🚀 Iniciando infraestrutura..." -ForegroundColor Green
docker-compose up -d mysql rabbitmq jaeger

Write-Host "⏳ Aguardando serviços ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "🔨 Build da imagem..." -ForegroundColor Cyan
docker build -t sports-api -f SportsEquipment.Api/Dockerfile .

Write-Host "🗑️ Removendo container antigo se existir..." -ForegroundColor Gray
docker stop sports-api-manual 2>$null
docker rm sports-api-manual 2>$null

Write-Host "▶️ Iniciando API..." -ForegroundColor Green
docker run -d --name sports-api-manual --network arlequimstack_sports-network -p 8080:80 -e ASPNETCORE_ENVIRONMENT=Docker -e "ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=sports_equipment_db;User=root;Password=Database@2026*;" -e "RabbitMq__Uri=rabbitmq://rabbitmq" -e "RabbitMq__User=guest" -e "RabbitMq__Password=guest" -e "Jwt__Secret=f6a467687223f18bdb6dbfe86352fcc9b28171dffa64049fb4efa19215c2874b" -e "Jwt__Issuer=SportsEquipment.Api" -e "Jwt__Audience=SportsEquipment.Client" -e "Jwt__ExpiryMinutes=60" sports-api

Write-Host "✅ API rodando em http://localhost:8080/swagger" -ForegroundColor Green
Write-Host "📊 RabbitMQ Management em http://localhost:15672" -ForegroundColor Green
Write-Host "🔍 Jaeger UI em http://localhost:16686" -ForegroundColor Green
Write-Host ""
Write-Host "Para ver logs: docker logs -f sports-api-manual" -ForegroundColor Cyan