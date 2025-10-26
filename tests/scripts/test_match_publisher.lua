local http = require("socket.http")
local ltn12 = require("ltn12")
local json = require("cjson")

local RABBITMQ_HOST = ""
local RABBITMQ_PORT = "15672"  -- Porta HTTP API
local RABBITMQ_USER = "admin"
local RABBITMQ_PASSWORD = ""
local EXCHANGE_NAME = "game-events"
local ROUTING_KEY = "match.ended"

local PLAYER_A_ID = "e19cfe2c-4f56-4914-841b-0aac8b46be24"
local PLAYER_B_ID = "0e3cc492-ec0f-4fc3-a2de-dfa2f84b6fa3"

-- Função para gerar UUID simples
local function generate_uuid()
    local template = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
    return string.gsub(template, '[xy]', function (c)
        local v = (c == 'x') and math.random(0, 0xf) or math.random(8, 0xb)
        return string.format('%x', v)
    end)
end

-- Função para obter timestamp ISO 8601
local function get_iso_timestamp()
    return os.date("!%Y-%m-%dT%H:%M:%S.000Z")
end

-- Função para criar evento de teste
local function create_test_match_event()
    local match_id = generate_uuid()
    local timestamp = get_iso_timestamp()

    local event = {
        matchId = match_id,
        playerAId = PLAYER_A_ID,
        playerBId = PLAYER_B_ID,
        rounds = {
            {
                id = generate_uuid(),
                roundNumber = 1,
                gameResponse = {
                    x = -23.5505,
                    y = -46.6333
                },
                playerAGuess = {
                    x = -23.5455,
                    y = -46.6283
                },
                playerBGuess = {
                    x = -23.5305,
                    y = -46.6133
                }
            },
            {
                id = generate_uuid(),
                roundNumber = 2,
                gameResponse = {
                    x = -23.5605,
                    y = -46.6433
                },
                playerAGuess = {
                    x = -23.5555,
                    y = -46.6383
                },
                playerBGuess = {
                    x = -23.5405,
                    y = -46.6233
                }
            },
            {
                id = generate_uuid(),
                roundNumber = 3,
                gameResponse = {
                    x = -23.5705,
                    y = -46.6533
                },
                playerAGuess = {
                    x = -23.5655,
                    y = -46.6483
                },
                playerBGuess = {
                    x = -23.5505,
                    y = -46.6333
                }
            }
        },
        endedAt = timestamp
    }

    return event
end

-- Função para publicar evento usando HTTP API do RabbitMQ
local function publish_match_event(event)
    local url = string.format(
        "http://%s:%s@%s:%s/api/exchanges/%%2F/%s/publish",
        RABBITMQ_USER,
        RABBITMQ_PASSWORD,
        RABBITMQ_HOST,
        RABBITMQ_PORT,
        EXCHANGE_NAME
    )

    local payload = {
        properties = {
            delivery_mode = 2,
            content_type = "application/json"
        },
        routing_key = ROUTING_KEY,
        payload = json.encode(event),
        payload_encoding = "string"
    }

    local request_body = json.encode(payload)
    local response_body = {}

    local res, code, headers, status = http.request{
        url = url,
        method = "POST",
        headers = {
            ["Content-Type"] = "application/json",
            ["Content-Length"] = tostring(#request_body)
        },
        source = ltn12.source.string(request_body),
        sink = ltn12.sink.table(response_body)
    }

    if code == 200 then
        local response = json.decode(table.concat(response_body))
        if response.routed then
            print("✓ Evento publicado com sucesso!")
            print(string.format("  Match ID: %s", event.matchId))
            print(string.format("  Player A: %s", event.playerAId))
            print(string.format("  Player B: %s", event.playerBId))
            print(string.format("  Rounds: %d", #event.rounds))
            print()
            print("Verifique os logs do geo-data-service para confirmar o processamento.")
            print(string.format("Consulte a partida em: http://localhost:5000/api/matches/%s", event.matchId))
            return true
        else
            print("✗ Mensagem não foi roteada")
            return false
        end
    else
        print(string.format("✗ Erro ao publicar: HTTP %d", code))
        print(table.concat(response_body))
        return false
    end
end

-- Main
math.randomseed(os.time())

print("Criando evento de teste...")
local event = create_test_match_event()

print("Publicando no RabbitMQ...")
local success = publish_match_event(event)

if not success then
    os.exit(1)
end
