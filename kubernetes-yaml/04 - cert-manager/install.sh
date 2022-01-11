#!/bin/bash
set -e

kubectl apply -f 01-cert-manager.yaml
sleep 15
kubectl apply -f 02-issuers.yaml