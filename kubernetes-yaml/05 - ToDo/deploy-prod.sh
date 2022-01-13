#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-prod.txt
aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > secrets.txt && \
    kubectl create namespace todo-prod --dry-run=client -o yaml | kubectl apply -f - && \
    kubectl create secret generic -n todo-prod todo-prod-secret --from-env-file=secrets.txt --dry-run=client -o yaml | kubectl apply -f - && \
    kubectl apply -f ToDoServiceProd.yaml && \
    rm secrets.txt