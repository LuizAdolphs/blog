#!/bin/bash

rm -rf node_modules && \
docker build -t "blog" . && \
docker run --rm -it -p 4000:4000 -v $PWD/..:/app --entrypoint "/bin/bash" \
blog -c 'cp -a /running/node_modules /app/blog/ && cd /app/blog && /bin/bash'