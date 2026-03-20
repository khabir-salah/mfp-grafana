Place your SSL certificate files here:
  - fullchain.pem
  - privkey.pem

Obtain them with Certbot:
  sudo certbot certonly --standalone -d yourdomain.com
  sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem ./fullchain.pem
  sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem   ./privkey.pem

Then uncomment the HTTPS server block in nginx/conf.d/default.conf
