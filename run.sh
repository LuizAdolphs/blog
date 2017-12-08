docker build -t "blog" . && \
docker run --rm -it -p 4000:4000 -v $PWD/..:/app -w ="/app/blog" blog