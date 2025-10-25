#!/bin/bash

# Test RabbitMQ consumer by publishing a sample match ended event

RABBITMQ_HOST="location404-rabbitmq-8b2418-181-215-135-221.traefik.me"
RABBITMQ_PORT=5672
RABBITMQ_USER="admin"
RABBITMQ_PASS="nxokmkqg"
EXCHANGE="game-events"
ROUTING_KEY="match.ended"

# Sample match ended event payload
PAYLOAD='{
  "matchId": "00000000-0000-0000-0000-000000000001",
  "playerAId": "11111111-1111-1111-1111-111111111111",
  "playerBId": "22222222-2222-2222-2222-222222222222",
  "winnerId": "11111111-1111-1111-1111-111111111111",
  "loserId": "22222222-2222-2222-2222-222222222222",
  "playerATotalPoints": 12500,
  "playerBTotalPoints": 10300,
  "pointsEarned": 25,
  "pointsLost": -10,
  "startTime": "2025-10-24T20:00:00Z",
  "endTime": "2025-10-24T20:15:00Z",
  "rounds": [
    {
      "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "gameMatchId": "00000000-0000-0000-0000-000000000001",
      "roundNumber": 1,
      "playerAId": "11111111-1111-1111-1111-111111111111",
      "playerBId": "22222222-2222-2222-2222-222222222222",
      "playerAPoints": 4800,
      "playerBPoints": 4700,
      "gameResponse": { "x": -23.5505, "y": -46.6333 },
      "playerAGuess": { "x": -23.5600, "y": -46.6400 },
      "playerBGuess": { "x": -23.5400, "y": -46.6200 },
      "gameRoundEnded": true
    },
    {
      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "gameMatchId": "00000000-0000-0000-0000-000000000001",
      "roundNumber": 2,
      "playerAId": "11111111-1111-1111-1111-111111111111",
      "playerBId": "22222222-2222-2222-2222-222222222222",
      "playerAPoints": 3900,
      "playerBPoints": 3200,
      "gameResponse": { "x": 35.6762, "y": 139.6503 },
      "playerAGuess": { "x": 35.6800, "y": 139.6600 },
      "playerBGuess": { "x": 35.6700, "y": 139.6400 },
      "gameRoundEnded": true
    },
    {
      "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
      "gameMatchId": "00000000-0000-0000-0000-000000000001",
      "roundNumber": 3,
      "playerAId": "11111111-1111-1111-1111-111111111111",
      "playerBId": "22222222-2222-2222-2222-222222222222",
      "playerAPoints": 3800,
      "playerBPoints": 2400,
      "gameResponse": { "x": 48.8566, "y": 2.3522 },
      "playerAGuess": { "x": 48.8600, "y": 2.3600 },
      "playerBGuess": { "x": 48.8500, "y": 2.3400 },
      "gameRoundEnded": true
    }
  ]
}'

echo "Testing RabbitMQ consumer..."
echo "Publishing match ended event to: $RABBITMQ_HOST"
echo ""

# Check if python3 and pika are available
if ! command -v python3 &> /dev/null; then
    echo "❌ Python3 not found. Installing rabbitmqadmin instead..."
    echo ""
    echo "Manual test command:"
    echo "docker run --rm -it --network host python:3.11-slim bash -c \\"
    echo "  pip install pika && \\"
    echo "  python3 -c \\\"
import pika
import json

credentials = pika.PlainCredentials('$RABBITMQ_USER', '$RABBITMQ_PASS')
connection = pika.BlockingConnection(pika.ConnectionParameters('$RABBITMQ_HOST', $RABBITMQ_PORT, '/', credentials))
channel = connection.channel()

payload = $PAYLOAD

channel.basic_publish(
    exchange='$EXCHANGE',
    routing_key='$ROUTING_KEY',
    body=json.dumps(payload),
    properties=pika.BasicProperties(content_type='application/json', delivery_mode=2)
)

print('✅ Message published successfully!')
connection.close()
\\\"\\"
    exit 1
fi

# Try with python3 + pika
python3 -c "
import pika
import json
import sys

try:
    credentials = pika.PlainCredentials('$RABBITMQ_USER', '$RABBITMQ_PASS')
    connection = pika.BlockingConnection(pika.ConnectionParameters('$RABBITMQ_HOST', $RABBITMQ_PORT, '/', credentials))
    channel = connection.channel()

    payload = $PAYLOAD

    channel.basic_publish(
        exchange='$EXCHANGE',
        routing_key='$ROUTING_KEY',
        body=json.dumps(payload),
        properties=pika.BasicProperties(content_type='application/json', delivery_mode=2)
    )

    print('✅ Message published successfully to RabbitMQ!')
    print('✅ geo-data-service should consume and process this event')
    connection.close()
except Exception as e:
    print(f'❌ Error: {e}')
    sys.exit(1)
" 2>&1

if [ $? -eq 0 ]; then
    echo ""
    echo "🎯 Check geo-data-service logs to see if the event was processed"
else
    echo ""
    echo "ℹ️  Alternative: Use the docker command above to test"
fi
