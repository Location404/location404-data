#!/usr/bin/env python3
"""
Script to publish test match.ended events to RabbitMQ for testing geo-data-service
without needing to play actual games.
"""

import pika
import json
import uuid
from datetime import datetime, timezone

# RabbitMQ Configuration
RABBITMQ_HOST = "location404-rabbitmq-8b2418-181-215-135-221.traefik.me"
RABBITMQ_PORT = 5672
RABBITMQ_USER = "admin"
RABBITMQ_PASSWORD = "rqtisgqs"
RABBITMQ_VHOST = "/"
EXCHANGE_NAME = "game-events"
ROUTING_KEY = "match.ended"

def create_test_match_event(player_a_id: str, player_b_id: str):
    """
    Creates a test GameMatchEndedEventDto
    """
    match_id = str(uuid.uuid4())
    now = datetime.now(timezone.utc).isoformat()

    # Create 3 rounds with realistic data
    rounds = []
    for i in range(1, 4):
        round_id = str(uuid.uuid4())

        # Random coordinates for correct answer
        correct_x = -23.5505 + (i * 0.01)  # Near São Paulo
        correct_y = -46.6333 + (i * 0.01)

        # Player A guess (closer to correct answer)
        player_a_x = correct_x + 0.005
        player_a_y = correct_y + 0.005

        # Player B guess (farther from correct answer)
        player_b_x = correct_x + 0.02
        player_b_y = correct_y + 0.02

        rounds.append({
            "id": round_id,
            "roundNumber": i,
            "gameResponse": {
                "x": correct_x,
                "y": correct_y
            },
            "playerAGuess": {
                "x": player_a_x,
                "y": player_a_y
            },
            "playerBGuess": {
                "x": player_b_x,
                "y": player_b_y
            }
        })

    event = {
        "matchId": match_id,
        "playerAId": player_a_id,
        "playerBId": player_b_id,
        "rounds": rounds,
        "endedAt": now
    }

    return event

def publish_match_event(event):
    """
    Publishes a match.ended event to RabbitMQ
    """
    credentials = pika.PlainCredentials(RABBITMQ_USER, RABBITMQ_PASSWORD)
    parameters = pika.ConnectionParameters(
        host=RABBITMQ_HOST,
        port=RABBITMQ_PORT,
        virtual_host=RABBITMQ_VHOST,
        credentials=credentials
    )

    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    # Ensure exchange exists
    channel.exchange_declare(
        exchange=EXCHANGE_NAME,
        exchange_type='topic',
        durable=True
    )

    # Publish message
    message = json.dumps(event)
    channel.basic_publish(
        exchange=EXCHANGE_NAME,
        routing_key=ROUTING_KEY,
        body=message,
        properties=pika.BasicProperties(
            delivery_mode=2,  # Make message persistent
            content_type='application/json'
        )
    )

    print(f"✓ Published match.ended event:")
    print(f"  Match ID: {event['matchId']}")
    print(f"  Player A: {event['playerAId']}")
    print(f"  Player B: {event['playerBId']}")
    print(f"  Rounds: {len(event['rounds'])}")

    connection.close()

def main():
    # Use test player IDs - you can replace these with actual player IDs from your database
    # Or pass them as command line arguments
    player_a_id = "e19cfe2c-4f56-4914-841b-0aac8b46be24"  # From the match we verified
    player_b_id = "0e3cc492-ec0f-4fc3-a2de-dfa2f84b6fa3"  # From the match we verified

    print("Creating test match event...")
    event = create_test_match_event(player_a_id, player_b_id)

    print("Publishing to RabbitMQ...")
    publish_match_event(event)

    print("\n✓ Done! Check geo-data-service logs to verify processing.")
    print(f"  You can query the match at: http://localhost:5000/api/matches/{event['matchId']}")

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"✗ Error: {e}")
        import traceback
        traceback.print_exc()
