#!/bin/bash
set -e

kubectl delete -f https://github.com/kubernetes-sigs/metrics-server/releases/download/metrics-server-helm-chart-3.7.0/components.yaml