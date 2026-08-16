#!/bin/bash
set -euo pipefail

# ============================================================
# TechStore Cloud — Script de Deploy AWS
# Uso: ./deploy-aws.sh [REGIAO] [NOME_STACK]
# Pré-requisitos: AWS CLI configurado, Docker, jq
# ============================================================

REGION="${1:-us-east-1}"
STACK_NAME="${2:-techstore-cloud}"
S3_BUCKET="techstore-frontend-$(aws sts get-caller-identity --query Account --output text)"
KEY_NAME="techstore-key"

echo "=== TechStore Cloud — Deploy AWS ==="
echo "Região: $REGION"
echo "Stack: $STACK_NAME"
echo ""

# 1. Criar bucket S3 para o frontend
echo "[1/5] Criando bucket S3 para frontend..."
aws s3 mb "s3://$S3_BUCKET" --region "$REGION" 2>/dev/null || true

aws s3 website "s3://$S3_BUCKET" \
    --index-document index.html \
    --error-document index.html

cat <<POLICY | aws s3api put-bucket-policy --bucket "$S3_BUCKET" --policy file:///dev/stdin
{
    "Version": "2012-10-17",
    "Statement": [{
        "Sid": "PublicReadGetObject",
        "Effect": "Allow",
        "Principal": "*",
        "Action": "s3:GetObject",
        "Resource": "arn:aws:s3:::$S3_BUCKET/*"
    }]
}
POLICY

echo "   Bucket: $S3_BUCKET"

# 2. Upload do frontend
echo "[2/5] Fazendo upload do frontend..."
aws s3 sync ../frontend/ "s3://$S3_BUCKET/" --delete

FRONTEND_URL="http://$S3_BUCKET.s3-website-$REGION.amazonaws.com"
echo "   Frontend URL: $FRONTEND_URL"

# 3. Criar Key Pair para EC2 (se não existir)
echo "[3/5] Verificando Key Pair..."
if ! aws ec2 describe-key-pairs --key-names "$KEY_NAME" --region "$REGION" &>/dev/null; then
    aws ec2 create-key-pair --key-name "$KEY_NAME" --region "$REGION" \
        --query 'KeyMaterial' --output text > "${KEY_NAME}.pem"
    chmod 400 "${KEY_NAME}.pem"
    echo "   Key Pair criado: ${KEY_NAME}.pem"
else
    echo "   Key Pair já existe: $KEY_NAME"
fi

# 4. Deploy CloudFormation
echo "[4/5] Fazendo deploy da infraestrutura via CloudFormation..."
aws cloudformation deploy \
    --template-file cloudformation/template.yaml \
    --stack-name "$STACK_NAME" \
    --region "$REGION" \
    --parameter-overrides \
        KeyName="$KEY_NAME" \
        FrontendBucket="$S3_BUCKET" \
    --capabilities CAPABILITY_IAM \
    --no-fail-on-empty-changeset

# 5. Obter outputs
echo "[5/5] Obtendo informações do deploy..."
echo ""
echo "=== Deploy concluído ==="
aws cloudformation describe-stacks \
    --stack-name "$STACK_NAME" \
    --region "$REGION" \
    --query 'Stacks[0].Outputs' \
    --output table

echo ""
echo "Frontend: $FRONTEND_URL"
echo ""
echo "Próximos passos:"
echo "  1. SSH no EC2: ssh -i ${KEY_NAME}.pem ec2-user@<EC2_IP>"
echo "  2. Instalar Docker e rodar a API"
echo "  3. Atualizar API_URL no frontend/js/app.js com o IP/DNS do EC2"
echo "  4. Re-upload frontend: aws s3 sync ../frontend/ s3://$S3_BUCKET/ --delete"
