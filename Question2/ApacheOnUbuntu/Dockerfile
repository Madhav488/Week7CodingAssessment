# Use a recent Ubuntu base image
FROM ubuntu:22.04

# Prevent interactive prompts during apt operations
ENV DEBIAN_FRONTEND=noninteractive

# Update package lists, install apache2, then clean apt cache to reduce image size
RUN apt-get update \
    && apt-get install -y apache2 \
    && rm -rf /var/lib/apt/lists/*

# Provide a simple home page
RUN bash -lc 'echo "<!doctype html><html><head><meta charset=\"utf-8\"><title>Apache on Ubuntu</title></head><body><h1>Apache on Ubuntu (Docker)</h1><p>Served from container</p></body></html>" > /var/www/html/index.html'

# Expose container port 80 (Apache default)
EXPOSE 80

# Run Apache in the foreground so the container keeps running
CMD ["apachectl", "-D", "FOREGROUND"]
