FROM node:9.2.0

RUN npm install -g hexo-cli

RUN mkdir /running

COPY package.json /running

WORKDIR /running

RUN npm install

ENTRYPOINT ["/bin/bash", "-c","cp -a /app/blog/. /running/ && hexo server"]