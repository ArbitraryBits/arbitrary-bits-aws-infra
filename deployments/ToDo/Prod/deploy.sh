#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-prod.txt
aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > .env && \
    docker compose up -d && \
    rm .env