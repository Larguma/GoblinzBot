#!/bin/sh

# Set the version of the app: major.minor.patch
# Change major if breaking changes
# Change minor if new features
# Change patch if bug fixes
appversion="1.3.3"

echo "Checking branches..."
if git rev-parse --abbrev-ref HEAD | grep -iq "main" ;then
  echo "Current branch is main, continuing..."
else
  echo "You're not on 'main' please merge or checkout main"
  exit 1
fi

echo -n "Commit all your changes before continuing... "
read

echo "Release version $appversion"

echo -n "Is this the correct version? (y/n) "
read answer

if echo "$answer" | grep -iq "^n" ;then
  echo "Aborting"
  exit 1
fi

echo "Building and pushing the docker images"

# Build and push the docker image for the goblinzbot : latest
docker build -t larguma/goblinzbot:latest GoblinzBot/
docker push larguma/goblinzbot:latest

# Build and push the docker image for the goblinzbot : version x.x.x
docker build -t larguma/goblinzbot:$appversion GoblinzBot/
docker push larguma/goblinzbot:$appversion

echo "Done"

echo "Tagging the release in git"

# Tag the release in git
git tag -a v$appversion -m "Release version $appversion"

echo -n "Would you like to push now? (y/n) "
read answer

if echo "$answer" | grep -iq "^n" ;then
  echo "Aborting"
  exit 1
else
  echo "Pushing"
  git push origin v$appversion
  echo "Done"
  exit 1
fi
