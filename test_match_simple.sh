#!/bin/bash
# Script simples para publicar eventos match.ended no RabbitMQ usando curl

# Configuração
RABBITMQ_HOST="location404-rabbitmq-8b2418-181-215-135-221.traefik.me"
RABBITMQ_PORT="15672"
RABBITMQ_USER="admin"
RABBITMQ_PASSWORD="rqtisgqs"
EXCHANGE_NAME="game-events"
ROUTING_KEY="match.ended"

# IDs dos jogadores
PLAYER_A_ID="e19cfe2c-4f56-4914-841b-0aac8b46be24"
PLAYER_B_ID="0e3cc492-ec0f-4fc3-a2de-dfa2f84b6fa3"

# Gerar UUIDs
MATCH_ID=$(cat /proc/sys/kernel/random/uuid)
ROUND1_ID=$(cat /proc/sys/kernel/random/uuid)
ROUND2_ID=$(cat /proc/sys/kernel/random/uuid)
ROUND3_ID=$(cat /proc/sys/kernel/random/uuid)

# Timestamp atual
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")

echo "========================================="
echo "Publicando evento de teste no RabbitMQ"
echo "========================================="
echo "Match ID: $MATCH_ID"
echo "Player A: $PLAYER_A_ID"
echo "Player B: $PLAYER_B_ID"
echo ""

# Criar payload do evento
PAYLOAD=$(cat <<EOF
{
  "matchId": "$MATCH_ID",
  "playerAId": "$PLAYER_A_ID",
  "playerBId": "$PLAYER_B_ID",
  "rounds": [
    {
      "id": "$ROUND1_ID",
      "roundNumber": 1,
      "gameResponse": {
        "x": -23.5505,
        "y": -46.6333
      },
      "playerAGuess": {
        "x": -23.5455,
        "y": -46.6283
      },
      "playerBGuess": {
        "x": -23.5305,
        "y": -46.6133
      }
    },
    {
      "id": "$ROUND2_ID",
      "roundNumber": 2,
      "gameResponse": {
        "x": -23.5605,
        "y": -46.6433
      },
      "playerAGuess": {
        "x": -23.5555,
        "y": -46.6383
      },
      "playerBGuess": {
        "x": -23.5405,
        "y": -46.6233
      }
    },
    {
      "id": "$ROUND3_ID",
      "roundNumber": 3,
      "gameResponse": {
        "x": -23.5705,
        "y": -46.6533
      },
      "playerAGuess": {
        "x": -23.5655,
        "y": -46.6483
      },
      "playerBGuess": {
        "x": -23.5505,
        "y": -46.6333
      }
    }
  ],
  "endedAt": "$TIMESTAMP"
}
EOF
)

# Escapar payload para JSON
PAYLOAD_ESCAPED=$(echo "$PAYLOAD" | sed 's/"/\\"/g' | tr -d '\n')

# Publicar no RabbitMQ via HTTP API
RESPONSE=$(curl -s -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X POST \
  -H "Content-Type: application/json" \
  -d "{
    \"properties\": {
      \"delivery_mode\": 2,
      \"content_type\": \"application/json\"
    },
    \"routing_key\": \"$ROUTING_KEY\",
    \"payload\": \"$PAYLOAD_ESCAPED\",
    \"payload_encoding\": \"string\"
  }" \
  "http://$RABBITMQ_HOST:$RABBITMQ_PORT/api/exchanges/%2F/$EXCHANGE_NAME/publish")

# Verificar resultado
if echo "$RESPONSE" | grep -q '"routed":true'; then
  echo "✓ Evento publicado com sucesso!"
  echo ""
  echo "Verifique os logs do geo-data-service para confirmar o processamento."
  echo "Consulte a partida em: http://localhost:5000/api/matches/$MATCH_ID"
  echo ""
  echo "Para verificar se salvou, execute:"
  echo "  curl http://localhost:5000/api/matches/$MATCH_ID"
else
  echo "✗ Erro ao publicar evento"
  echo "Resposta: $RESPONSE"
  exit 1
fi
