# kafka-transactional-outbox-demo
## Overview
The code demonstrates an approach of using the "transactional outbox" to handle events.

## How to run
- Run `docker/docker-compose up` to create the Kafka cluster and PostgreSQL DB instance.
- Navigate to `http://localhost:9000` and create a new topic `books-topic` with the number of partitions = 50.
- Go to `src/kafka-transactional-outbox-demo` and run `dotnet run`.