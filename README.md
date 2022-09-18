# kafka-transactional-outbox-demo
## Overview
- The code demonstrates an implementation of the "transactional outbox" approach.
- The app creates a DB with two tables Books and BooksOutbox.
- Every few seconds a new book will be published (by a background service), and records will be added to both DB tables in a single transaction.
- After a new book is published a handler will be raised and records from the outbox table will be sent to the Kafka topic.

## How to run
- Run `docker/docker-compose up` to create the Kafka cluster and PostgreSQL DB instance.
- Navigate to `http://localhost:9000` and create a new topic `books-topic` with the number of partitions = 50.
- Go to `src/kafka-transactional-outbox-demo` and run `dotnet run`.