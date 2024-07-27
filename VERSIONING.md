# Versioning

Versioning handled by MinVer - https://github.com/adamralph/minver

It's based on tags and commit history

## Version order samples
```
1.0.0 < 2.0.0 < 2.1.0 < 2.1.1

1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0

```


## How to start a new version?

Add tag of pre-release version

```
git tag 1.1.0-alpha.0
git push --tags
```

## How to finalize version

On master or develop branch - add tag of needed version

```
git tag 1.1.0
git push --tags
```