Maintaining Math.NET Numerics
=============================

*Note: This document is only relevant for the maintainers of this project*

When creating a new release
---------------------------

Repository:

- Update RELEASENOTES file with relevant changes, attributed by contributor (if external). Set date.
- Update CONTRIBUTORS file (via `git shortlog -sn`)
- Create and publish a new annotated git tag for the release.
- Consider to resync the subtree in the native provider repository
- Consider to update all the repository mirrors (codeplex, gitorious, google)

Publish:

- Generate manual release build from internal TeamCity
- Update release notes in NuGet packages (using package explorer)
- Create new codeplex release, attach Zip files
- Upload NuGet packages to the NuGet Gallery

Misc:

- Consider a tweet via @MathDotNet
- Consider a post to the Google+ site
- Update Wikipedia release version+date for the [Math.NET Numerics](http://en.wikipedia.org/wiki/Math.NET_Numerics) and [Comparison of numerical analysis software](http://en.wikipedia.org/wiki/Comparison_of_numerical_analysis_software) articles.
- Regenerate api reference (at numerics.mathdotnet.com/api) using [docu](https://github.com/cdrnet/docu) and deploy to website.
- Update documentation if necessary
- Consider blog post about changes
