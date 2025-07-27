FROM alpine:latest

RUN apk add --no-cache openssl

COPY init-certs.sh /usr/local/bin/init-certs.sh

RUN sed -i 's/\r$//' /usr/local/bin/init-certs.sh
RUN chmod +x /usr/local/bin/init-certs.sh

ENTRYPOINT ["/usr/local/bin/init-certs.sh"]