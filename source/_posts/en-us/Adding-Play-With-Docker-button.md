---
title: Adding Play With Docker button
date: 2018-11-04 21:03:35
tags: [docker]
---

Hello folks!

It has been a while that I didn't write anything, but whit sunday I found something very nice to share about inside [Oren Yomtov's Medium article](https://medium.com/@patternrecognizer/how-to-add-a-try-in-play-with-docker-button-to-your-github-project-41cb65721e94).

I'm a huge fan of the [Play With Docker](https://labs.play-with-docker.com/) (PWD) for at least two big reasons:

1. You can try images and softwares very quickly since you don't have to install Docker, docker-compose or even kubernetes (try [Play With Kubernetes](https://labs.play-with-k8s.com/)).
2. Computer resources can be spared, so your machine will not handle any heavy worload (or deal with virtualization configuration).

It's possible to try almost any software inside PWD (maybe all of them, I don't have sure, but still it's a lot!). But one limitation is that one person should know how to clone repos, run docker images and so on...

But now PWD can read an online `Dockerfile` or `docker-compose` file within GitHub, for example. So I decided to try it out.

I have my [DiffTool proof-of-concept](https://github.com/LuizAdolphs/DiffTool) project inside GitHub, which uses `docker-compose` to up and run two containers (one is the API service, other is UI web app service). My `docker-compose.yml` file was like that:

```yaml
version: '3'
services:
  api:
    build: ./api
    ports:
      - "5000:5000"
  web:
    build: ./app
    ports:
      - "80:80"
```

Where I was configuring two services using distinct Dockerfiles. This repository was perfect to test PWD on-the-fly button.

But, after some tentatives (where you can find [here](https://github.com/LuizAdolphs/DiffTool/commits/master)), I found out that was simpler to build and pull both images to Docker repository and run them with a docker compose file.

So I adjust my `.travis.yml` to build and pull my docker images:

```yaml
    - language: generic
      dist: trusty
      sudo: true
      script:
      - echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
      - docker build -t difftoolapi ./api
      - docker images
      - docker tag difftoolapi $DOCKER_USERNAME/difftoolapi
      - docker push $DOCKER_USERNAME/difftoolapi
      - docker build -t difftoolapp ./app
      - docker images
      - docker tag difftoolapp $DOCKER_USERNAME/difftoolapp
      - docker push $DOCKER_USERNAME/difftoolapp
```

Whole file:

```yaml
matrix:
  include:
    - language: csharp
      dist: trusty
      mono: none
      dotnet: 2.0.0
      install:
        - cd api/
        - dotnet restore
      script:
        - cd ../test/api/
        - dotnet build
        - dotnet test 
    - language: node_js
      dist: trusty
      node_js:
        - "9"
      install:
        - cd app/
        - yarn install
      script:
        - yarn test 
    - language: generic
      dist: trusty
      sudo: true
      script:
      - echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
      - docker build -t difftoolapi ./api
      - docker images
      - docker tag difftoolapi $DOCKER_USERNAME/difftoolapi
      - docker push $DOCKER_USERNAME/difftoolapi
      - docker build -t difftoolapp ./app
      - docker images
      - docker tag difftoolapp $DOCKER_USERNAME/difftoolapp
      - docker push $DOCKER_USERNAME/difftoolapp
```

As you can see, I made use of the [generic language image](https://docs.travis-ci.com/user/languages/minimal-and-generic/) to have Docker CLI and [Travis enviroment variables](https://docs.travis-ci.com/user/environment-variables/) to keep my docker username and password in secret. The whole idea was took from [here](https://docs.travis-ci.com/user/build-stages/share-docker-image/).

Now, after any master branch pull is made, a newer release of that image is pulled too.

So now I'm able to write a new `.yml` file exclusive to PWD, which I've called `pwd-docker-compose.yml`:

```yaml
version: '3'
services:
  api:
    image: "adolphsluiz/difftoolapi:latest"
    ports:
      - "5000:5000"
  web:
    image: "adolphsluiz/difftoolapp:latest"
    ports:
      - "80:80"
```

The last step is to add the markdown code with the button link in README.md:

```markdown
[![Try in PWD](https://raw.githubusercontent.com/play-with-docker/stacks/master/assets/images/button.png)](https://labs.play-with-docker.com/?stack=https://raw.githubusercontent.com/LuizAdolphs/DiffTool/master/pwd-docker-compose.yml)
```

Where it says `https://raw.githubusercontent.com/LuizAdolphs/DiffTool/master/pwd-docker-compose.yml` you must replace with your compose public URL.

And this is the result:

![PWD button in README github page](/images/button-pwd.jpg)

Now just click in the button to be redirected to PWD webpage (which probably you need to logon using a docker account) running the repository stack!

That's it! Thanks for all referenced folks and webpages in this article! See you next time!