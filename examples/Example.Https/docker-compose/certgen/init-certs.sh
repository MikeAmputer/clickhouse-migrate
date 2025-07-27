#!/bin/sh

set -e

CONTAINER_NAME="${CONTAINER_NAME:?Environment variable CONTAINER_NAME is required}"
USER_NAME="${USER_NAME:?Environment variable USER_NAME is required}"
CERT_DIR="${CERT_DIR:?Environment variable CERT_DIR is required}"
CA_DIR="${CA_DIR:?Environment variable CA_DIR is required}"

mkdir -p "$CERT_DIR"
mkdir -p "$CA_DIR"

CA_KEY="$CERT_DIR/ca.key"
CA_CERT="$CERT_DIR/ca.pem"

SERVER_KEY="$CERT_DIR/server.key"
SERVER_CERT="$CERT_DIR/server.pem"
SERVER_CSR="$CERT_DIR/server.csr"

CLIENT_KEY="$CERT_DIR/client.key"
CLIENT_CERT="$CERT_DIR/client.pem"
CLIENT_CSR="$CERT_DIR/client.csr"

SSL_CONF="$CERT_DIR/req.conf"

DHPARAM="$CERT_DIR/dhparam.pem"

cat > "$SSL_CONF" <<EOF
[ req ]
distinguished_name = req_distinguished_name
x509_extensions = v3_ca
prompt = no

[ req_distinguished_name ]
C = CN
ST = GD
O = ${CONTAINER_NAME}
CN = ${USER_NAME}

[ v3_ca ]
basicConstraints = critical,CA:TRUE
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always,issuer:always

[ v3_req ]
keyUsage = keyEncipherment, dataEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[ alt_names ]
DNS.1 = ${CONTAINER_NAME}
DNS.2 = localhost
IP.1 = 127.0.0.1
EOF

# CA
if [ ! -f "$CA_CERT" ]; then
  echo "Creating CA certificate..."
  openssl genrsa -out "$CA_KEY" 2048
  openssl req -x509 -new -key "$CA_KEY" -days 365 \
    -out "$CA_CERT" -extensions 'v3_ca' -config "$SSL_CONF"
fi

# Server
if [ ! -f "$SERVER_CERT" ]; then
  echo "Creating server certificate..."
  openssl genrsa -out "$SERVER_KEY" 2048
  openssl req -new -sha256 -key "$SERVER_KEY" -out "$SERVER_CSR" \
    -subj "/C=CN/ST=GD/O=$CONTAINER_NAME/CN=$USER_NAME"
  openssl x509 -req -days 365 -sha256 -extfile "$SSL_CONF" -extensions v3_req \
    -CA "$CA_CERT" -CAkey "$CA_KEY" -CAcreateserial \
    -in "$SERVER_CSR" -out "$SERVER_CERT"
fi

# Client
if [ ! -f "$CLIENT_CERT" ]; then
  echo "Creating client certificate..."
  openssl genrsa -out "$CLIENT_KEY" 2048
  openssl req -new -sha256 -key "$CLIENT_KEY" -out "$CLIENT_CSR" \
    -subj "/C=CN/ST=GD/O=$CONTAINER_NAME/CN=$USER_NAME"
  openssl x509 -req -days 365 -sha256 -extfile "$SSL_CONF" -extensions v3_req \
    -CA "$CA_CERT" -CAkey "$CA_KEY" -CAcreateserial \
    -in "$CLIENT_CSR" -out "$CLIENT_CERT"
fi

# dhparam
if [ ! -f "$DHPARAM" ]; then
  echo "Creating dhparam..."
  openssl dhparam -out "$DHPARAM" 4096
fi

chmod 755 "$CERT_DIR"/*
chmod 755 "$CERT_DIR"

# Copy ca.pem
if [ ! -f "$CA_DIR/ca.crt" ]; then
  echo "Copy ca.pem..."
  cp "$CA_CERT" "$CA_DIR/ca.crt"
fi

chmod 644 "$CA_DIR"/*
chmod 644 "$CA_DIR"