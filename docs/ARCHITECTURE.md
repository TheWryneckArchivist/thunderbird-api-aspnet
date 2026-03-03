# Thunderbird API Architecture

- ASP.NET gRPC services are the authoritative backend.
- Postgres stores durable state and outbox events.
- Redis supports cache/locks/rate-limit state.
- RabbitMQ carries asynchronous domain events and worker jobs.
- Envoy provides gRPC-Web translation for browser clients.
