# Nuspec dependencies updater
This build task automatically updates all nuspec dependencies to the latest version found in any **packages.config** or **nuspec** file found in the given root directory.
It only changes the version if any of the used packages versions is newer than the one defined as nuspec dependency.

## Version from other nuspec files
This task also reads the versions of all found nuspec files and updates the version in nuspec files to the latest version found. 

This is extremely helpful if the version has just been changed by the build pipeline.

# But why?
Due to modularity our application is split into a lot of NuGet packages which we internally use for various different projects. This task makes it easy to make sure the latest used packages in the project are also defined as dependency of the build NuGet package.

# Example
![Example](https://i.imgur.com/dMxwyzb.png)
