docker build -t "blog" . && \
docker run --rm -it -p 4000:4000 -v $PWD/..:/app --entrypoint "/bin/bash" \
blog -c 'cp -a /app/blog/. /running/ && hexo clean && hexo generate && hexo deploy && cp -a /running/public/. /app/luizadolphs/'